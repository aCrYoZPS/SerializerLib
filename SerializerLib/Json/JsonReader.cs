using System.Collections;
using System.Globalization;
using System.Reflection;
using SerializerLib.Json.Attributes;

namespace SerializerLib.Json;

public class JsonReader(string jsonString, JsonSerializerOptions options)
{
    private readonly List<JsonToken> tokens = JsonTokenizer.TokenizeJson(jsonString);

    private int position = 0;
    private JsonToken CurrentToken => tokens[position];

    public object? ReadValue(Type type)
    {
        return CurrentToken.Type switch
        {
            JsonTokenType.PropertyName => ReadPrimitive(type),
            JsonTokenType.Value => ReadPrimitive(type),
            JsonTokenType.StartObject => (IsDictionaryType(type) ? ReadDictionary(type) : ReadObject(type)),
            JsonTokenType.StartArray => ReadArray(type),
            _ => throw new ArgumentOutOfRangeException(nameof(CurrentToken.Type), "DAYM")
        };
    }

    private object ReadDictionary(Type type)
    {
        var keyType = type.GenericTypeArguments[0];
        var valueType = type.GenericTypeArguments[1];
        var dictionary = (IDictionary)Activator.CreateInstance(type)!;

        Next();

        while (CurrentToken.Type != JsonTokenType.EndObject)
        {
            var key = ReadValue(keyType);
            Next();
            var value = ReadValue(valueType);
            Next();
            dictionary.Add(key!, value);
        }

        return dictionary;
    }

    private object ReadArray(Type type)
    {
        var list = (IList)Activator.CreateInstance(type);
        Next();
        while (CurrentToken.Type != JsonTokenType.EndArray)
        {
            list.Add(ReadValue(type.GenericTypeArguments[0]));
            Next();
        }

        return list;
    }

    private object ReadObject(Type type)
    {
        var propsMap = GetJsonNameToPropertyNameMap(type, options);
        var instance = Activator.CreateInstance(type);
        Next();
        while (true)
        {
            if (CurrentToken.Type == JsonTokenType.EndObject)
            {
                break;
            }

            var property = type.GetProperty(propsMap[CurrentToken.Value], BindingFlags.Public | BindingFlags.Instance);
            if (property != null)
            {
                Next();
                var value = ReadValue(property.PropertyType);
                property.SetValue(instance, value);
            }

            Next();
        }

        return instance;
    }

    private object? ReadPrimitive(Type type)
    {
        if (CurrentToken.Value == "null")
        {
            if (!IsTypeNullable(type))
                throw new ArgumentException($"Cannot convert null to non-nullable type {type.Name}");

            return null;
        }


        if (IsNullableValueType(type, out var underlyingType))
        {
            return ReadPrimitive(underlyingType);
        }

        var value = CurrentToken.Value;
        return type switch
        {
            not null when type == typeof(string) => value.Trim('"'),
            not null when type == typeof(float) => float.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(double) => double.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(sbyte) => sbyte.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(short) => short.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(int) => int.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(nint) => nint.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(long) => long.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(Int128) => Int128.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(byte) => byte.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(ushort) => ushort.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(uint) => uint.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(nuint) => nuint.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(ulong) => ulong.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(UInt128) => UInt128.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(bool) => bool.Parse(value),
            not null when type == typeof(decimal) => decimal.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(Guid) => Guid.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(DateTime) => DateTime.Parse(value, CultureInfo.InvariantCulture),
            not null when type == typeof(DateTimeOffset) => DateTimeOffset.Parse(value, CultureInfo.InvariantCulture),
            _ => throw new Exception("Tried to deserialize non-primitive type into primitive type")
        };
    }

    private void Next(int step = 1)
    {
        position += step;
    }

    private static Dictionary<string, string> GetJsonNameToPropertyNameMap(Type type, JsonSerializerOptions options)
    {
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        var jsonNameToPropertyName = new Dictionary<string, string>();
        foreach (var property in properties)
        {
            if (property.GetCustomAttribute<JsonIgnoreAttribute>() != null)
            {
                continue;
            }

            var propertyNameAttribute = property.GetCustomAttribute<JsonPropertyNameAttribute>();
            var name = (propertyNameAttribute != null)
                ? propertyNameAttribute.PropertyName
                : Utils.Caseify(property.Name, options.CasePolicy);
            jsonNameToPropertyName.Add(name, property.Name);
        }

        return jsonNameToPropertyName;
    }

    private static bool IsTypeNullable(Type type)
    {
        return !type.IsValueType || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
    }

    private static bool IsNullableValueType(Type type, out Type? underlyingType)
    {
        if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
        {
            underlyingType = Nullable.GetUnderlyingType(type);
            return true;
        }

        underlyingType = null;
        return false;
    }

    private static bool IsDictionaryType(Type type)
    {
        return type.IsGenericType &&
               type.GetGenericTypeDefinition() == typeof(Dictionary<,>);
    }
}