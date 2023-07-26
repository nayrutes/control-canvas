﻿
using UnityEditor;
using UnityEngine.UIElements;

namespace ControlCanvas.Editor.ReactiveInspector
{

    public class PlanetsTreeView : PlanetsWindow
    {
        [MenuItem("Planets/Standard Tree")]
        static void Summon()
        {
            GetWindow<PlanetsTreeView>("Standard Planet Tree");
        }

        void CreateGUI()
        {
            VisualTreeAsset visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/ControlCanvas/Editor/ReactiveInspector/GenericTreeView.uxml");
            visualTree.CloneTree(rootVisualElement);
            
            //uxml.CloneTree(rootVisualElement);
            var treeView = rootVisualElement.Q<TreeView>();

            // Call TreeView.SetRootItems() to populate the data in the tree.
            treeView.SetRootItems(treeRoots);

            // Set TreeView.makeItem to initialize each node in the tree.
            treeView.makeItem = () => new Label();

            // Set TreeView.bindItem to bind an initialized node to a data item.
            treeView.bindItem = (VisualElement element, int index) =>
                (element as Label).text = treeView.GetItemDataForIndex<IPlanetOrGroup>(index).name;
        }
    }
}