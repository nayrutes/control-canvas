using System.Collections.Generic;
using NUnit.Framework;

namespace ControlCanvas.Tests.EditorTests
{
    public class DecisionFlowTest : EditorTestBase
    {
        protected override string GetTestMessage()
        {
            return "Decision of ";
        }
        
        [Test]
        public void TestDecisionSingle()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionTests/SingleDecision.xml");
            string guidNode1 = "069c76ce-f878-4155-a234-a26600242e31";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestDecisionTrue()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionTests/DecisionTrue.xml");
            string guidNode1 = "069c76ce-f878-4155-a234-a26600242e31";
            string guidNode2 = "b8047ef3-3dec-4e18-b07f-208076fc233e";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode2,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestDecisionFalse()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionTests/DecisionFalse.xml");
            string guidNode1 = "069c76ce-f878-4155-a234-a26600242e31";
            string guidNode2 = "c7e7fda8-4b4c-49e4-9d60-7c8c5b8cc04a";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode2,
            });
            
            CleanUpTest();
        }
        
        [Test]
        public void TestDecisionComplex()
        {
            SetUpTest("Assets/ControlFlows/Tests/DecisionTests/DecisionComplex.xml");
            string guidNode1 = "069c76ce-f878-4155-a234-a26600242e31";
            string guidNode2 = "b8047ef3-3dec-4e18-b07f-208076fc233e";
            string guidNode3 = "b7f5589b-8196-49fd-b03f-64bd1a4618ba";
            string guidNode4 = "f2c21b64-1a89-48d0-a508-d47cf0763ac4";
            string guidNode5 = "646ae68d-790a-4bb3-ab7e-676541b10535";

            controlRunner.RunningUpdate(0);
            
            AssertExecutionOrderAndType(new List<string>()
            {
                guidNode1,
                guidNode2,
                guidNode3,
                guidNode4,
                guidNode5,
            });
            
            CleanUpTest();
        }
    }
}