using System.Collections.Generic;
using NUnit.Framework;

namespace ControlCanvas.Tests.EditorTests
{
    public class ParallelTest : EditorTestBase
    {
        [Test]
        public void TestDecisionBeforeBehaviour()
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
    }
}