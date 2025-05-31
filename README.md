# Custom-Serlization-Deserlization


## What is Serlization ?
it is the process of converting an object or data structure to a new format that can be easily saved, transmited OR reconstructed;

## Purpose of Serlization ?
The primary purpose is to store the state of an object or send it through a network;

### Here im building my own Serlization Logic !

```csharp
    [MySerializable]
    public class Person
    {
        [MyRequired] //
        [MyOrder(0)]  public int? ID;
        
    [MyDefaultValue("Bob")] //
    [MyOrder(2)]  public string Name { get; set; }

    [MyNonSerializable] //
    public string City { get; set; }

    [MyDefaultValue(true)] //
    [MyOrder(5)] public bool? isAlive { get; set; }

    [MyNickName("ZipCode")] //
    [MyOrder(4)] public int? PostalCode { get; set; }

    [MyDefaultValue(18)] //
    [MyOrder(1)] public double? Age { get; set; }

    [MyTxtType(typeof(Person))]
    [MyDefaultValue(null)] //
    [MyOrder(3)] public object instance {  get; set; }
    }
```


```csharp
      Person person = new Person { ID = 4, Name = "Ahmed", City = "Oued", PostalCode = 1006, Age = 2.5 };
    
      mySerializer myserializer = mySerializer.CreateInstance(typeof(Person));
      
      if (myserializer == null) return;
      
      using (TextWriter writer = new StreamWriter("myCustomSerializer.txt"))
      {
          myserializer.Serializer(writer, person);
      }
```

# Result :

    {
     "Type" : "Person",
     "Data" : {
       "ID" : 4,
       "Age" : 2.5,
       "Name" : "Ahmed",
       "instance" : NULL,
       "ZipCode" : 1006,
       "isAlive" : True
       }
    }
