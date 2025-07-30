using CustomAttribute;


namespace Unit_Class_Testing
{

    public struct Dog
    {
        public string Name { get; set; }
        public int Age { get; set; }    
    }

    [MySerializable]
    public class Product
    {
        [MyRequired]
        [MyOrder(0)]
        public Guid? ProductId { get; set; }

        [MyDefaultValue("N/A")]
        [MyOrder(2)]
        public string Description { get; set; }

        [MyRequired]
        [MyOrder(1)]
        [MyNickName("Label")]
        public string Name { get; set; }

        [MyPattern("^[A-Z]{2}-\\d{4}$")]
        [MyOrder(4)]
        public string SKU { get; set; }

        [MyDefaultValue(0.0)]
        [MyOrder(3)]
        public double? Price { get; set; }

        [MyDefaultValue(true)]
        [MyOrder(5)]
        public bool? IsAvailable { get; set; }

        [MyNonSerializable]
        public string InternalNotes { get; set; }

        [MyPattern("^cat")]
        public List<string> Categories { get; set; }

        [MyPattern("^feat")]
        public object[] Features { get; set; }

        public int[] Ratings { get; set; }

        [MyDefaultValue(null)]
        public object Metadata { get; set; }   
    }
    public class RootObject
    {
        public string? String { get; set; }
        public int Number { get; set; }
        public double Float { get; set; }
        public bool BooleanTrue { get; set; }
        public bool BooleanFalse { get; set; }
        public object? NullValue { get; set; }

        public List<object?> EmptyArray { get; set; } = new();
        public List<object?> ArrayOfNulls { get; set; } = new();
        public List<object?> ArrayOfPrimitives { get; set; } = new();
        public List<List<object?>> ArrayOfArrays { get; set; } = new();
        public List<ArrayObject> ArrayOfObjects { get; set; } = new();

        public NestedObject NestedObject { get; set; } = new();
        public DictionarySection Dictionary { get; set; } = new();
        public SpecialCharacters SpecialCharacters { get; set; } = new();
    }

    public class ArrayObject
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<string>? Tags { get; set; }
        public Details? Details { get; set; }
    }

    public class Details
    {
        public string? Joined { get; set; }
        public bool Active { get; set; }
    }

    public class NestedObject
    {
        public Level1 Level1 { get; set; } = new();
    }

    public class Level1
    {
        public Level2 Level2 { get; set; } = new();
    }

    public class Level2
    {
        public Level3 Level3 { get; set; } = new();
    }

    public class Level3
    {
        public string? Value { get; set; }
        public List<int> List { get; set; } = new();
        public Child Child { get; set; } = new();
    }

    public class Child
    {
        public string? Key { get; set; }
        public bool Final { get; set; }
    }

    public class DictionarySection
    {
        public Meta Meta { get; set; } = new();
        public string? Title { get; set; }
        public List<string> Tags { get; set; } = new();
        public Published Published { get; set; } = new();
        public AnonymObject AnonymObject { get; set; } = new();
    }

    public class Meta
    {
        public string? Author { get; set; }
        public string? Editor { get; set; }
    }

    public class Published
    {
        public int Year { get; set; }
        public List<string> Languages { get; set; } = new();
    }

    public class AnonymObject
    {
        public string? Years { get; set; }
        public string? Color { get; set; }
        public Data Data { get; set; } = new();
    }

    public class Data
    {
        public List<int> Ids { get; set; } = new();
        public Dictionary<string, object?> EmptyDict { get; set; } = new();
        public Dictionary<string, object?> DeepNulls { get; set; } = new();
    }

    public class SpecialCharacters
    {
        public string? Quote { get; set; }
        public string? Backslash { get; set; }
        public string? Newline { get; set; }
        public string? Tab { get; set; }
        public string? UnicodeSmile { get; set; }
        public string? Arabic { get; set; }
        public string? Chinese { get; set; }
        public string? Emoji { get; set; }
    }

    [MySerializable]
    public class ComplexUser
    {
        [MyOrder(0)]
        [MyRequired]
        public Guid? UserId { get; set; }

        [MyOrder(1)]
        [MyNickName("LoginName")]
        public string Username { get; set; }

        [MyOrder(2)]
        [MyDefaultValue("user@domain.com")]
        public string Email { get; set; }

        [MyOrder(3)]
        [MyPattern(@"^\+?\d{10,15}$")]
        public string PhoneNumber { get; set; }

        [MyOrder(4)]
        [MyNonSerializable]
        public string InternalSecret { get; set; }

        [MyOrder(5)]
        public List<UserAddress> Addresses { get; set; }

        [MyOrder(6)]
        public Dictionary<string, UserSetting> Settings { get; set; }

        [MyOrder(7)]
        public List<string> Tags { get; set; }

        [MyOrder(8)]
        public string[] Roles { get; set; }

        [MyOrder(9)]
        public AccountStatus Status { get; set; }

        public object? Metadata { get; set; }
    }

    public enum AccountStatus
    {
        Active,
        Suspended,
        Deleted
    }

    public class UserAddress
    {
        [MyOrder(0)]
        [MyRequired]
        public string Street { get; set; }

        [MyOrder(1)]
        [MyRequired]
        public string City { get; set; }

        [MyOrder(2)]
        [MyDefaultValue("N/A")]
        public string State { get; set; }

        [MyOrder(3)]
        [MyPattern(@"^\d{5}(-\d{4})?$")]
        public string PostalCode { get; set; }

        [MyOrder(4)]
        public string Country { get; set; }

        [MyNonSerializable]
        public string Notes { get; set; }
    }

    public class UserSetting
    {
        [MyOrder(0)]
        [MyDefaultValue(true)]
        public bool IsEnabled { get; set; }

        [MyOrder(1)]
        [MyDefaultValue(100)]
        public int Limit { get; set; }

        [MyOrder(2)]
        [MyPattern(@"^\d{2}:\d{2}$")]
        public string TimeWindow { get; set; }

        [MyOrder(3)]
        public Dictionary<string, object> Parameters { get; set; }
    }
}
