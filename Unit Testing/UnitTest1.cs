using Akka.Util;
using CustomJSON;
using CustomJSONSerializer;
using CustomSerializer;
using Unit_Class_Testing;
using Xunit;
using Xunit.Abstractions;
using static Akka.IO.Tcp;

namespace Unit_Testing
{
    public class UnitTest1
    {

        private readonly ITestOutputHelper _output;

        public UnitTest1(ITestOutputHelper output)
        {
            _output = output;
        }

        public static bool AreEqualIgnoringWhitespace(string? str1, string? str2)
        {
            if (str1 == null || str2 == null) return str1 == str2;

            string normalized1 = string.Concat(str1.Where(c => !char.IsWhiteSpace(c)));
            string normalized2 = string.Concat(str2.Where(c => !char.IsWhiteSpace(c)));

            return normalized1 == normalized2;
        }

        [Fact]
        public void Serialize_Struct_ReturnsValidString()
        {
            MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(Dog));

            if (myserializer == null) return;

            Dog dog = new Dog
            {
                Age = 10,
                Name = "bobby"
            };

            using (var writer = new StringWriter())
            {
                if (!myserializer.Serialize(writer, dog, true))
                    Assert.Fail();

                _output.WriteLine(writer.ToString());

                Assert.True(AreEqualIgnoringWhitespace(
                            "{\"Name\":\"bobby\",\"Age\":10}",
                            writer.ToString()));

            }
        }

        [Fact]
        public void Serialize_EnumToString_ReturnsValidString()
        {

        MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(AccountStatus));

            if (myserializer == null) return;

            AccountStatus accountStatus = AccountStatus.Active;

