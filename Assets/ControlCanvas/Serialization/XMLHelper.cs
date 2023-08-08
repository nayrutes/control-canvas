using System.IO;
using System.Xml.Serialization;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;

namespace ControlCanvas.Serialization
{
    public class XMLHelper
    {
        public static void SerializeToXML(string path, CanvasData data)
        {
            IExtendedXmlSerializer serializerEx = new ConfigurationContainer()
                .UseAutoFormatting()
                .UseOptimizedNamespaces()
                .EnableImplicitTyping(typeof(CanvasData))
                .Create();
            XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
            using (StreamWriter writer = new StreamWriter(path))
            {
                serializer.Serialize(writer, data);
            }
        }
        
        public static void DeserializeFromXML(string path, out CanvasData data)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(CanvasData));
            using (StreamReader reader = new StreamReader(path))
            {
                data = (CanvasData)serializer.Deserialize(reader);
            }
        }
    }
}