using System;
using System.Globalization;
using FluentAssertions;
using NUnit.Framework;

namespace ObjectPrinting.Tests
{
	[TestFixture]
	public class ObjectPrinterAcceptanceTests
	{
		[Test]
		public void Demo()
		{
			var person = new Person { Name = "Alex", Age = 19, Height = 167.8};

		    var printer = ObjectPrinter.For<Person>()
		        //1. Исключить из сериализации свойства определенного типа
		        .Excluding<Guid>()
		        //2. Указать альтернативный способ сериализации для определенного типа
		        .Printing<int>().Using(i => i.ToString("x"))
		        //3. Для числовых типов указать культуру
		        .Printing<double>().Using(CultureInfo.InvariantCulture)
		        //4. Настроить сериализацию конкретного свойства
		        .Printing(p=>p.Age).Using(age => age.ToString())
				//5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
		        .Printing(p => p.Name).TrimmedToLength(2)
                //6. Исключить из сериализации конкретного свойства
		        .Excluding(p => p.Height);

            string s1 = printer.PrintToString(person);

            //7. Синтаксический сахар в виде метода расширения, сериализующего по-умолчанию		
		    string s2 = person.PrintToString();
            //8. ...с конфигурированием
		    string s3 = person.PrintToString(s => s.Excluding<Guid>().Excluding(p => p.Age));

		    TestContext.WriteLine(s1);
		    TestContext.WriteLine(s2);
		    TestContext.WriteLine(s3);
        }
	}

    [TestFixture]
    public class ObjectPrinter_Should
    {
        private Person person;
        [SetUp]
        public void SetUp()
        {
            person = new Person { Name = "Alex", Age = 16, Height = 167.8 };
        }

        [Test]
        public void ExcludeType()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding<Guid>()
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().NotContain("Id");
        }

        [Test]
        public void ExcludeProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Excluding(p => p.Height)
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().NotContain("Height");
            
        }

        [Test]
        public void UseCustomSerializer_ForType()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<int>()
                .Using(i => i+" year")
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().Contain("16 year");
        }

        [Test]
        public void UseCustomSerializer_ForProperty()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Height)
                .Using(age => age+" sm")
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().Contain("167,8 sm");
        }

        [Test]
        public void UseCustomCulture_ForNums()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing<double>()
                .Using(CultureInfo.InvariantCulture)
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().Contain("167.8");
        }

        [Test]
        public void TrimString()
        {
            var result = ObjectPrinter.For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(2)
                .PrintToString(person);
            TestContext.WriteLine(result);
            result.Should().Contain("Al\r");
        }
    }
}