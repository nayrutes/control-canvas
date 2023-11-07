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
        public static string SerializeToXML(CanvasData data)
        {
            IExtendedXmlSerializer serializerEx = new ConfigurationContainer()
                .UseAutoFormatting()
                .EnableImplicitTyping(typeof(CanvasData))
                .UseOptimizedNamespaces()
                .AllowMultipleReferences()
                .Create();
            
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            
            return serializerEx.Serialize(settings, data);
        }
        public static void SerializeToXML(string path, CanvasData data)
        {
            string xml = SerializeToXML(data);
           
            try
            {
                using (StreamWriter writer = new StreamWriter(path))
                {
                    writer.Write(xml);
                }
                //AssetDatabase.Refresh();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
            
        }

        public static CanvasData DeserializeFromXML(string xml)
        {
            CanvasData data = new CanvasData();
            IExtendedXmlSerializer serializerEx = new ConfigurationContainer()
                .Type<EdgeData>()
                .AddMigration(new EdgeDataMigration())
                .Type<NodeData>()
                .AddMigration(new NodeDataMigration())
                .UseAutoFormatting()
                //.WithUnknownContent()
                //.Continue()
                .EnableImplicitTyping(typeof(CanvasData))
                .UseOptimizedNamespaces()
                .AllowMultipleReferences()
                .Create();

            try
            {
                data = serializerEx.Deserialize<CanvasData>(xml);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return data;
        }
        
        public static void DeserializeFromXML(string path, out CanvasData data)
        {
            string xml = "";
            data = new CanvasData();
            try
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    //data = serializerEx.Deserialize<CanvasData>(reader);
                    xml = reader.ReadToEnd();
                }
                data = DeserializeFromXML(xml);
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}