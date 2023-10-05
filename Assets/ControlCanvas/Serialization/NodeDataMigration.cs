using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;

namespace ControlCanvas.Serialization
{
    public class NodeDataMigration : IEnumerable<Action<XElement>>
    {
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
        
        public IEnumerator<Action<XElement>> GetEnumerator()
        {
            yield return MigrationV0;
            yield return MigrationV1;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}