using System.Text;
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
        var result = ParseXml(rootNode);
        return result;
    }

    private string ParseXml(XmlNode node)
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
                var childNodes = new List<string>();

                foreach (XmlNode childNode in node.ChildNodes)
                {
                    string childJson = ParseXml(childNode);
                    childNodes.Add(childJson);
                }

                json.Append($"\"{node.Name}\": {(childNodes.Count > 1 ? "[" : "")}");
                json.Append(string.Join(",", childNodes));
                json.Append($"{(childNodes.Count > 1 ? "]" : "")}");
            }
            else
            {
                json.Append($"\"{node.Name}\": null");
            }

            json.Append("}");
        }
        else if (node.NodeType == XmlNodeType.Text)
        {
            json.Append($"\"{EscapeString(node.ParentNode.Name)}\": \"{EscapeString(node.InnerText)}\"");
        }

        return json.ToString();
    }
    private string EscapeString(string s)
    {
        return s.Replace("\"", "\\\"");
    }

    public XmlDocument FromJsonToXml()
    {
        XmlDocument xmlObjResult = new();


        return xmlObjResult;
    }
}