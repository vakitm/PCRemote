using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace PCRemote
{
    [TestClass]
    public class ClientTest
    {
        [TestMethod]
        public void VolumeUpTest()
        {
            MainForm mainForm = new MainForm();
            var data = new { 
                a = "vc", 
                o = "u"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "VolumeUp");
        }
        [TestMethod]
        public void SetCordsTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "d",
                x = "0",
                y = "0"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "SetCords");
        }
        [TestMethod]
        public void CursorMoveTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "m",
                x = "0",
                y = "0"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "CursorMove");
        }
        [TestMethod]
        public void LeftClickTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "mc",
                o = "l"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "LeftClick");
        }
        [TestMethod]
        public void RightClickTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "mc",
                o = "r"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "RightClick");
        }
        [TestMethod]
        public void KeyPressTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "k",
                k = "11",
                cpt="0"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "KeyPress");
        }
        [TestMethod]
        public void BadJsonFormatTest()
        {
            MainForm mainForm = new MainForm();
            string result = mainForm.processJson("{\"a\":\"vc\",\"o\":}");
            Assert.AreEqual(result, "BadJsonFormat");
        }
        [TestMethod]
        public void CommandNotFoundTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "asd"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "CommandNotFound");
        }
        [TestMethod]
        public void SubCommandNotFoundTest()
        {
            MainForm mainForm = new MainForm();
            var data = new
            {
                a = "vc",
                o = "p"
            };
            var jsonData = JsonConvert.SerializeObject(data);
            string result = mainForm.processJson(jsonData);
            Assert.AreEqual(result, "SubCommandNotFound");
        }
    }

}
