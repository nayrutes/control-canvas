using UnityEditor;
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

        private void OnEnable()
        {
            VisualElement root = rootVisualElement;
            DataContainer dataContainer = new DataContainer();
            GenericViewModel genericViewModel = GenericViewModel.GetViewModel(dataContainer);
            genericViewModel.Log();
            root.Clear();
            root.Add(GenericField.CreateGenericInspector(dataContainer));
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
        public string testString;
        public float testFloat;
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
    }

    public class DataContainer2
    {
        public int testInt2;
    }
}