using SerializerLib.Enums;
using SerializerLib.Json;
using SerializerLib.Json.Attributes;
using SerializerLib.Xml;

var options = new JsonSerializerOptions(CasePolicy.SnakeCase);

var i = new TestStruct();
// var json = JsonSerializer.Serialize(i, options);
// var tokens = JsonTokenizer.TokenizeJson(json);
//
// foreach (var token in tokens)
// {
//     Console.WriteLine(token);
// }
//
// var elem = JsonSerializer.Deserialize<TestStruct>(json, options);
var xml = XmlSerializer.Serialize(i);
var st = XmlSerializer.Deserialize<TestStruct>(xml);
Console.WriteLine(st);


record Easy
{
    public Easy() { }
    public int? AEasyNull { get; set; } = null;
    public int AEasyInt { get; set; } = 5;
    public string AEasyString { get; set; } = "AA";
    public bool AEasyBool { get; set; } = true;
}

record DictStruct
{
    public DictStruct() { }

    public Dictionary<int, Dictionary<string, string>> DPr { get; set; } =
        new Dictionary<int, Dictionary<string, string>>()
        {
            {
                1, new Dictionary<string, string>()
                {
                    { "k1", "v1" },
                    { "k2", "v2" }
                }
            },
            {
                2, new Dictionary<string, string>()
                {
                    { "k1", "v2" },
                    { "k2", "v1" }
                }
            }
        };
}

record Inner
{
    public Inner() { }

    public decimal Dec { get; set; } = 10.51M;
}

record TestStruct
{
    // public TestStruct() { }
    //
    // public int? NullInt { get; set; } = null;
    //
    // public int AProperty { get; set; } = 5;
    //
    // public string OtherProperty { get; set; } = "other";

    public List<int> LInt { get; set; } = [1, 3, 5, 77];

    // [JsonIgnore]
    // public string Ignored { get; set; } = "not ignored";
    //
    // [JsonPropertyName("namey")]
    // public double NotNamey { get; set; } = 6.9;
    //
    // public Inner InnerStruct { get; set; } = new Inner();
    // public Guid GuidishceAaaa { get; set; } = Guid.NewGuid();
    // public DateTime DTi { get; set; } = DateTime.Now;
}