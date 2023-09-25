using System.Collections.Generic;
using ControlCanvas.Runtime;
using NSubstitute;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class BehaviourFlowTest
    {
        CompositeDisposable disposables;
        ControlRunner controlRunner;
        ControlAgentDebug controlAgent;
        
        public void SetUpTest(string path)
        {
            
            disposables = new CompositeDisposable();
            controlRunner = new ControlRunner();
            controlAgent = new ControlAgentDebug(controlRunner);
            controlRunner.Initialize(path, controlAgent);
        }
        
        public void CleanUpTest()
        {
            disposables.Dispose();
            disposables = null;
            controlRunner = null;
            controlAgent = null;
        }
        
        public void AssertUpdateExecutionOrder(List<string> expected)
        {
            Assert.AreEqual(expected.Count, controlAgent.Log.Count);
            for (int i = 0; i < expected.Count; i++)
            {
                Assert.AreEqual($"DebugBehaviour.OnUpdate of {expected[i]}", controlAgent.Log[i]);
            }
            Assert.IsNull(controlRunner.GetNextSuggestion());
        }
        
        [Test]
        public void TestBehaviourFlowSingleInit()
        {
            //ARRANGE
            SetUpTest("Assets/ControlFlows/Tests/SingleNodeInit.xml");
            string guid = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";

            //ASSERT Subscriptions
            //check if next node is null
            //controlRunner.StepDoneNext.Subscribe(Assert.IsNull).AddTo(disposables);
            
            //ACT
            controlRunner.RunningUpdate(0);
            
            //ASSERT
            //check if node has executed
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guid}",controlAgent.Log[0]);
            Assert.AreEqual(1,controlAgent.Log.Count);
            Assert.IsNull(controlRunner.GetNextSuggestion());
            
            //CLEANUP
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourFlowTwoSuccess()
        {
            //ARRANGE
            SetUpTest("Assets/ControlFlows/Tests/TwoNodesSuccess.xml");
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";

            //ACT
            controlRunner.RunningUpdate(0);
            
            //ASSERT
            //check if node has executed
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode1}",controlAgent.Log[0]);
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode2}",controlAgent.Log[1]);
            Assert.AreEqual(2,controlAgent.Log.Count);
            Assert.IsNull(controlRunner.GetNextSuggestion());
            
            //CLEANUP
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourFlowTwoFailure()
        {
            //ARRANGE
            SetUpTest("Assets/ControlFlows/Tests/TwoNodesFailure.xml");
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";

            //ACT
            controlRunner.RunningUpdate(0);
            
            //ASSERT
            //check if node has executed
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode1}",controlAgent.Log[0]);
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode2}",controlAgent.Log[1]);
            Assert.AreEqual(2,controlAgent.Log.Count);
            Assert.IsNull(controlRunner.GetNextSuggestion());
            
            //CLEANUP
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourFlowSuccessAndFailure()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/ThreeNodesSuccessAndFailure.xml");
            controlRunner.RunningUpdate(0);
            AssertUpdateExecutionOrder(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2
            });
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourFlowWait()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/ThreeNodesWait.xml");
            controlRunner.RunningUpdate(0);
            AssertUpdateExecutionOrder(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(4);
            AssertUpdateExecutionOrder(new List<string>()
            {
                guidNode1,
                guidNode1,
            });
            controlRunner.RunningUpdate(2);
            AssertUpdateExecutionOrder(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode1,
                guidNode2,
            });
            CleanUpTest();
        }
    }
}