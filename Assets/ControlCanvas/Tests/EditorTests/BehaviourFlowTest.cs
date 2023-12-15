using System.Collections.Generic;
using NUnit.Framework;

namespace ControlCanvas.Tests.EditorTests
{
    public class StanaloneTest
    {
        
    }
    
    public class BehaviourFlowTest : EditorTestBase
    {
        protected override string GetTestMessage()
        {
            return "DebugBehaviour.OnUpdate of ";
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
            AssertExecutionOrderAndType(new List<string>()
            {
                guid
            });
            
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
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode2
            });
            
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
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode2
            });
            
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
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2
            });
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourFlowRerun()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            //string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            //string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/SingleNodeRerun.xml");
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(0.5f);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
            });
            controlRunner.RunningUpdate(2);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode1,
            });
            CleanUpTest();
        }

        [Test]
        public void EmptyTest()
        {
            int i = 1;
            Assert.True(i == 1);
            //Assert.AreEqual(i, 1);
        }
        
        [Test]
        public void TestBehaviourFlowWait()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            //string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/ThreeNodesWait.xml");
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(4);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                
                guidNode1,
            });
            controlRunner.RunningUpdate(2);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                
                guidNode1,
                
                guidNode1,
                guidNode2,
            });
            controlRunner.RunningUpdate(3);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                
                guidNode1,
                
                guidNode1,
                guidNode2,
                
                guidNode1,
            });
            controlRunner.RunningUpdate(1.1f);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                
                guidNode1,
                
                guidNode1,
                guidNode2,
                
                guidNode1,
                
                guidNode1,
                //Node 2 will not be called because overlapping time was dismissed before
            });
            controlRunner.RunningUpdate(1f);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                
                guidNode1,
                
                guidNode1,
                guidNode2,
                
                guidNode1,
                
                guidNode1,
                
                guidNode1,
                guidNode2,
            });
            CleanUpTest();
        }
        
        // [Test]
        // public void TestBehaviourSubFlow()
        // {
        //     string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
        //     string guidNode2 = "0113112f-89bb-41c1-a167-c17bd8145552";
        //     SetUpTest("Assets/ControlFlows/Tests/SubFlowParent.xml");
        //     controlRunner.RunningUpdate(0);
        //     AssertExecutionOrderAndType(new List<string>()
        //     {
        //         guidNode1,
        //         guidNode2,
        //     });
        //     CleanUpTest();
        // }
        
        [Test]
        public void TestBehaviourRoutingNode()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/RoutingNode.xml");
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
            });
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourRepeaterLoopSuccess()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            //string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/RepeaterLoopSuccess.xml");
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode3,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode3,
                guidNode3,
            });
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourRepeaterLoopFailure()
        {
            string guidNode1 = "f10575fb-a58e-4eb5-b4e5-c50c48bdc1fa";
            string guidNode2 = "7d0cb3cc-a1e9-4bb1-8419-fc0f4d9ed361";
            string guidNode3 = "5605018b-7d0f-4927-b711-e9a14dbe23fb";
            SetUpTest("Assets/ControlFlows/Tests/RepeaterLoopFailure.xml");
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode2,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode2,
            });
            CleanUpTest();
        }
    }
}