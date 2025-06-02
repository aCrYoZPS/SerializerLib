namespace SerializerLib.Json.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class JsonIgnoreAttribute : Attribute
{
    public JsonIgnoreAttribute() { }
}