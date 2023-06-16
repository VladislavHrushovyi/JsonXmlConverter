using JsonXmlConverter;

var xmlConverter = new XmlConverter("inputXml.xml");
var jsonResult = xmlConverter.Convert();

File.WriteAllText("resultJson.json", jsonResult);

var jsonConverter = new JsonConverter("inputJson.json");
var xmlResult = jsonConverter.Convert();

File.WriteAllText("resultXml.xml", xmlResult);
