using System;
using System.Globalization;
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
		        .Printing<int>().Using(CultureInfo.InvariantCulture)
		        //4. Настроить сериализацию конкретного свойства
		        .Printing(p=>p.Age).Using(age => age.ToString())
				//5. Настроить обрезание строковых свойств (метод должен быть виден только для строковых свойств)
		        .Printing(p => p.Name).TrimmedToLength(10)
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
}