using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class DecisionStateTest : EditorTestBase
    {
        [Test]
        public void TestDecisionAsRoot()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionStateTests/DecisionAsRoot.xml");
            string guidNode1 = "3be38891-4300-4459-b36e-a73da96a9611";
            string guidNode2 = "dc1b1791-998a-4209-bc4c-e6f496d84f45";
            string guidNode3 = "8339553a-2726-4553-9b24-3fe719813c4e";
            string guidNode4 = "347d0f95-77eb-4c21-bb40-f3d60a2d208a";

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
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode3,
                guidNode4,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestDecisionTransitions()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionStateTests/DecisionTransition.xml");
            string guidNode1 = "9a0f90b3-3921-4ac1-b317-effa0e79c7a7";
            string guidNode2 = "6211c21e-1a24-4a27-8f37-eebca4cf863d";
            string guidNode3 = "98419860-2127-4bb9-bdd4-f0f48d70f41d";
            string guidNode4 = "ea108815-27c4-4065-b2d2-ddf3eca120f3";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode1,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
                guidNode3,
            });
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode3,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode3,
                guidNode4,
                guidNode1,
            });
            
            CleanUpTest();
        }
    }
}