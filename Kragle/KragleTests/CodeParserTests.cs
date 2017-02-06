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

            Assert.IsTrue(JToken.DeepEquals(new JArray(), script.Blocks));
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
        [ExpectedException(typeof(JsonReaderException))]
        public void ParseScriptCoordinateTypeFailureTest()
        {
            JArray json = JArray.Parse("[[], 661, 431]");
            ParseScript(json, ScriptScope.Stage, "stage");
        }
        

        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleFalseTest()
        {
            JArray json = JArray.Parse("[581, 31, [483, 21]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedFalseTest()
        {
            JArray json = JArray.Parse("[246, 333, [[[[[[38]]], \"no\"]]]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleEmptyFalseTest()
        {
            JArray json = JArray.Parse("[291, 59, []]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedEmptyFalseTest()
        {
            JArray json = JArray.Parse("[809, 606, [[[[[[[]]]]]]]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleTrueTest()
        {
            JArray json = JArray.Parse("[246, 333, [917]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedTrueTest()
        {
            JArray json = JArray.Parse("[246, 333, [[[[[[[[[[[[[3]]]]]]]]]]]]]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(script.HasExactlyOneField());
        }
    }
}
