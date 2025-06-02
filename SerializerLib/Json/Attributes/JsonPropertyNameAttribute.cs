namespace SerializerLib.Json.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class JsonPropertyNameAttribute(string propertyName) : Attribute
{
    public string PropertyName { get; } = propertyName;
}