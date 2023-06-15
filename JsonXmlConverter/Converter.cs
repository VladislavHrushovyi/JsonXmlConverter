using System.Text;
using System.Text.Json;
using System.Xml;

namespace JsonXmlConverter;

public class Converter
{
    private readonly string _path;

    public Converter(string path)
    {
        _path = path;
    }

    public string? FromXmlToJson()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(_path);
        var rootNode = xmlDoc.DocumentElement!;
        var json = new StringBuilder();
        json.Append("{");
        json.AppendFormat($"\"{rootNode.Name}\":");
        foreach (XmlNode node in xmlDoc.ChildNodes)
        {
            json.Append(ConvertXmlNodeToJson(node));
        }

        if (json[json.Length - 1] == ',')
        {
            json.Length--; // Remove the trailing comma
        }

        json.Append("}");
        return json.ToString();
    }

    private string ConvertXmlNodeToJson(XmlNode node)
    {
        var json = new StringBuilder();

        if (node.NodeType == XmlNodeType.Element)
        {
            json.Append("{");

            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                json.Append("\"@attributes\": {");
                var attributes = new List<string>();

                foreach (XmlAttribute attr in node.Attributes)
                {
                    attributes.Add($"\"{attr.Name}\": \"{attr.Value}\"");
                }

                json.Append(string.Join(",", attributes));
                json.Append("},");
            }

            if (node.HasChildNodes)
            {
                var groupedChildNodes = node.ChildNodes.Cast<XmlNode>()
                    .Where(n => n.NodeType != XmlNodeType.Text) // Exclude text nodes
                    .GroupBy(n => n.Name)
                    .ToList();

                foreach (var group in groupedChildNodes)
                {
                    if (group.Count() > 1)
                    {
                        json.AppendFormat("\"{0}\": [", group.Key);

                        var childNodes = new List<string>();

                        foreach (XmlNode childNode in group)
                        {
                            string childJson = ConvertXmlNodeToJson(childNode);
                            childNodes.Add(childJson);
                        }

                        json.Append(string.Join(",", childNodes));
                        json.Append("],");
                    }
                    else
                    {
                        string childJson = ConvertXmlNodeToJson(group.First());

                        if (group.First().HasChildNodes && group.First().FirstChild.NodeType == XmlNodeType.Text)
                        {
                            string textValue = EscapeString(group.First().FirstChild.Value);
                            json.AppendFormat("\"{0}\": {1},", group.Key, $"\"{textValue}\"");
                        }
                        else
                        {
                            json.AppendFormat("\"{0}\": {1},", group.Key, childJson);
                        }
                    }
                }

                if (json[json.Length - 1] == ',')
                {
                    json.Length--; // Remove the trailing comma
                }
            }

            json.Append("}");
        }

        return json.ToString();
    }

    private string EscapeString(string input)
    {
        return input.Replace("\"", "\\\"");
    }

    public string FromJsonToXml()
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
                    //xml.Append($"<{parentElementName}");
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
                        //xml.Append('>');
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