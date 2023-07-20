using System;
using ControlCanvas.Editor;
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
            if (evt.newValue is not ControlCanvasSO canvasSO)
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
        });
        inspectorView.SetCurrentCanvas(m_ControlCanvasSO);
        graphView.PopulateView(m_ControlCanvasSO);
        graphView.OnSelectionChanged += inspectorView.OnSelectionChanged;
        
        // if (m_ControlCanvasSO != null)
        // {
        //     graphView.PopulateView(m_ControlCanvasSO);
        // }
        // else
        // {
        //     graphView.ClerView();
        // }
    }

}