            using (var writer = new StringWriter())
            {
                if (!myserializer.Serialize(writer, accountStatus))
                    Assert.Fail();

                _output.WriteLine(writer.ToString());

                Assert.True(AreEqualIgnoringWhitespace(
                            "{\"Type\":\"AccountStatus\",\"Data\":{\"value__\":0,\"Active\":\"Active\",\"Suspended\":\"Suspended\",\"Deleted\":\"Deleted\"}}",
                            writer.ToString()));
            }
        }

        [Fact]
        public void Serialize_EnumToNum_ReturnsValidString()
        {

            MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(AccountStatus));

            if (myserializer == null) return;

            AccountStatus accountStatus = AccountStatus.Deleted;

            using (var writer = new StringWriter())
            {
                if (!myserializer.Serialize(writer, accountStatus, JSONIgnoreTargets.StringEnum))
                    Assert.Fail();

                _output.WriteLine(writer.ToString());

                Assert.True(AreEqualIgnoringWhitespace(
                            "{\"Type\":\"AccountStatus\",\"Data\":{\"value__\":2,\"Active\":0,\"Suspended\":1,\"Deleted\":2}}",
                            writer.ToString()));

            }
        }

        [Fact]
        public void Serialize_Product_ReturnsValidString()
        {

            string expected = "{\r\n  \"Type\":\"Product\",\r\n  \"Data\":{\r\n    \"ProductId\":\"11111111-1111-1111-1111-111111111111\",\r\n    \"Label\":\"Laptop\",\r\n    \"Description\":\"High-end gaming laptop\",\r\n    \"Price\":1499.99,\r\n    \"SKU\":\"EL-2023\",\r\n    \"IsAvailable\":true,\r\n    \"Categories\":[\"catElectronics\",\"catGaming\"],\r\n    \"Features\":[\"featureRGB\",\"featureCooling\"],\r\n    \"Ratings\":[5,4,4,5],\r\n    \"Metadata\":{\r\n        \"Warranty\":\"2 years\",\r\n        \"Color\":\"Black\"\r\n    }\r\n  }\r\n}\r\n\r\n";
            MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(Product));

            if (myserializer == null) return;

            Product product1 = new Product
            {
                ProductId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "Laptop",
                Description = "High-end gaming laptop",
                Price = 1499.99,
                SKU = "EL-2023",
                IsAvailable = true,
                InternalNotes = "Check supplier ID 3345",
                Categories = new List<string> { "catElectronics", "catGaming" },
                Features = new object[] { "featureRGB", "featureCooling" },
                Ratings = new int[] { 5, 4, 4, 5 },
                Metadata = new { Warranty = "2 years", Color = "Black" }
            };




            using (var writer = new StringWriter())
            {
                if (!myserializer.Serialize(writer, product1))
                    Assert.Fail();

                _output.WriteLine(writer.ToString());

                Assert.True(AreEqualIgnoringWhitespace(
                            expected,
                            writer.ToString()));

            }
        }

        [Fact]
        public void Serialize_RootObject_ReturnsValidString()
        {

            string expected = " {\r\n  \"Type\":\"RootObject\",\r\n  \"Data\":{\r\n    \"String\":\"example\",\r\n    \"Number\":12345,\r\n    \"Float\":123.456,\r\n    \"BooleanTrue\":true,\r\n    \"BooleanFalse\":false,\r\n    \"NullValue\":null,\r\n    \"EmptyArray\":[],\r\n    \"ArrayOfNulls\":[null,null,null],\r\n    \"ArrayOfPrimitives\":[\"a\",1,false],\r\n    \"ArrayOfArrays\":[[1,2],[\"x\",\"y\"],[true,false]],\r\n    \"ArrayOfObjects\":[{\r\n        \"Id\":1,\r\n        \"Name\":\"Alice\",\r\n        \"Tags\":null,\r\n        \"Details\":null\r\n    },{\r\n        \"Id\":2,\r\n        \"Name\":\"Bob\",\r\n        \"Tags\":[\"dev\",\"qa\"],\r\n        \"Details\":null\r\n    },{\r\n        \"Id\":3,\r\n        \"Name\":null,\r\n        \"Tags\":null,\r\n        \"Details\":{\r\n            \"Joined\":\"2020-01-01\",\r\n            \"Active\":true\r\n        }\r\n    }],\r\n    \"NestedObject\":{\r\n        \"Level1\":{\r\n            \"Level2\":{\r\n                \"Level3\":{\r\n                    \"Value\":\"deep\",\r\n                    \"List\":[1,2,3],\r\n                    \"Child\":{\r\n                        \"Key\":\"value\",\r\n                        \"Final\":true\r\n                    }\r\n                }\r\n            }\r\n        }\r\n    },\r\n    \"Dictionary\":{\r\n        \"Meta\":{\r\n            \"Author\":\"mark\",\r\n            \"Editor\":\"john\"\r\n        },\r\n        \"Title\":\"book\",\r\n        \"Tags\":[\"novel\",\"mystery\",\"classic\"],\r\n        \"Published\":{\r\n            \"Year\":1999,\r\n            \"Languages\":[\"en\",\"fr\",\"ar\"]\r\n        },\r\n        \"AnonymObject\":{\r\n            \"Years\":\"2 years\",\r\n            \"Color\":\"Green\",\r\n            \"Data\":{\r\n                \"Ids\":[101,102,103],\r\n                \"EmptyDict\":{\r\n\r\n                },\r\n                \"DeepNulls\":{\r\n                  \"one\":null,\r\n                  \"two\":null\r\n                }\r\n            }\r\n        }\r\n    },\r\n    \"SpecialCharacters\":{\r\n        \"Quote\":\"\"double quote\"\",\r\n        \"Backslash\":\"\\\",\r\n        \"Newline\":\"line\\nbreak\",\r\n        \"Tab\":\"tab\\tspace\",\r\n        \"UnicodeSmile\":\"☺\",\r\n        \"Arabic\":\"مرحبا\",\r\n        \"Chinese\":\"你好\",\r\n        \"Emoji\":\"💡\"\r\n    }\r\n  }\r\n}";

            var testObject = new RootObject
            {
                String = "example",
                Number = 12345,
                Float = 123.456,
                BooleanTrue = true,
                BooleanFalse = false,
                NullValue = null,

                EmptyArray = new(),
                ArrayOfNulls = new List<object?> { null, null, null },
                ArrayOfPrimitives = new() { "a", 1, false },
                ArrayOfArrays = new()
                {
                    new List<object?> { 1, 2 },
                    new List<object?> { "x", "y" },
                    new List<object?> { true, false }
                },
                ArrayOfObjects = new()
                {
                    new ArrayObject { Id = 1, Name = "Alice" },
                    new ArrayObject { Id = 2, Name = "Bob", Tags = new() { "dev", "qa" } },
                    new ArrayObject
                    {
                        Id = 3,
                        Details = new Details
                        {
                            Joined = "2020-01-01",
                            Active = true
                        }
                    }
                },
                NestedObject = new NestedObject
                {
                    Level1 = new Level1
                    {
                        Level2 = new Level2
                        {
                            Level3 = new Level3
                            {
                                Value = "deep",
                                List = new() { 1, 2, 3 },
                                Child = new Child
                                {
                                    Key = "value",
                                    Final = true
                                }
                            }
                        }
                    }
                },
                Dictionary = new DictionarySection
                {
                    Meta = new Meta { Author = "mark", Editor = "john" },
                    Title = "book",
                    Tags = new() { "novel", "mystery", "classic" },
                    Published = new Published
                    {
                        Year = 1999,
                        Languages = new() { "en", "fr", "ar" }
                    },
                    AnonymObject = new AnonymObject
                    {
                        Years = "2 years",
                        Color = "Green",
                        Data = new Data
                        {
                            Ids = new() { 101, 102, 103 },
                            EmptyDict = new(),
                            DeepNulls = new()
                {
                    { "one", null },
                    { "two", null }
                }
                        }
                    }
                },
                SpecialCharacters = new SpecialCharacters
                {
                    Quote = "\"double quote\"",
                    Backslash = "\\",
                    Newline = "line\nbreak",
                    Tab = "tab\tspace",
                    UnicodeSmile = "\u263A",
                    Arabic = "مرحبا",
                    Chinese = "你好",
                    Emoji = "💡"
                }
            };

            MyJSONSerializer MySerializer = MyJSONSerializer.CreateInstance(typeof(RootObject));
            using (var writer = new StringWriter())
            {
                MySerializer.Serialize(writer, testObject);

                _output.WriteLine(writer.ToString());

                Assert.True(AreEqualIgnoringWhitespace(writer.ToString(), expected));

            }
        }

        [Fact]
        public void Serialize_ComplexUser_ReturnsValidString()
        {
            string expected = "{\r\n  \"UserId\": \"12345678-1234-1234-1234-1234567890ab\",\r\n  \"LoginName\": \"maamar\",\r\n  \"Email\": \"maamar@example.com\",\r\n  \"PhoneNumber\": \"+213123456789\",\r\n  \"Addresses\": [\r\n    {\r\n      \"Street\": \"123 Code Street\",\r\n      \"City\": \"DevTown\",\r\n      \"State\": \"Algiers\",\r\n      \"PostalCode\": \"16000\",\r\n      \"Country\": \"DZ\"\r\n    }\r\n  ],\r\n  \"Settings\": {\r\n    \"theme\": {\r\n      \"IsEnabled\": true,\r\n      \"Limit\": 50,\r\n      \"TimeWindow\": \"08:00\",\r\n      \"Parameters\": {\r\n        \"darkMode\": true,\r\n        \"fontSize\": \"medium\"\r\n      }\r\n    }\r\n  },\r\n  \"Tags\": [\"admin\", \"tester\"],\r\n  \"Roles\": [\"Root\", \"PowerUser\"],\r\n  \"Status\": \"Active\",\r\n  \"Metadata\":null\r\n}\r\n";

            var user = new ComplexUser
            {
                UserId = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
                Username = "maamar",
                Email = "maamar@example.com",
                PhoneNumber = "+213123456789",
                InternalSecret = "TOP_SECRET",
                Addresses = new List<UserAddress>
                {
                    new UserAddress
                    {
                        Street = "123 Code Street",
                        City = "DevTown",
                        State = "Algiers",
                        PostalCode = "16000",
                        Country = "DZ",
                        Notes = "Don't serialize me"
                    }
                },
                Settings = new Dictionary<string, UserSetting>
                {
                    ["theme"] = new UserSetting
                    {
                        IsEnabled = true,
                        Limit = 50,
                        TimeWindow = "08:00",
                        Parameters = new Dictionary<string, object>
                        {
                            ["darkMode"] = true,
                            ["fontSize"] = "medium"
                        }
                    }
                },
                Tags = new List<string> { "admin", "tester" },
                Roles = new[] { "Root", "PowerUser" },
                Status = AccountStatus.Active,
                Metadata = null
            };

            MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(ComplexUser));

            using (var writer = new StringWriter())
            {
                if (!myserializer.Serialize(writer, user, true))
                    Assert.Fail();

                string res = writer.ToString();

                _output.WriteLine(res);


                Assert.True(AreEqualIgnoringWhitespace(
                                expected,
                                writer.ToString()));

            }
        }

    }
       
}