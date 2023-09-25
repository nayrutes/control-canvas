using System;
using System.IO;
using System.Xml;
using ExtendedXmlSerializer;
using ExtendedXmlSerializer.Configuration;
using UnityEditor;
using UnityEngine;

namespace ControlCanvas.Serialization
{
    public class XMLHelper
    {
        public static void SerializeToXML(string path, CanvasData data)
        {
            IExtendedXmlSerializer serializerEx = new ConfigurationContainer()
                    .UseAutoFormatting()
                    .EnableImplicitTyping(typeof(CanvasData))
                    .UseOptimizedNamespaces()
                    .Create();
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            
            string xml = serializerEx.Serialize(settings, data);
           
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(xml);
                }
                AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
        
        public static void DeserializeFromXML(string path, out CanvasData data)
        {
            data = new CanvasData();
            IExtendedXmlSerializer serializerEx = new ConfigurationContainer()
                .UseAutoFormatting()
                .WithUnknownContent()
                .Continue()
                .EnableImplicitTyping(typeof(CanvasData))
                .UseOptimizedNamespaces()
                .Create();

            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    data = serializerEx.Deserialize<CanvasData>(reader);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }
    }
}