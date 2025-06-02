using System.Globalization;
using System.Text;

namespace SerializerLib.Json;

public class JsonWriter(JsonSerializerOptions options = default)
{
    private readonly StringBuilder builder = new StringBuilder();
    private int currentIndentationLevel = 0;

    public void WritePrimitive(object value)
    {
        if (value is string str)
        {
            builder.Append($"\"{str}\"");
        }
        else if (value is DateTime dt)
        {
            builder.Append($"\"{dt.ToString(CultureInfo.InvariantCulture)}\"");
        }
        else if (value is DateTimeOffset dto)
        {
            builder.Append($"\"{dto.ToString(CultureInfo.InvariantCulture)}\"");
        }
        else
        {
            var primitiveStr = Convert.ToString(value, CultureInfo.InvariantCulture);
            builder.Append(primitiveStr);
        }
    }

    public void WriteComma()
    {
        builder.Append(',');
        if (options.PrettyPrint)
        {
            builder.Append('\n');
        }
    }

    public void WriteNull()
    {
        builder.Append("null");
    }

    public void WritePropertyName(string name)
    {
        if (options.PrettyPrint)
        {
            var spaceCount = options.IndentSize * currentIndentationLevel;
            builder.Append(new string(' ', spaceCount));
            builder.Append($"\"{name}\": ");
        }
        else
        {
            builder.Append($"\"{name}\":");
        }
    }

    public void WriteStartObject()
    {
        if (options.PrettyPrint)
        {
            builder.Append("{\n");
            currentIndentationLevel += 1;
        }
        else
        {
            builder.Append('{');
        }
    }

    public void WriteEndObject()
    {
        if (options.PrettyPrint)
        {
            currentIndentationLevel -= 1;
            var spaceCount = options.IndentSize * currentIndentationLevel;
            builder.Append('\n');
            builder.Append(new string(' ', spaceCount));
        }

        builder.Append('}');
    }

    public void WriteStartArray()
    {
        if (options.PrettyPrint)
        {
            builder.Append("[\n");
            currentIndentationLevel += 1;
        }
        else
        {
            builder.Append('[');
        }
    }

    public void WriteEndArray()
    {
        if (options.PrettyPrint)
        {
            currentIndentationLevel -= 1;
            var spaceCount = options.IndentSize * currentIndentationLevel;
            builder.Append('\n');
            builder.Append(new string(' ', spaceCount));
        }

        builder.Append(']');
    }

    public string GetJson() => builder.ToString();
}