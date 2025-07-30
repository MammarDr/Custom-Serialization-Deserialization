# Custom-Serializer-Deserializer


## What is Serlization ?
it is the process of converting an object or data structure to a new format that can be easily saved, transmited OR reconstructed;

## Purpose of Serlization ?
The primary purpose is to store the state of an object or send it through a network;

### Here im building my own Serlization Logic !

# Usage


1- Clone the repository:

```bash
git clone https://github.com/MammarDr/Custom-Serialization-Deserialization.git
```

2- Navigate to the project directory:

```bash
cd Custom-Serialization-Deserialization
```

3- Build and run the project using your preferred IDE or the .NET CLI.

## Features
Custom Attributes: Define serialization behavior using attributes like [MyRequired], [MyOrder], [MyDefaultValue], [MyNickName], [MyPattern], and [MyNonSerializable].

Reflection-Based Serialization: Dynamically serialize objects based on their metadata.

Flexible JSON Output: Customize property names, order, default values, and validation patterns.

## Targeted Class
```csharp
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
```
## Notes
Properties marked with [MyNonSerializable] are excluded from the output.

[MyNickName] allows renaming properties in the serialized JSON.

[MyOrder] determines the order of properties in the output.

[MyDefaultValue] assigns default values if none are provided.

[MyPattern] can be used to validate property values against regular expressions.


## Declaration
```csharp
MyJSONSerializer myserializer = MyJSONSerializer.CreateInstance(typeof(Product));

if (myserializer == null) return;


Product product1 = new Product
{
    ProductId = Guid.NewGuid(),
    Name = "Laptop",
    Description = "High-end gaming laptop",
    Price = 1499.99,
    SKU = "EL-2023",
    IsAvailable = true,
    InternalNotes = "Check supplier ID 3345",
    Categories = new List<string> { "Electronics", "gaming" },
    Features = new object[] { "RGB", "Cooling" },
    Ratings = new int[] { 5, 4, 4, 5 },
    Metadata = new { Warranty = "2 years", Color = "Black" }
};

using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
{
    Console.WriteLine("Product 01 :");
    if (!myserializer.Serialize(writer, product1))
        Console.WriteLine("\nFailed to serialize the object\n");
}
```

# Result :

```
Product 01 :
{
  "Type" : "Product",
  "Data" : {
    "ProductId" : 132cd722-d3e2-4e7f-80f4-461f7719c540,
    "Label" : "Laptop",
    "Description" : "High-end gaming laptop",
    "Price" : 1499.99,
    "SKU" : "EL-2023",
    "IsAvailable" : true,
    "Categories" : ["Electronics","Gaming"],
    "Features" : ["RGB","Cooling"],
    "Ratings" : [5,4,4,5],
    "Metadata" : {
      "Warranty" : "2 years",
      "Color" : "Black"
    }
  }
}
```




✅ Suggestions for Improvement
Correct Spelling Errors: The term "Serlization" should be corrected to "Serialization" throughout the repository for professionalism and clarity.

Implement Deserialization: Currently, the focus is on serialization. Adding deserialization functionality would complete the framework and enhance its utility.

Unit Testing: Incorporate unit tests to ensure the reliability and correctness of the serialization and deserialization processes.

Exception Handling: Enhance robustness by implementing comprehensive exception handling, especially during reflection and I/O operations.

Documentation: Expand the README with detailed explanations of each custom attribute, their usage, and examples.

NuGet Packaging: Consider packaging the serializer as a NuGet package for easy integration into other projects.

Support for More Data Types: Extend support to handle more complex data types, such as dictionaries, nested objects, and custom collections.

Performance Optimization: Analyze and optimize performance, especially for large object graphs, possibly by caching reflection metadata.

Attribute Validation: Implement validation to ensure that attributes are used correctly and consistently across different classes.

Sample Projects: Include sample projects or use cases demonstrating the serializer's capabilities in real-world scenarios.
