using System.Collections;
using System.Collections.Generic;
using ControlCanvas.Runtime;
using NUnit.Framework;
using UniRx;
//using UnityEngine;

public class EditorTestBase
{
    protected CompositeDisposable disposables;
    protected ControlRunner controlRunner;
    protected ControlAgentDebug controlAgent;
    protected string testMessage = "";
    public void SetUpTest(string path)
    {
        //Debug.Log("Testing on path: " + path);
        testMessage = GetTestMessage();
        disposables = new CompositeDisposable();
        controlRunner = new ControlRunner();
        controlAgent = new ControlAgentDebug(controlRunner);
        controlRunner.Initialize(path, controlAgent);
    }

    protected virtual string GetTestMessage()
    {
        return "";
    }

    public void CleanUpTest()
    {
        disposables.Dispose();
        disposables = null;
        controlRunner = null;
        controlAgent = null;
        testMessage = "";
    }
    public void AssertLogExecutionOrder(List<string> expected, List<string> actual)
    {
        //Assert.AreEqual(expected.Count, actual.Count);
        Assert.True(expected.Count == actual.Count, $"Expected count {expected.Count} but was {actual.Count}");
        for (int i = 0; i < expected.Count; i++)
        {
            //Assert.AreEqual($"{testMessage}{expected[i]}", actual[i]);
            Assert.True($"{testMessage}{expected[i]}" == actual[i], $"Expected {testMessage}{expected[i]} but was {actual[i]}");
        }
    }

    public void AssertExecutionOrderAndType(List<string> expected)
    {
        AssertLogExecutionOrder(expected, controlAgent.Log1);
        //Assert.IsNotNull(controlRunner.GetNextSuggestion());
    }
    
    public void AssertExecutionOrderByGUIDOnly(List<string> expected)
    {
        AssertLogExecutionOrder(expected, controlAgent.Log2);
    }
}
