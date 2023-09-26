using System.Collections;
using System.Collections.Generic;
using ControlCanvas.Runtime;
using NUnit.Framework;
using UniRx;
using UnityEngine;

public class EditorTestBase
{
    protected CompositeDisposable disposables;
    protected ControlRunner controlRunner;
    protected ControlAgentDebug controlAgent;
    protected string testMessage = "";
    public void SetUpTest(string path)
    {
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
        Assert.AreEqual(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            Assert.AreEqual($"{testMessage}{expected[i]}", actual[i]);
        }
    }
        
    public void AssertBehaviourExecutionOrder(List<string> expected)
    {
        AssertLogExecutionOrder(expected, controlAgent.Log1);
        Assert.IsNull(controlRunner.GetNextSuggestion());
    }

    public void AssertStateExecutionOrder(List<string> expected)
    {
        AssertLogExecutionOrder(expected, controlAgent.Log1);
        Assert.IsNotNull(controlRunner.GetNextSuggestion());
    }
}
