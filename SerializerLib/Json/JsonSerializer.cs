using System.Collections;
using System.Reflection;
using System.Text.Json;
using SerializerLib.Json.Attributes;

namespace SerializerLib.Json;

public static class JsonSerializer
{
    public static string Serialize<T>(T? obj, JsonSerializerOptions options = default)
    {
        var writer = new JsonWriter(options);
        SerializeValue(obj, writer, options);
        return writer.GetJson();
    }

    private static void SerializeValue(object? value, JsonWriter writer, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNull();
            return;
        }

        var type = value.GetType();

        if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal) || type == typeof(Guid) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset))
        {
            writer.WritePrimitive(value);
        }
        else
        {
            switch (value)
            {
                case IDictionary dictionary:
                    SerializeDictionary(dictionary, writer, options);
                    break;
                case IEnumerable enumerable:
                    SerializeEnumerable(enumerable, writer, options);
                    break;
                default:
                    SerializeObject(value, writer, options);
                    break;
            }
        }
    }


    private static void SerializeDictionary(
        IDictionary dictionary,
        JsonWriter writer,
        JsonSerializerOptions options
    )
    {
        writer.WriteStartObject();

        var isFirst = true;
        foreach (DictionaryEntry entry in dictionary)
        {
            var keyString = entry.Key.ToString();
            if (keyString == null)
            {
                throw new InvalidOperationException
                (
                    "The dictionary key's string representation is null. Ensure key.ToString() returns a valid non-null string."
                );
            }

            if (!isFirst)
            {
                writer.WriteComma();
            }

            writer.WritePropertyName(keyString);
            SerializeValue(entry.Value, writer, options);
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    private static void SerializeEnumerable(IEnumerable enumerable, JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        var isFirst = true;
        foreach (var obj in enumerable)
        {
            if (!isFirst)
            {
                writer.WriteComma();
            }

            SerializeValue(obj, writer, options);

            isFirst = false;
        }

        writer.WriteEndArray();
    }

    private static void SerializeObject(object obj, JsonWriter writer, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        var type = obj.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var isFirst = true;
        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                continue;
            }

            var value = property.GetValue(obj);
            if (value == null && options.IgnoreNullValues)
            {
                continue;
            }

            var propertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            var name = (propertyNameAttribute != null)
                ? propertyNameAttribute.PropertyName
                : Utils.Caseify(property.Name, options.CasePolicy);
            if (!isFirst)
            {
                writer.WriteComma();
            }

            writer.WritePropertyName(name);
            SerializeValue(value, writer, options);
            isFirst = false;
        }

        writer.WriteEndObject();
    }

    public static T? Deserialize<T>(string jsonString, JsonSerializerOptions options = default)
    {
        var reader = new JsonReader(jsonString, options);
        return (T)reader.ReadValue(typeof(T));
    }
}