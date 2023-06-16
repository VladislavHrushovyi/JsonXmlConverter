using System.Text;
using System.Xml;

namespace JsonXmlConverter;

public class XmlConverter : BaseConverter
{
    public XmlConverter(string path) : base(path)
    {
    }

    public override string Convert()
    {
        var xmlDoc = new XmlDocument();
        xmlDoc.Load(_path);
        var rootNode = xmlDoc.DocumentElement!;
        var json = new StringBuilder();
        json.Append('{');
        json.AppendFormat($"\"{rootNode.Name}\":");
        foreach (XmlNode node in xmlDoc.ChildNodes)
        {
            json.Append(ConvertXmlNodeToJson(node));
        }

        if (json[^1] == ',')
        {
            json.Length--;
        }

        json.Append('}');
        return json.ToString();
    }
    
    private string ConvertXmlNodeToJson(XmlNode node)
    {
        var json = new StringBuilder();
        
        if (node.NodeType == XmlNodeType.Element)
        {
            json.Append('{');

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
                    //.Where(n => n.NodeType != XmlNodeType.Text)
                    .GroupBy(n => n.Name)
                    .ToList();

                foreach (var group in groupedChildNodes)
                {
                    if (group.Count() > 1)
                    {
                        json.Append($"\"{group.Key}\": [");

                        var childNodes = new List<string>();

                        foreach (XmlNode childNode in group)
                        {
                            if (childNode.FirstChild == childNode.LastChild)
                            {
                                childNodes.Add($"\"{childNode.FirstChild.InnerText}\"");
                            }
                            else
                            {
                                string childJson = ConvertXmlNodeToJson(childNode);
                                childNodes.Add(childJson);   
                            }
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
                            json.Append($"\"{group.Key}\": \"{textValue}\",");
                        }
                        else
                        {
                            json.Append($"\"{group.Key}\": {childJson},");
                        }
                    }
                }

                if (json[^1] == ',')
                {
                    json.Length--;
                }
            }

            json.Append('}');
        }

        return json.ToString();
    }

    private string EscapeString(string input)
    {
        return input.Replace("\"", "\\\"");
    }
}