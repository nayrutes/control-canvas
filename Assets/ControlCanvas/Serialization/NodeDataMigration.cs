using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UnityEngine;

namespace ControlCanvas.Serialization
{
    public class NodeDataMigration : IEnumerable<Action<XElement>>
    {
        private static List<Type> allTypes;
        public static List<Type> AllTypes
        {
            get
            {
                if (allTypes == null)
                {
                    allTypes = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(a => a.GetTypes()).ToList();
                }
                return allTypes;
            }
        }

        public static void MigrationV0(XElement node)
        {
            XNamespace ns = "clr-namespace:ControlCanvas.Serialization;assembly=SerializationScripts";
            XElement specificControl = node.Element(ns + "specificControl");
            if (specificControl == null && node.Attribute("nodeType")?.Value=="Routing")
            {
                XNamespace exs = "https://extendedxmlserializer.github.io/v2";
                specificControl = new XElement(ns +"specificControl");
                specificControl.SetAttributeValue(exs+"type", "ns1:RoutingControl");
                node.Add(specificControl);
            }
        }

        public static void MigrationV1(XElement node)
        {
            XAttribute nodeType = node.Attribute("nodeType");
            nodeType?.Remove();
        }
        
        public static void RemoveNotFoundSpecificControls(XElement node)
        {
            XNamespace ns = "clr-namespace:ControlCanvas.Serialization;assembly=SerializationScripts";
            XElement specificControl = node.Element(ns + "specificControl");
            if (specificControl != null)
            {
                XNamespace exs = "https://extendedxmlserializer.github.io/v2";
                var specificTypeAttribute = specificControl.Attribute(exs + "type");
                if (specificTypeAttribute != null)
                {
                    var typeString = specificTypeAttribute.Value;
                    var className = typeString.Split(':').Last();
                    if (!TypeExists(className))
                    {
                        Debug.LogWarning($"Type {className} does not exist. Removing attribute.");
                        specificControl.Remove();
                    }
                }

            }
        }
        
        public IEnumerator<Action<XElement>> GetEnumerator()
        {
            yield return MigrationV0;
            yield return MigrationV1;
            yield return RemoveNotFoundSpecificControls;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        private static bool TypeExists(string className)
        {
            var type = AllTypes.FirstOrDefault(t => t.Name == className);
            return type != null;
        }

    }
}