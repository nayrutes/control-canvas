using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using ExtendedXmlSerializer;

namespace ControlCanvas.Serialization
{
    public class EdgeDataMigration : IEnumerable<Action<XElement>>
    {
        public static void MigrationV0(XElement edge)
        {
            XAttribute startPortName = edge.Attribute("StartPortName");
            PortType startPortType = PortNameToType(startPortName.Value);
            edge.Add(new XAttribute("StartPortType", startPortType));
            
            XAttribute endPortName = edge.Attribute("EndPortName");
            PortType endPortType = PortNameToType(endPortName.Value);
            edge.Add(new XAttribute("EndPortType", endPortType));
            
            startPortName.Remove();
            endPortName.Remove();
            
        }
        
        public IEnumerator<Action<XElement>> GetEnumerator()
        {
            yield return MigrationV0;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        
        
        private static PortType PortNameToType(string portName)
        {
            PortType portType;
            switch (portName)
            {
                case "portIn" or "In":
                    portType = PortType.In;
                    break;
                case "portOut" or "Out":
                    portType = PortType.Out;
                    break;
                case "portOut-2" or "Failure":
                    portType = PortType.Out2;
                    break;
                case "portParallel" or "Parallel" or "portOutParallel":
                    portType = PortType.Parallel;
                    break;
                case "In/Out" or "inout" or "InOut":
                    portType = PortType.InOut;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(portName), portName, null);
            }

            return portType;
        }
    }
}