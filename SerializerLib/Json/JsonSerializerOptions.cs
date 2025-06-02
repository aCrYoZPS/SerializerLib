using System.Globalization;
using SerializerLib.Enums;

namespace SerializerLib.Json;

public struct JsonSerializerOptions
{
    public JsonSerializerOptions() { }

    public JsonSerializerOptions(
        CasePolicy casePolicy = CasePolicy.CamelCase,
        bool prettyPrint = false,
        int indentSize = 4,
        bool ignoreNullValues = false
    )
    {
        CasePolicy = casePolicy;
        PrettyPrint = prettyPrint;
        IndentSize = indentSize;
        IgnoreNullValues = ignoreNullValues;
    }

    public CasePolicy CasePolicy { get; } = CasePolicy.CamelCase;
    public bool PrettyPrint { get; } = false;
    public int IndentSize { get; } = 4;
    public bool IgnoreNullValues { get; } = false;
}