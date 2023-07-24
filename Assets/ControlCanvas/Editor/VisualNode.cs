using System;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using System.Reflection;

namespace ControlCanvas.Editor
{
    public class VisualNode : UnityEditor.Experimental.GraphView.Node
    {
        public Node node;

        public Port portIn;
        public Port portOut;

        private VisualElement m_DynamicContent;
        
        public VisualNode(Node node) : base("Assets/ControlCanvas/Editor/VisualNodeUXML.uxml")
        {
            //this.UseDefaultStyling();
            //StyleSheet styleSheet = EditorGUIUtility.Load("StyleSheets/GraphView/Node.uss") as StyleSheet;
            //StyleSheet styleSheet = EditorGUIUtility.Load("Assets/ControlCanvas/Editor/Node.uss") as StyleSheet;
            //this.styleSheets.Add(styleSheet);  
            
            //WriteStyleSheetToFile(styleSheet, "Assets/ControlCanvas/Editor/Node.uss");
            
            this.node = node;
            this.title = node.Name + node.Guid;
            this.viewDataKey = node.Guid;
            this.SetPosition(new Rect(node.Position, node.Size));
            this.RegisterCallback((GeometryChangedEvent evt) =>
            {

                node.Position = evt.newRect.position;
                node.Size = evt.newRect.size;
            });

            CreatePorts();

            m_DynamicContent = this.Q<VisualElement>("dynamic-content");
            this.Q<EnumField>("type-enum").RegisterValueChangedCallback(OnTypeChanged);
            this.Q<EnumField>("type-enum").value = node.NodeType;
            SetNewType(node.NodeType);
        }

        private void OnTypeChanged(ChangeEvent<Enum> evt)
        {
            SetNewType((ControlCanvasEditorWindow.NodeType)evt.newValue);
        }
        
        private void SetNewType(ControlCanvasEditorWindow.NodeType type)
        {
            node.NodeType = type;
            m_DynamicContent.Clear();
            m_DynamicContent.Add(new Label($"This is a {type} node"));
        }
        
        public void CreatePorts()
        {
            portIn = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            if (portIn != null)
            {
                portIn.portName = "portIn";
                inputContainer.Add(portIn);
            }

            portOut = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            if (portOut != null)
            {
                portOut.portName = "portOut";
                outputContainer.Add(portOut);
            }
        }
        
        public static void WriteStyleSheetToFile(StyleSheet styleSheet, string path)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies)
            {
                Type type = assembly.GetType("Unity.UI.Builder.StyleSheetToUss");
                if (type != null)
                {
                    // Get the method itself
                    MethodInfo method = type.GetMethod("WriteStyleSheet", BindingFlags.Static | BindingFlags.Public);

                    if (method == null)
                    {
                        Debug.LogError("Could not find method WriteStyleSheet");
                        return;
                    }

                    // Create an instance of UssExportOptions
                    Type optionsType = assembly.GetType("Unity.UI.Builder.UssExportOptions");
                    object options = Activator.CreateInstance(optionsType);

                    // Call the method
                    method.Invoke(null, new object[] { styleSheet, path, options });
                    return;
                }
            }
        }



        
        
    }
}