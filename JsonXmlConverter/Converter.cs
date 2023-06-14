using System.Text;
using System.Xml;
using Newtonsoft.Json;

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

    public XmlDocument FromJsonToXml()
    {
        XmlDocument xmlObjResult = new();


        return xmlObjResult;
    }
}