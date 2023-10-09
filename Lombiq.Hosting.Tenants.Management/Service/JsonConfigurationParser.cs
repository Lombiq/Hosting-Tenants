// This file is a copy and slight modification of Microsoft.Extensions.Configuration.Json.JsonConfigurationFileParser
// https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.Extensions.Configuration.Json/src/JsonConfigurationFileParser.cs.
// Their recommended way of using this class is to copy it. https://github.com/dotnet/runtime/issues/73946
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Lombiq.Hosting.Tenants.Management.Service;

public class JsonConfigurationParser
{
    private readonly Dictionary<string, string> _configurationData = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<string> _paths = new();

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
            throw new FormatException(
                $"Top-level JSON element must be an object. Instead, '{doc.RootElement.ValueKind}' was found.");
        }

        VisitObjectElement(doc.RootElement);

        return _configurationData;
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
        var index = 0;

        foreach (var arrayElement in element.EnumerateArray())
        {
            EnterContext(index.ToTechnicalString());
            VisitValue(arrayElement);
            ExitContext();
            index++;
        }

        SetNullIfElementIsEmpty(isEmpty: index == 0);
    }

    private void SetNullIfElementIsEmpty(bool isEmpty)
    {
        if (isEmpty && _paths.Count > 0)
        {
            _configurationData[_paths.Peek()] = null;
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
                var key = _paths.Peek();
                if (_configurationData.ContainsKey(key))
                {
                    throw new FormatException($"A duplicate key '{key}' was found.");
                }

                _configurationData[key] = value.ToString();
                break;

            case JsonValueKind.Undefined:
            default:
                throw new FormatException($"Unsupported JSON token '{value.ValueKind}' was found.");
        }
    }

    private void EnterContext(string context) =>
        _paths.Push(_paths.Count > 0 ?
            _paths.Peek() + ConfigurationPath.KeyDelimiter + context :
            context);

    private void ExitContext() => _paths.Pop();
}
