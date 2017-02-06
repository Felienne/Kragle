using System;
using Kragle;
using Microsoft.CSharp.RuntimeBinder;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace KragleTests
{
    [TestClass]
    public class CodeParserTests : CodeParser
    {
        public CodeParserTests() : base(new FileStore())
        {
        }


        [TestMethod]
        public void ParseScriptSimpleCoordinatesTest()
        {
            JArray json = JArray.Parse("[14, 73, []]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.AreEqual(14, script.X);
            Assert.AreEqual(73, script.Y);
        }

        [TestMethod]
        public void ParseScriptSimpleScopeTest()
        {
            JArray json = JArray.Parse("[58, 21, [[41, \"fd2\"], {}]]");
            Script script = ParseScript(json, ScriptScope.Script, "my_script");

            Assert.AreEqual(ScriptScope.Script, script.Scope);
            Assert.AreEqual("my_script", script.ScopeName);
        }

        [TestMethod]
        public void ParseScriptSimpleCodeTest()
        {
            JArray json = JArray.Parse("[24, 921, []]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(JToken.DeepEquals(new JArray(), script.Code));
        }

        [TestMethod]
        [ExpectedException(typeof(RuntimeBinderException))]
        public void ParseScriptNullTest()
        {
            ParseScript(null, ScriptScope.Script, "something");
        }

        [TestMethod]
        [ExpectedException(typeof(JsonReaderException))]
        public void ParseScriptArrayTypeFailureTest()
        {
            JArray json = JArray.Parse("[68, 31, {}]");
            ParseScript(json, ScriptScope.Script, "start");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseScriptCoordinateTypeFailureTest()
        {
            JArray json = JArray.Parse("[[], 661, 431]");
            ParseScript(json, ScriptScope.Stage, "stage");
        }
    }
}
