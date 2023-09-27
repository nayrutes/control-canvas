using System.Collections.Generic;
using NUnit.Framework;
using UniRx;

namespace ControlCanvas.Tests.EditorTests
{
    public class DecisionBehaviourTest : EditorTestBase
    {
        [Test]
        public void TestDecisionInBehaviourFlow()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionBehaviourTests/DecisionsInBehaviourFlow.xml");
            string guidNode1 = "a8506dc0-c788-4c07-a53f-babcd27b7818";
            string guidNode2 = "fa4598a8-81e2-4f98-9a41-ab8513ea5be0";
            string guidNode3 = "0588f10b-ba45-4799-ad79-7ff2ecc1e45a";
            string guidNode4 = "50aa82b6-1661-4497-9bce-b05861b18dc6";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4
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
                guidNode4
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestDecisionBeforeBehaviour()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionBehaviourTests/DecisionsBeforeBehaviour.xml");
            string guidNode1 = "925c6166-b75b-4a6e-b53b-3e7d0a09411c";
            string guidNode2 = "a9734632-2840-4a25-8342-296a4bf02f0d";

            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
            });
            controlRunner.RunningUpdate(0);
            AssertExecutionOrderByGUIDOnly(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode1,
                guidNode2,
            });
            
            CleanUpTest();
        }
    }
}