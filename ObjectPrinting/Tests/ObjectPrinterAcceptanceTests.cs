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
		        .Printing(p => p.Name).TrimmedToLength(1)
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
        public void PrintObjects()
        {
            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);
            TestContext.WriteLine(result);
            var expectedStr = "Person\r\n	Id = Guid\r\n	Name = Alex\r\n	" +
                              "Height = 167,8\r\n	Age = 16\r\n	Parent = null\r\n";
            result.Should().Be(expectedStr);
        }

        [Test]
        public void SetCorrectNesting()
        {
            var parent = new Person {Name = "Ivan", Age = 54, Height = 168.9};
            person = new Person {Name = "Anna", Age = 17, Height = 178.8, Parent = parent};

            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);
            TestContext.WriteLine(result);
            var expectedStr = "Person\r\n" +
                              "\tId = Guid\r\n" +
                              "\tName = Anna\r\n" +
                              "\tHeight = 178,8\r\n" +
                              "\tAge = 17\r\n" +
                              "\tParent = Person\r\n" +
                              "\t\tId = Guid\r\n" +
                              "\t\tName = Ivan\r\n" +
                              "\t\tHeight = 168,9\r\n" +
                              "\t\tAge = 54\r\n" +
                              "\t\tParent = null\r\n";
            result.Should().Be(expectedStr);
        }

        [Test]
        public void CorrectlyPrintNullObjects()
        {
            person = new Person();
            var result = ObjectPrinter.For<Person>()
                .PrintToString(person);
            TestContext.WriteLine(result);
            var expectedStr = "Person\r\n	Id = Guid\r\n	Name = null\r\n" +
                              "	Height = 0\r\n	Age = 0\r\n	Parent = null\r\n";
            result.Should().Be(expectedStr);
        }

        [Test]
        public void Throw_OutOfRangeException_WhenTrimmedString_LessThenZero()
        {
            var result = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(-1);
            var action = new Action(()=>result.PrintToString(person));
            action.ShouldThrow<ArgumentOutOfRangeException>();
        }

        [Test]
        public void Throw_OutOfRangeException_WhenTrimmedString_LongerThanRawString()
        {
            var result = ObjectPrinter
                .For<Person>()
                .Printing(p => p.Name)
                .TrimmedToLength(15);
            var action = new Action(() => result.PrintToString(person));
            action.ShouldThrow<ArgumentOutOfRangeException>();
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
            var expectedStr = "Person\r\n	Id = Guid\r\n	Name = Alex\r\n	Age = 16\r\n	Parent = null\r\n";
            result.Should().Be(expectedStr);
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