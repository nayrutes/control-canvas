using ControlCanvas.Runtime;
using NSubstitute;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class BehaviourFlowTest
    {
        [Test]
        public void TestBehaviourFlowSingleInit()
        {
            //ARRANGE
            CompositeDisposable disposables = new CompositeDisposable();
            ControlRunner controlRunner = new ControlRunner();
            ControlAgentDebug controlAgent = new ControlAgentDebug();
            string path = "Assets/ControlFlows/Tests/SingleNodeInit.xml";
            string guid = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            controlRunner.Initialize(path, controlAgent);

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
            disposables.Dispose();
            NodeManager.Instance = null;
        }
        
        [Test]
        public void TestBehaviourFlowTwoSuccess()
        {
            //ARRANGE
            CompositeDisposable disposables = new CompositeDisposable();
            ControlRunner controlRunner = new ControlRunner();
            ControlAgentDebug controlAgent = new ControlAgentDebug();
            string path = "Assets/ControlFlows/Tests/TwoNodesSuccess.xml";
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            controlRunner.Initialize(path, controlAgent);

            //ACT
            controlRunner.RunningUpdate(0);
            
            //ASSERT
            //check if node has executed
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode1}",controlAgent.Log[0]);
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode2}",controlAgent.Log[1]);
            Assert.AreEqual(2,controlAgent.Log.Count);
            Assert.IsNull(controlRunner.GetNextSuggestion());
            
            //CLEANUP
            disposables.Dispose();
            NodeManager.Instance = null;
        }
        
        [Test]
        public void TestBehaviourFlowTwoFailure()
        {
            //ARRANGE
            CompositeDisposable disposables = new CompositeDisposable();
            ControlRunner controlRunner = new ControlRunner();
            ControlAgentDebug controlAgent = new ControlAgentDebug();
            string path = "Assets/ControlFlows/Tests/TwoNodesFailure.xml";
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            controlRunner.Initialize(path, controlAgent);

            //ACT
            controlRunner.RunningUpdate(0);
            
            //ASSERT
            //check if node has executed
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode1}",controlAgent.Log[0]);
            Assert.AreEqual( $"DebugBehaviour.OnUpdate of {guidNode2}",controlAgent.Log[1]);
            Assert.AreEqual(2,controlAgent.Log.Count);
            Assert.IsNull(controlRunner.GetNextSuggestion());
            
            //CLEANUP
            disposables.Dispose();
            NodeManager.Instance = null;
        }
    }
}