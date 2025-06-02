namespace SerializerLib.Json;

public enum JsonTokenType
{
    StartObject,
    EndObject,
    StartArray,
    EndArray,
    PropertyName,
    Value,
}

public class JsonToken(JsonTokenType type, string value)
{
    public JsonTokenType Type { get; private set; } = type;
    public string Value { get; private set; } = value;

    public override string ToString()
    {
        return $"Token value: {Value}\nToken type: {Type}";
    }

    public bool Equals(JsonToken? other)
    {
        if (other == null)
        {
            return false;
        }

        return other.Value == Value && other.Type == Type;
    }
}

public static class JsonTokenizer
{
    public static List<JsonToken> TokenizeJson(string json)
    {
        var pos = 0;
        var value = string.Empty;
        var result = new List<JsonToken>();
        while (pos < json.Length)
        {
            var currentChar = json[pos];
            switch (currentChar)
            {
                case '\n':
                case ' ':
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(new JsonToken(JsonTokenType.Value, value.Trim('"')));
                        value = string.Empty;
                    }

                    break;
                case '"':
                    pos += 1;
                    while (json[pos] != '"' || json[pos - 1] == '\\')
                    {
                        value += json[pos];
                        pos += 1;
                    }

                    break;
                case ':':
                    result.Add(new JsonToken(JsonTokenType.PropertyName, value.Trim('"')));
                    value = string.Empty;
                    break;
                case '{':
                    result.Add(new JsonToken(JsonTokenType.StartObject, "{"));
                    break;
                case '}':
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(new JsonToken(JsonTokenType.Value, value.Trim('"')));
                        value = string.Empty;
                    }

                    result.Add(new JsonToken(JsonTokenType.EndObject, "}"));
                    break;
                case '[':
                    result.Add(new JsonToken(JsonTokenType.StartArray, "["));
                    break;
                case ']':
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(new JsonToken(JsonTokenType.Value, value.Trim('"')));
                        value = string.Empty;
                    }

                    result.Add(new JsonToken(JsonTokenType.EndArray, "]"));
                    break;
                case ',':
                    if (!string.IsNullOrEmpty(value))
                    {
                        result.Add(new JsonToken(JsonTokenType.Value, value.Trim('"')));
                        value = string.Empty;
                    }

                    break;
                default:
                    value += currentChar;
                    break;
            }

            pos += 1;
        }

        if (!string.IsNullOrEmpty(value))
        {
            result.Add(new JsonToken(JsonTokenType.Value, value));
            value = string.Empty;
        }

        return result;
    }
}