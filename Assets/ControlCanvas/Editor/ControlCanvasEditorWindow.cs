using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ControlCanvas.Editor;
using ControlCanvas.Serialization;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlCanvasEditorWindow : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;
    
    private ControlCanvasSO m_ControlCanvasSO;
    
    [SerializeField]
    private string m_ControlCanvasSO_dataPath;



    private ControlGraphView graphView;
    private InspectorView inspectorView;
    private IMGUIContainer blackboardView;
    
    public SerializedObject canvasObject;
    private SerializedProperty blackboardProperty;

    [MenuItem("Window/UI Toolkit/ControlCanvasEditorWindow")]
    public static void OpenWindow()
    {
        ControlCanvasEditorWindow wnd = GetWindow<ControlCanvasEditorWindow>();
        wnd.titleContent = new GUIContent("ControlCanvasEditorWindow");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        // VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        // root.Add(labelFromUXML);
        
        //var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ControlCanvas/Editor/ControlCanvasEditorWindow.uxml");
        //visualTree.CloneTree(root);
        
        m_VisualTreeAsset.CloneTree(root);
        
        // var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/ControlCanvas/Editor/ControlCanvasEditorWindow.uss");
        // root.styleSheets.Add(styleSheet);

        graphView = root.Q<ControlGraphView>();
        inspectorView = root.Q<InspectorView>();
        inspectorView.SetEditorWindow(this);

        var objectField = root.Q<ObjectField>("currentCanvas");
        if(objectField == null)
        {
            Debug.LogError("objectField is not found");
            return;
        }
        
        if (m_ControlCanvasSO_dataPath != null)
        {
            m_ControlCanvasSO = AssetDatabase.LoadAssetAtPath<ControlCanvasSO>(m_ControlCanvasSO_dataPath);
            objectField.value = m_ControlCanvasSO;
        }

        objectField.RegisterValueChangedCallback((evt) =>
        {
            OnControlCanvasChanged(evt.newValue as ControlCanvasSO);
        });
        OnControlCanvasChanged(m_ControlCanvasSO);
        graphView.OnSelectionChanged += inspectorView.OnSelectionChanged;
        
        
        blackboardView = root.Q<IMGUIContainer>("blackboardView");
        blackboardView.onGUIHandler = () =>
        {
            if (m_ControlCanvasSO != null)
            {
                canvasObject.Update();
                EditorGUILayout.PropertyField(blackboardProperty);
                canvasObject.ApplyModifiedProperties();
            }
        };
        
        root.Q<ToolbarButton>("save-button" ).clicked += () => SerializeDataAsXML();
        root.Q<ToolbarButton>("load-button" ).clicked += () => DeserializeDataFromXML();
        
        // if (m_ControlCanvasSO != null)
        // {
        //     graphView.PopulateView(m_ControlCanvasSO);
        // }
        // else
        // {
        //     graphView.ClerView();
        // }
    }

    public void OnControlCanvasChanged(ControlCanvasSO canvasSO)
    {
        
        if (canvasSO == null)
        {
            m_ControlCanvasSO = null;
            m_ControlCanvasSO_dataPath = "";
        }
        else
        {
            m_ControlCanvasSO = canvasSO;
            m_ControlCanvasSO_dataPath = AssetDatabase.GetAssetPath(m_ControlCanvasSO);
        }
        inspectorView.SetCurrentCanvas(m_ControlCanvasSO);
        graphView.PopulateView(m_ControlCanvasSO);
        if(m_ControlCanvasSO != null)
        {
            canvasObject = new SerializedObject(m_ControlCanvasSO);
            blackboardProperty = canvasObject.FindProperty("blackboard");
        }
    }
    
    public void SerializeDataAsXML()
    {
        if (m_ControlCanvasSO == null)
        {
            Debug.LogError("ControlCanvasSO is null");
            return;
        }
        var path = EditorUtility.SaveFilePanel("Save ControlCanvasSO as XML", "", "ControlCanvasSO", "xml");
        if (path.Length != 0)
        {
            SerializeToXML(path);
            AssetDatabase.Refresh();
        }
    }

    private void SerializeToXML(string path)
    {
        ControlCanvasData data = new ControlCanvasData();
        data.Nodes = new List<NodeData>();
        m_ControlCanvasSO.NodesCC.ForEach((node) =>
        {
            NodeData nodeData = new NodeData();
            nodeData.Guid = node.Guid;
            nodeData.Name = node.Name;
            nodeData.Position = node.Position;
            
            data.Nodes.Add(nodeData);
        });

        data.Edges = new List<EdgeData>();
        m_ControlCanvasSO.EdgesCC.ForEach((edge) =>
        {
            EdgeData edgeData = new EdgeData();
            edgeData.Guid = edge.Guid;
            edgeData.StartNodeGuid = edge.StartNodeGuid;
            edgeData.EndNodeGuid = edge.EndNodeGuid;
            
            data.Edges.Add(edgeData);
        });
        
        XmlSerializer serializer = new XmlSerializer(typeof(ControlCanvasData));
        using (StreamWriter writer = new StreamWriter(path))
        {
            serializer.Serialize(writer, data);
        }
    }

    public void DeserializeDataFromXML()
    {
        var path = EditorUtility.OpenFilePanel("Open ControlCanvasSO from XML", "", "xml");
        if (path.Length != 0)
        {
            DeserializeFromXML(path);
        }
    }

    private void DeserializeFromXML(string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ControlCanvasData));
        using (StreamReader reader = new StreamReader(path))
        {
            ControlCanvasData data = (ControlCanvasData)serializer.Deserialize(reader);
            if(m_ControlCanvasSO == null)
            {
                m_ControlCanvasSO = ScriptableObject.CreateInstance<ControlCanvasSO>();
                m_ControlCanvasSO_dataPath = AssetDatabase.GenerateUniqueAssetPath("Assets/ControlCanvas/ControlCanvasSO.asset");
                AssetDatabase.CreateAsset(m_ControlCanvasSO, m_ControlCanvasSO_dataPath);
            }
            m_ControlCanvasSO.NodesCC = new List<Node>();
            data.Nodes.ForEach((nodeData) =>
            {
                Node node = new Node();
                node.Guid = nodeData.Guid;
                node.Name = nodeData.Name;
                node.Position = nodeData.Position;
                
                m_ControlCanvasSO.NodesCC.Add(node);
            });
            m_ControlCanvasSO.EdgesCC = new List<Edge>();
            data.Edges.ForEach((edgeData) =>
            {
                Edge edge = new Edge();
                edge.Guid = edgeData.Guid;
                edge.StartNodeGuid = edgeData.StartNodeGuid;
                edge.EndNodeGuid = edgeData.EndNodeGuid;
                
                m_ControlCanvasSO.EdgesCC.Add(edge);
            });
            //OnControlCanvasChanged(m_ControlCanvasSO);
            EditorUtility.SetDirty(m_ControlCanvasSO);
        }
    }


    public enum NodeType
    {
        State,
        Behaviour,
        Decision
    }

}
