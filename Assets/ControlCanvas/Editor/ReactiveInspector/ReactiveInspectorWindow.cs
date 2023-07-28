using System;
using System.Collections.Generic;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{
    public class ReactiveInspectorWindow : EditorWindow
    {
        [MenuItem("Window/Test ReactiveInspectorWindow")]
        public static void ShowWindow()
        {
            GetWindow<ReactiveInspectorWindow>("My Custom Inspector");
        }

        private DataContainer dataContainer;
        GenericViewModel genericViewModel;
        private CompositeDisposable _compositeDisposable = new CompositeDisposable();
        private CompositeDisposable _viewDisposableCollection = new();

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;
            dataContainer = new DataContainer();
            ReloadView();
            
            genericViewModel = GenericViewModel.GetViewModel(dataContainer);
            genericViewModel.DataChanged.Subscribe(b =>
            {
                string title = b ? "My Custom Inspector *" : "My Custom Inspector";
                titleContent = new GUIContent(title);
            }).AddTo(_compositeDisposable);
        }

        private void OnDisable()
        {
            _compositeDisposable.Dispose();
            genericViewModel?.Dispose();
        }

        private void ReloadView()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            
            root.Add(new Button(ReloadView) { text = "Reload View" });
            root.Add(new Button(ReloadData) { text = "Reload Data" });
            root.Add(new Button(SaveData) { text = "Save Data" });
            //GenericViewModel genericViewModel = GenericViewModel.GetViewModel(dataContainer);
            //genericViewModel.Log();
            _viewDisposableCollection.Dispose();
            _viewDisposableCollection = new CompositeDisposable();
            root.Add(GenericField.CreateGenericInspector(dataContainer, _viewDisposableCollection));
        }
        
        //LoadFromDataContainer
        private void ReloadData()
        {
            GenericViewModel.ReloadViewModel(dataContainer);
        }


        //SaveToDataContainer
        private void SaveData()
        {
            GenericViewModel.SaveDataFromViewModel(dataContainer);
        }
    }

    public enum MyEnumTest1
    {
        Entry1, Entry2, Entry3
    }
    
    public enum MyEnumTest2
    {
        Hugo, Peter, Paul
    }
    
    public class DataContainer
    {
        public int testInt;
        public int testInt2 = 42;
        public string testString;
        public float testFloat = 1.993f;
        public bool testBool;
        public int[] testIntArray;
        public string[] testStringArray = new[] { "a", "b", "c" };
        public float[] testFloatArray;
        public bool[] testBoolArray = new[] { true, false, true, false, true, false };
        public int[,] testInt2DArray = new int[3, 3] { { 1, 2, 12 }, { 3, 4, 34 }, { 5, 6, 56 } };
        public int[,,] testInt3DArray = new int[2, 2, 2] { { { 1, 2 }, { 3, 4 } }, { { 5, 6 }, { 7, 8 } } };
        
        public MyEnumTest1 TestEnumTest1;
        public MyEnumTest2 AnotherTestEnumTest2 = MyEnumTest2.Paul;
        // public int[][] testIntJaggedArray = new int[2][] { new int[2] { 1, 2 }, new int[2] { 3, 4 } };
        // public string[][] testStringJaggedArray = new string[2][] { new string[2] { "a", "b" }, new string[2] { "c", "d" } };

        public DataContainer2 testContainer2 = new DataContainer2()
        {
            testInt2 = 2
        };

        public DataContainer2[] testContainer2Array = new DataContainer2[2]
        {
            new DataContainer2()
            {
                testInt2 = 3
            },
            new DataContainer2()
            {
                testInt2 = 4
            }
        };
        
        public void LogData()
        {
            Debug.Log($"testInt: {testInt}");
            Debug.Log($"testInt2: {testInt2}");
            Debug.Log($"testString: {testString}");
            Debug.Log($"testFloat: {testFloat}");
            Debug.Log($"testBool: {testBool}");
            Debug.Log($"testIntArray: {testIntArray}");
            Debug.Log($"testStringArray: {testStringArray}");
            Debug.Log($"testFloatArray: {testFloatArray}");
            Debug.Log($"testBoolArray: {testBoolArray}");
            Debug.Log($"testInt2DArray: {testInt2DArray}");
            Debug.Log($"testInt3DArray: {testInt3DArray}");
            Debug.Log($"TestEnumTest1: {TestEnumTest1}");
            Debug.Log($"AnotherTestEnumTest2: {AnotherTestEnumTest2}");
            Debug.Log($"testContainer2: {testContainer2}");
            Debug.Log($"testContainer2Array: {testContainer2Array}");
        }
    }

    public class DataContainer2
    {
        public int testInt2;
    }
}