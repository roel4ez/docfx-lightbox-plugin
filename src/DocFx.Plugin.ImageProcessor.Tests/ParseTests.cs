using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DocFx.Plugin.ImageProcessor.Tests
{
    [TestClass]
    public class ParseTests
    {
        [TestMethod]
        [DeploymentItem("test.html","test")]
        public void TestMethod1()
        {
            var undertest = new ImagePostProcessor();
            undertest.AddLightBoxToImage("test/test.html");
        }
    }
}
