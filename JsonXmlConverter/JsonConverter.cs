using System.Text;
using System.Text.Json;

namespace JsonXmlConverter;

public class JsonConverter : BaseConverter
{
    public JsonConverter(string path) : base(path)
    {
    }

    public override string Convert()
    {
        var xml = new StringBuilder();
        xml.Append("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
        var jsonString = File.ReadAllText(_path);
        var jsonObj = JsonDocument.Parse(jsonString);
        JsonNodeToXml(jsonObj.RootElement, xml);

        return xml.ToString();
    }
    
    private void JsonNodeToXml(JsonElement jsonElement, StringBuilder xml, string parentElementName = "")
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in jsonElement.EnumerateObject())
            {
                if (property.Name == "@attributes")
                {
                    var attrBuilder = new StringBuilder();
                    foreach (var attributeProperty in property.Value.EnumerateObject())
                    {
                        attrBuilder.AppendFormat(
                            $" {attributeProperty.Name}=\"{attributeProperty.Value.GetString()}\"");
                    }

                    xml.Append($"{attrBuilder}>");
                }
                else
                {
                    if (property.Value.ValueKind == JsonValueKind.Array)
                    {
                        JsonNodeToXml(property.Value, xml, property.Name);
                    }
                    else
                    {
                        xml.Append($"<{property.Name}>");
                        JsonNodeToXml(property.Value, xml, property.Name);
                        xml.Append($"</{property.Name}>");
                    }
                }
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in jsonElement.EnumerateArray())
            {
                if (item.ValueKind is not JsonValueKind.Array or JsonValueKind.Object)
                {
                    xml.Append($"<{parentElementName}>");
                    JsonNodeToXml(item, xml, parentElementName);
                    xml.Append($"</{parentElementName}>");
                }
                else
                {
                    xml.Append($"<{parentElementName}");
                    JsonNodeToXml(item, xml, parentElementName);
                    xml.Append($"</{parentElementName}>");
                }
            }
        }
        else if (jsonElement.ValueKind == JsonValueKind.String)
        {
            xml.Append(jsonElement.GetString());
        }
        else if (jsonElement.ValueKind == JsonValueKind.Number)
        {
            xml.Append(jsonElement.GetRawText());
        }
        else if (jsonElement.ValueKind is JsonValueKind.True or JsonValueKind.False)
        {
            xml.Append(jsonElement.GetBoolean());
        }
        else if (jsonElement.ValueKind == JsonValueKind.Null)
        {
            xml.Append("null");
        }
    }
}