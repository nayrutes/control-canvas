using System;
using ControlCanvas.Editor;
using ControlCanvas.Editor.Extensions;
using ControlCanvas.Editor.ViewModels;
using ControlCanvas.Editor.Views;
using ControlCanvas.Runtime;
using UniRx;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = ControlCanvas.Editor.Debug;

public class ControlCanvasEditorWindow : EditorWindow, IDisposable
{
    [SerializeField] private VisualTreeAsset m_VisualTreeAsset = default;
    private const string ControlFlowPlayerPrefsKey = "ControlFlowPath";

    private CanvasViewModel m_CanvasViewModel;
    private ControlGraphView graphView;
    private InspectorView inspectorView;
    
    private Label canvasNameLabel;
    private Label canvasPathLabel;
    CompositeDisposable disposables = new ();

    ObjectField debugRunnerField;

    DebugLinker debugLinker;
    
    private Button playButton;
    private Button stopButton;
    private Button stepButton;

    [MenuItem("Window/UI Toolkit/ControlCanvasEditorWindow")]
    public static void OpenWindow()
    {
        ControlCanvasEditorWindow wnd = GetWindow<ControlCanvasEditorWindow>();
        wnd.titleContent = new GUIContent("ControlCanvasEditorWindow");
    }

    public void CreateGUI()
    {
        //Debug.Log("CreateGUI");

        SetUpViewModel();
        SetUpView();
        SetUpSubscriptions();
        AutoLoadLastFile();
        
    }

    private void SetUpViewModel()
    {
        if (m_CanvasViewModel != null) return;

        //Debug.Log("SetUpViewModel");
        m_CanvasViewModel = new CanvasViewModel();
    }
    
    private void SetUpView()
    {
        m_VisualTreeAsset.CloneTree(rootVisualElement);
        graphView = rootVisualElement.Q<ControlGraphView>();
        inspectorView = rootVisualElement.Q<InspectorView>();
        rootVisualElement.Q<ToolbarButton>("save-button").clicked += () => SerializeDataAsXML(true);
        rootVisualElement.Q<ToolbarButton>("load-button").clicked += () => DeserializeDataFromXML();
        rootVisualElement.Q<ToolbarButton>("new-button").clicked += () => NewXML();
        canvasNameLabel = rootVisualElement.Q<Label>("canvas-name");
        canvasPathLabel = rootVisualElement.Q<Label>("canvas-path");
        debugRunnerField = rootVisualElement.Q<ObjectField>("debug-runner");
        
        playButton = rootVisualElement.Q<Button>("Play");
        stopButton = rootVisualElement.Q<Button>("Stop");
        stepButton = rootVisualElement.Q<Button>("Step");
    }

    private void SetUpSubscriptions()
    {
        graphView.OnSelectionChanged += m_CanvasViewModel.OnSelectionChanged;
        graphView.SetViewModel(m_CanvasViewModel.GraphViewModel);
        inspectorView.SetViewModel(m_CanvasViewModel.InspectorViewModel);

        m_CanvasViewModel.DataProperty.DoWithLast(x => { disposables.Clear(); }).Subscribe(x =>
        {
            if (x != null)
            {
                m_CanvasViewModel.canvasName.Subscribe(x => { canvasNameLabel.text = x; }).AddTo(disposables);
                m_CanvasViewModel.canvasPath.Subscribe(x => { canvasPathLabel.text = x; }).AddTo(disposables);
            }
        });
        
        
        
        debugRunnerField.RegisterValueChangedCallback(evt =>
        {
            debugLinker?.Unlink();
            var runnerMono = evt.newValue as ControlRunnerMono;
            if (runnerMono == null) return;
            ControlRunner runner = runnerMono.GetControlRunner();
            if(runner == null) return;
            debugLinker = new DebugLinker(runner, m_CanvasViewModel);
            debugLinker.SetButtons(playButton, stopButton, stepButton);
            debugLinker.Link();
        });
        
        Selection.selectionChanged += OnSelectionChanged;
        OnSelectionChanged();
        
        //register ctrl+s key event
        rootVisualElement.RegisterCallback<KeyDownEvent>(evt =>
        {
            if (evt.keyCode == KeyCode.S && evt.ctrlKey)
            {
                SerializeDataAsXML(evt.shiftKey);
                evt.StopPropagation();
            }
        });
    }

