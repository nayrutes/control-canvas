using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.Views;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlCanvasEditorWindow : EditorWindow
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;

    CanvasViewModel m_CanvasViewModel;

    private ControlGraphView graphView;
    private InspectorView inspectorView;
    private IMGUIContainer blackboardView;

    public SerializedObject canvasObject;
    private SerializedProperty blackboardProperty;

    CompositeDisposable disposables = new CompositeDisposable();


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
        m_VisualTreeAsset.CloneTree(root);
        graphView = root.Q<ControlGraphView>();
        inspectorView = root.Q<InspectorView>();
        if (m_CanvasViewModel == null)
        {
            m_CanvasViewModel = new CanvasViewModel();
        }

        graphView.SetViewModel(m_CanvasViewModel.GraphViewModel);
        inspectorView.SetViewModel(m_CanvasViewModel.InspectorViewModel);

        root.Q<ToolbarButton>("save-button").clicked += () => SerializeDataAsXML();
        root.Q<ToolbarButton>("load-button").clicked += () => DeserializeDataFromXML();

        Label canvasNameLabel = root.Q<Label>("canvas-name");
        m_CanvasViewModel.canvasName.Subscribe(x => { canvasNameLabel.text = x; }).AddTo(disposables);

        graphView.OnSelectionChanged += m_CanvasViewModel.OnSelectionChanged;

        //TODO: Save and load last file from EditorPrefs or PlayerPrefs
    }

    private void OnDestroy()
    {
        disposables.Dispose();
    }

    public void SerializeDataAsXML()
    {
        var path = EditorUtility.SaveFilePanel("Save ControlCanvasSO as XML", "", "ControlCanvasSO", "xml");
        if (path.Length != 0)
        {
            m_CanvasViewModel.SerializeData(path);
        }
    }


    public void DeserializeDataFromXML()
    {
        var path = EditorUtility.OpenFilePanel("Open ControlCanvasSO from XML", "", "xml");
        if (path.Length != 0)
        {
            m_CanvasViewModel.DeserializeData(path);
        }
    }


}