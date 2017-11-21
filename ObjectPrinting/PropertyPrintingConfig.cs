using System;
using System.Globalization;
using NUnit.Framework.Internal;

namespace ObjectPrinting
{
    public class PropertyPrintingConfig<TOwner, TPropType> : IPropertyPrintingConfig<TOwner, TPropType>
    {
        private readonly PrintingConfig<TOwner> printingConfig;

        private readonly string propName;

        public PropertyPrintingConfig(PrintingConfig<TOwner> printingConfig, string propName = null)
        {
            this.printingConfig = printingConfig;
            this.propName = propName;
        }

        public PrintingConfig<TOwner> Using(Func<TPropType,string> serializeFunc)
        {
            if(propName==null)
                printingConfig.AddCustomSerializator(typeof(TPropType), serializeFunc);
            else
                printingConfig.AddCustomSerializator(propName, serializeFunc);
            return printingConfig;
        }

        PrintingConfig<TOwner> IPropertyPrintingConfig<TOwner, TPropType>.ParentConfig => printingConfig;
    }

    public interface IPropertyPrintingConfig<TOwner, TPropType>
    {
        PrintingConfig<TOwner> ParentConfig { get; }
    }

    public static class PropertyPrintingConfigExtension
    {
        public static PrintingConfig<TOwner> TrimmedToLength<TOwner>(this PropertyPrintingConfig<TOwner, string> propConfig, int maxLen)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, string>) propConfig).ParentConfig;
            parentConfig.MaxStringLength = maxLen;
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, int> propConfig,CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, int>)propConfig).ParentConfig;
            parentConfig.UseCulture(typeof(int), culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, double> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, double>)propConfig).ParentConfig;
            parentConfig.UseCulture(typeof(double),culture);
            return parentConfig;
        }

        public static PrintingConfig<TOwner> Using<TOwner>(this PropertyPrintingConfig<TOwner, float> propConfig, CultureInfo culture)
        {
            var parentConfig = ((IPropertyPrintingConfig<TOwner, float>)propConfig).ParentConfig;
            parentConfig.UseCulture(typeof(float), culture);
            return parentConfig;
        }
    }


}