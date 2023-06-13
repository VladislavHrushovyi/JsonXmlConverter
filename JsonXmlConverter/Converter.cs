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
        var result = (Dictionary<string, object>)ParseXml(rootNode);

        return result.ToString();
    }

    private object ParseXml(XmlNode node)
    {
        if (node.NodeType == XmlNodeType.Element)
        {
            var element = (XmlElement)node;

            if (element.HasAttributes || element.HasChildNodes)
            {
                var jsonObject = new Dictionary<string, object>();

                if (element.HasAttributes)
                {
                    var attributes = new Dictionary<string, string>();
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        attributes[attribute.Name] = attribute.Value;
                    }
                    jsonObject["@attributes"] = attributes;
                }

                if (element.HasChildNodes)
                {
                    var childNodes = element.ChildNodes;
                    var groupedChildNodes = new Dictionary<string, List<object>>();

                    foreach (XmlNode childNode in childNodes)
                    {
                        var childObjectName = childNode.Name;

                        if (!groupedChildNodes.ContainsKey(childObjectName))
                        {
                            groupedChildNodes[childObjectName] = new List<object>();
                        }

                        var childObject = ParseXml(childNode);
                        groupedChildNodes[childObjectName].Add(childObject);
                    }

                    foreach (var kvp in groupedChildNodes)
                    {
                        if (kvp.Value.Count == 1)
                        {
                            jsonObject[kvp.Key] = kvp.Value[0];
                        }
                        else
                        {
                            jsonObject[kvp.Key] = kvp.Value;
                        }
                    }
                }

                return jsonObject;
            }
        }
        else if (node.NodeType == XmlNodeType.Text)
        {
            return node.InnerText;
        }

        return null;
    }


    public XmlDocument FromJsonToXml()
    {
        XmlDocument xmlObjResult = new();
        
        

        return xmlObjResult;
    }
}