namespace JsonXmlConverter;

public abstract class BaseConverter
{
    protected readonly string _path;

    public BaseConverter(string path)
    {
        _path = path;
    }

    public abstract string Convert();
}