using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class ParallelTest : EditorTestBase
    {
        [Test]
        public void TestParallelOneNode()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/ParallelBehaviour.xml");
            string guidNode1 = "bf51f8c1-18cd-4039-a095-9deada8b70d3";
            string guidNode2 = "f9f4c77b-de42-4f66-815a-966c365cf1f3";
            string guidNode3 = "5c0d419a-b716-4bc8-9ca3-a56648d0df3e";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestParallelOneNodeRerun()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/ParallelBehaviour.xml");
            string guidNode1 = "bf51f8c1-18cd-4039-a095-9deada8b70d3";
            string guidNode2 = "f9f4c77b-de42-4f66-815a-966c365cf1f3";
            string guidNode3 = "5c0d419a-b716-4bc8-9ca3-a56648d0df3e";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
            });
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode1,
                guidNode2,
                guidNode3,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestParallelTwoNodes()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/ParallelBehaviour2.xml");
            string guidNode1 = "bf51f8c1-18cd-4039-a095-9deada8b70d3";
            string guidNode2 = "f9f4c77b-de42-4f66-815a-966c365cf1f3";
            string guidNode3 = "5c0d419a-b716-4bc8-9ca3-a56648d0df3e";
            string guidNode4 = "7a95d88d-4d07-44fe-8f12-73f190fdca1b";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourParallelInState()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/BehaviourParallelInState.xml");
            string guidNode1 = "898f64b6-a9b4-4308-8f76-d1eda0b9b144";
            string guidNode2 = "5a36113b-41cb-4602-8ccc-5fba1de84c71";
            string guidNode3 = "7cfdb323-0c54-4ab9-ad22-0cfd632b1541";
            string guidNode4 = "01b7e967-a263-4fc8-b0bc-f4f955a86dcb";
            string guidNode5 = "f661caab-058e-483c-acb3-b338c9214452";
            string guidNode6 = "1d3490fa-bf1f-4e6d-8db5-0c24b2789166";

            controlRunner.RunningUpdate(1);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode4,
            });
            
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode4,
                guidNode2,
                guidNode4,
            });
            
            controlRunner.RunningUpdate(5);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode4,
                guidNode2,
                guidNode4,
                guidNode2,
                guidNode4,
                //guidNode5,
                guidNode6,
            });
            
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode4,
                guidNode2,
                guidNode4,
                guidNode2,
                guidNode4,
                //guidNode5,
                guidNode6,
                guidNode3,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestStateInState()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/StateInState.xml");
            string guidNode1 = "5b7b45b8-c721-44cd-a3e5-12643357bb10";
            string guidNode2 = "a1698699-bda9-49a2-97f3-80698790eff6";
            string guidNode3 = "19465efa-417b-4c5a-9a12-277499491a6a";
            string guidNode4 = "2bdf6cc8-52a3-4c43-9e29-bb8336c5715e";
            string guidNode5 = "6db6562a-65be-4701-bf08-d3b50276601e";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
            });
            
            controlAgent.BlackboardAgent.OtherExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode1,
                guidNode4,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode1,
                guidNode4,
                guidNode2,
                guidNode1,
                guidNode4,
            });
            
            controlAgent.BlackboardAgent.OtherExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode3,
                guidNode1,
                guidNode4,
                guidNode2,
                guidNode1,
                guidNode4,
                guidNode1,
                guidNode5,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestStateInStateEarlyExitEvent()
        {
            SetUpTest("Assets/ControlFlows/Tests/ParallelTests/StateInState.xml");
            string guidNode1 = "5b7b45b8-c721-44cd-a3e5-12643357bb10";
            string guidNode2 = "a1698699-bda9-49a2-97f3-80698790eff6";
            string guidNode3 = "19465efa-417b-4c5a-9a12-277499491a6a";
            string guidNode4 = "2bdf6cc8-52a3-4c43-9e29-bb8336c5715e";
            string guidNode5 = "6db6562a-65be-4701-bf08-d3b50276601e";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            controlAgent.BlackboardAgent.OtherExitEvent.OnNext(Unit.Default);
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode3,
                guidNode2,
                guidNode1,
                guidNode4,//4 and not 3 because subscription is not removed
            });
            
            CleanUpTest();
        }
    }
}