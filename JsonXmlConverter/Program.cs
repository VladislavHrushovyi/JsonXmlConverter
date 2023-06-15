using JsonXmlConverter;

var converter1 = new Converter("inputXml.xml");

var resultJsonString = converter1.FromXmlToJson();

File.WriteAllText("resultJson.json", resultJsonString);

var converter2 = new Converter("inputJson.json");
var resultXmlString = converter2.FromJsonToXml();

File.WriteAllText("resultXml.xml", resultXmlString);
