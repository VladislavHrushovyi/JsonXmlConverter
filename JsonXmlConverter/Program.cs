using JsonXmlConverter;

var converter = new Converter("inputXml.xml");

var resultJsonString = converter.FromXmlToJson();

File.WriteAllText("resultJson.json", resultJsonString);
