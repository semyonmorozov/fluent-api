using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using FluentAssertions;

namespace ObjectPrinting
{
    public class PrintingConfig<TOwner>
    {
        private readonly List<Type> excludedTypes = new List<Type>();
        private readonly HashSet<string> excludedProperties = new HashSet<string>();
        private readonly Dictionary<Type, CultureInfo> usedCultures = new Dictionary<Type, CultureInfo>();
        private readonly Dictionary<Type, Delegate> typeSerializators = new Dictionary<Type, Delegate>();
        private readonly Dictionary<string, Delegate> propSerializators = new Dictionary<string, Delegate>();
        public int? MaxStringLength { get; set; }
        private readonly List<Type> finalTypes = new List<Type>
        {
            typeof(int), typeof(double), typeof(float), typeof(string),
            typeof(DateTime), typeof(TimeSpan)
        };

        public PrintingConfig()
        {
            MaxStringLength = null;
        }

        public string PrintToString(TOwner obj)
        {
            return PrintToString(obj, 0);
        }

        private string PrintToString(object obj, int nestingLevel)
        {
            if (obj == null)
                return "null" + Environment.NewLine;

            if (obj is string s && MaxStringLength != null)
                return s.Substring(0, MaxStringLength.Value) + Environment.NewLine;
            if (finalTypes.Contains(obj.GetType()))
                return obj + Environment.NewLine;

            var identation = new string('\t', nestingLevel + 1);
            var sb = new StringBuilder();
            var type = obj.GetType();
            sb.AppendLine(type.Name);
            foreach (var propertyInfo in type.GetProperties())
            {
                if (excludedTypes.Contains(propertyInfo.PropertyType)) continue;
                if (excludedProperties.Contains(propertyInfo.Name)) continue;
                var serializedObj =Serialize(obj, propertyInfo, nestingLevel);
                sb.Append(identation + propertyInfo.Name + " = " + serializedObj);
            }
            return sb.ToString();
        }

        private string Serialize(object obj, PropertyInfo propertyInfo, int nestingLevel)
        {
            var value = propertyInfo.GetValue(obj);
            if (typeSerializators.ContainsKey(propertyInfo.PropertyType))
                return typeSerializators[propertyInfo.PropertyType].DynamicInvoke(value) + Environment.NewLine;
            if (usedCultures.ContainsKey(propertyInfo.PropertyType))
                return ((IFormattable)value).ToString(null, usedCultures[propertyInfo.PropertyType]) + Environment.NewLine;
            return PrintToString(value, nestingLevel + 1);
        }

        public PropertyPrintingConfig<TOwner,TPropType> Printing<TPropType>()
        {
            return new PropertyPrintingConfig<TOwner, TPropType>(this);
        }

        public PropertyPrintingConfig<TOwner, TPropType> Printing<TPropType>(Expression<Func<TOwner,TPropType>> memberSelector)
        {
            var propName = GetPropertyInfo(memberSelector).Name;
            return new PropertyPrintingConfig<TOwner, TPropType>(this, propName);
        }
        
        public PrintingConfig<TOwner> Excluding<TPropType>(Expression<Func<TOwner, TPropType>> memberSelector)
        {
            var propInfo = GetPropertyInfo(memberSelector);
            excludedProperties.Add(propInfo.Name);
            return this;
        }

        public PrintingConfig<TOwner> Excluding<TPropType>()
        {
            excludedTypes.Add(typeof(TPropType));
            return this;
        }

        private static PropertyInfo GetPropertyInfo<TPropType>(Expression<Func<TOwner, TPropType>> propertyExtractor) =>
            ((MemberExpression) propertyExtractor.Body).Member as PropertyInfo;

        public void UseCulture(Type type, CultureInfo culture)
        {
            usedCultures[type]=culture;
        }

        public void AddCustomSerializator(Type type, Delegate serializeFunc)
        {
            typeSerializators[type] = serializeFunc;
        }

        public void AddCustomSerializator(string propName, Delegate serializeFunc)
        {
            propSerializators[propName] = serializeFunc;
        }
    }

    
}