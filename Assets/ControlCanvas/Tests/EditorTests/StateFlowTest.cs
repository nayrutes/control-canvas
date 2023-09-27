using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class StateFlowTest : EditorTestBase
    {
        protected override string GetTestMessage()
        {
            return "Execute of ";
        }

        [Test]
        public void TestStateSingle()
        {
            SetUpTest("Assets/ControlFlows/Tests/StateTests/SingleState.xml");
            string guidNode1 = "2e501c23-c2eb-4005-bcec-49f42c626653";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestStateTwo()
        {
            SetUpTest("Assets/ControlFlows/Tests/StateTests/TwoStates.xml");
            string guidNode1 = "2e501c23-c2eb-4005-bcec-49f42c626653";
            string guidNode2 = "dfa1a108-9723-40da-9c66-95ccaebbba7f";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
            });
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
            });
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
                guidNode2,
            });
            
            controlAgent.BlackboardAgent.ExitEvent.OnNext(Unit.Default);
            
            controlRunner.RunningUpdate(1);
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode1,
                guidNode2,
                guidNode2,
                guidNode1
            });
            
            CleanUpTest();
        }
    }
}
