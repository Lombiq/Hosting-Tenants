using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;

namespace Lombiq.Hosting.Tenants.Management.Service;

public class JsonConfigurationParser
{
    private readonly Dictionary<string, string> Data = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> Paths = new();

    public IDictionary<string, string> ParseConfiguration(string inputJson)
    {
        var jsonDocumentOptions = new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        };

        using var doc = JsonDocument.Parse(inputJson, jsonDocumentOptions);
        if (doc.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new FormatException("Format Exception:" + doc.RootElement.ValueKind);
        }

        VisitObjectElement(doc.RootElement);

        return Data;
    }

    private void VisitObjectElement(JsonElement element)
    {
        var isEmpty = true;

        foreach (var property in element.EnumerateObject())
        {
            isEmpty = false;
            EnterContext(property.Name);
            VisitValue(property.Value);
            ExitContext();
        }

        SetNullIfElementIsEmpty(isEmpty);
    }

    private void VisitArrayElement(JsonElement element)
    {
        int index = 0;

        foreach (var arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToString(CultureInfo.InvariantCulture));
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && Paths.Count > 0)
        {
            Data[Paths.Peek()] = null;
        }
    }

    private void VisitValue(JsonElement value)
    {
        switch (value.ValueKind)
        {
            case JsonValueKind.Object:
                VisitObjectElement(value);
                break;

            case JsonValueKind.Array:
                VisitArrayElement(value);
                break;

            case JsonValueKind.Number:
            case JsonValueKind.String:
            case JsonValueKind.True:
            case JsonValueKind.False:
            case JsonValueKind.Null:
                string key = Paths.Peek();
                if (Data.ContainsKey(key))
                {
                    throw new FormatException("Key Is Duplicated:" + key);
                }

                Data[key] = value.ToString();
                break;

            case JsonValueKind.Undefined:
            default:
                throw new FormatException("Unsupported JSON Token:" + value.ValueKind);
        }
    }

    private void EnterContext(string context) =>
        Paths.Push(Paths.Count > 0 ?
            Paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);

    private void ExitContext() => Paths.Pop();
}