    private void OnSelectionChanged()
    {
        var selectedObject = Selection.activeObject;
        if (selectedObject == null) return;
        var controlRunnerGo = selectedObject as GameObject;
        if (controlRunnerGo == null) return;
        var controlRunnerMono = controlRunnerGo.GetComponent<ControlRunnerMono>();
        if (controlRunnerMono == null) return;
        debugRunnerField.value = controlRunnerMono;
    }

    private void AutoLoadLastFile()
    {
        var lastFile = EditorPrefs.GetString(ControlFlowPlayerPrefsKey);
        if (string.IsNullOrEmpty(lastFile)) return;
        m_CanvasViewModel.DeserializeData(lastFile);
    }

    private void OnEnable()
    {
        //Debug.Log("OnEnable");
        EditorApplication.quitting += OnEditorApplicationQuitting;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnDisable()
    {
        //Debug.Log("OnDisable");
        EditorApplication.quitting -= OnEditorApplicationQuitting;
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private void OnEditorApplicationQuitting()
    {
        //Debug.Log("OnEditorApplicationQuitting");
        Dispose();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            //Debug.Log($"OnPlayModeStateChanged: {state}");
            Dispose();
        }
        else if (state == PlayModeStateChange.ExitingPlayMode)
        {
            debugLinker?.Unlink();
            //Debug.Log($"OnPlayModeStateChanged: {state}");
            //Dispose();
        }
    }

    private void OnDestroy()
    {
        Dispose();
    }

    [InitializeOnLoadMethod]
    private static void InitializeOnLoad()
    {
        CompilationPipeline.compilationStarted += OnCompilationStartedStatic;
    }

    private static void OnCompilationStartedStatic(object obj)
    {
        // Get a reference to the window
        var window = GetWindow<ControlCanvasEditorWindow>();
        window.OnCompilationStarted();
    }

    private void OnCompilationStarted()
    {
        //Debug.Log("OnCompilationStarted");
        Dispose();
    }

    public void SerializeDataAsXML(bool forcePopup = false)
    {
        string directory = "";
        string fileName = "ControlCanvasSO";
        string path = "";
        if (!String.IsNullOrEmpty(m_CanvasViewModel.canvasPath.Value))
        {
            try
            {
                path = m_CanvasViewModel.canvasPath.Value;
                directory = System.IO.Path.GetDirectoryName(m_CanvasViewModel.canvasPath.Value);
                fileName = System.IO.Path.GetFileNameWithoutExtension(m_CanvasViewModel.canvasPath.Value);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                forcePopup = true;
                //Console.WriteLine(e);
                //throw;
            }
        }

        if(forcePopup || string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(fileName))
        {
            path = EditorUtility.SaveFilePanel("Save ControlCanvasSO as XML", directory, fileName, "xml");
        }
        if (path.Length != 0)
        {
            m_CanvasViewModel.SerializeData(path);
            EditorPrefs.SetString(ControlFlowPlayerPrefsKey, path);
        }
    }


    public void DeserializeDataFromXML()
    {
        var path = EditorUtility.OpenFilePanel("Open ControlCanvasSO from XML", "", "xml");
        if (path.Length != 0)
        {
            m_CanvasViewModel.DeserializeData(path);
            EditorPrefs.SetString(ControlFlowPlayerPrefsKey, path);
        }
    }

    public void NewXML()
    {
        m_CanvasViewModel.NewData();
    }
    
    private void ReleaseUnmanagedResources()
    {
    }

    private void Dispose(bool disposing)
    {
        ReleaseUnmanagedResources();
        if (disposing)
        {
            disposables?.Dispose();
            m_CanvasViewModel?.Dispose();
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