using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class StateDecisionBehaviourTest : EditorTestBase
    {
        [Test]
        public void TestDecisionBeforeBehaviour()
        {
            SetUpTest("Assets/ControlFlows/Tests/StateDecisionBehaviourTests/BehaviourAfterStateAndDecision.xml");
            string guidNode1 = "bcde7d2f-0178-4266-a288-95f2a3dab9c0";
            string guidNode2 = "4a358c6c-8cd4-4e48-ad0a-597a7c8383f1";
            string guidNode3 = "f92b4425-d785-419e-95aa-d565f822e517";
            string guidNode4 = "fa5292b0-8b74-4a55-a7e9-20a997a56e0e";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
            });
            controlAgent.DebugBlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
            });
            controlAgent.DebugBlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
            });
            
            //Now that the last behaviour had only a null transition, the flow should restart automatically from the last state
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
                guidNode2,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestBehaviourToState()
        {
            SetUpTest("Assets/ControlFlows/Tests/StateDecisionBehaviourTests/DecisionToBehaviourToState.xml");
            string guidNode1 = "f92b4425-d785-419e-95aa-d565f822e517";
            string guidNode2 = "fa5292b0-8b74-4a55-a7e9-20a997a56e0e";
            string guidNode3 = "4a358c6c-8cd4-4e48-ad0a-597a7c8383f1";

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
                guidNode3,
            });
            
            controlAgent.DebugBlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode3,
                guidNode1,
                guidNode2,
                guidNode3,
            });
            
            
            CleanUpTest();
        }
    }
}