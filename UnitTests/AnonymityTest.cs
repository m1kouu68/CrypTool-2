using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTests
{
    [TestClass]
    public class AnonymityTest
    {
        public AnonymityTest()
        {
        }

        [TestMethod]
        public void AnonymityTestMethod()
        {
            CrypTool.PluginBase.ICrypComponent pluginInstance = TestHelpers.GetPluginInstance("Anonymity");
            PluginTestScenario scenario = new PluginTestScenario(pluginInstance, new[] { "InputCSV" }, new[] { "OutputData" });
            object[] output;

            foreach (TestVector vector in testvectors)
            {
                output = scenario.GetOutputs(new object[] { vector.Input});
                Assert.AreEqual(vector.Output, (string)output[0], "Unexpected value in test #" + vector.n + ".");
            }

        }

        private struct TestVector
        {
            public string Input, Output;
            public int n;

        }

        //
        //
        //  CrypTool1-Testvectors
        //
        private readonly TestVector[] testvectors = new TestVector[] {
            new TestVector () { n=0, Input="ABCDEFGHIJKLMNOPQRSTUVWXYZ", Output="ABCDEFGHIJKLMNOPQRSTUVWXYZ"},
            new TestVector () { n=1, Input="123456789", Output="123456789"},
            new TestVector () { n=2, Input="Name,Gender,Postal Code,Diagnosis\nJohn,male,68169,Flu\nMax,male,62317,Cancer", Output="Name,Gender,Postal Code,Diagnosis\nJohn,male,68169,Flu\nMax,male,62317,Cancer" },

        };

    }
}

