using System;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.Views;
using UniRx;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public class ControlCanvasEditorWindow : EditorWindow, IDisposable
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
            disposables.Add(m_CanvasViewModel);
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

    private void OnEnable()
    {
        EditorApplication.quitting += OnEditorApplicationQuitting;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        EditorApplication.quitting -= OnEditorApplicationQuitting;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }
    
    private void OnEditorApplicationQuitting()
    {
        Dispose();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.ExitingPlayMode)
        {
            Dispose();
        }
    }
    
    private void OnDestroy()
    {
        Dispose();
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        CompilationPipeline.compilationStarted += OnCompilationStarted;
    }
    
    private static void OnCompilationStarted(object obj)
    {
        // Get a reference to the window
        var window = GetWindow<ControlCanvasEditorWindow>();
        window.Dispose();
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


    private void ReleaseUnmanagedResources()
    {
        // TODO release unmanaged resources here
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            disposables?.Dispose();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~ControlCanvasEditorWindow()
    {
        Debug.LogWarning(
            $"Dispose was not called on {this.GetType()}. Using Finalizer to dispose because unity event did not trigger.");
        Dispose(false);
    }
}