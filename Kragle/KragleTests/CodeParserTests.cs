using System.Collections.Generic;
using System.Linq;
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

        [TestMethod]
        public void ScriptGetWaitBlocksTest()
        {
            JArray json = JArray.Parse("[26, 841, [[\"doWaitUntil\", [true]]]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            IList<Script> expected = new List<Script>();
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [true]]"), ScriptScope.Stage, "stage"));

            Assert.IsTrue(expected.SequenceEqual(script.GetWaitBlocks()));
        }

        [TestMethod]
        public void ScriptEqualsNullFalseTest()
        {
            JArray json = JArray.Parse("[539, 668, []]");
            Script scriptA = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.AreNotEqual(scriptA, null);
        }

        [TestMethod]
        public void ScriptEqualsJsonFalseTest()
        {
            JArray jsonA = JArray.Parse("[950, 754, [\"fj1\"]]");
            JArray jsonB = JArray.Parse("[229, 159, [\"551\"]]");
            Script scriptA = ParseScript(jsonA, ScriptScope.Script, "bla");
            Script scriptB = ParseScript(jsonB, ScriptScope.Script, "bla");

            Assert.AreNotEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsScopeFalseTest()
        {
            JArray json = JArray.Parse("[259, 956, [\"g01\", [], [\"551\", [51]]]]");
            Script scriptA = ParseScript(json, ScriptScope.Script, "script_name");
            Script scriptB = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.AreNotEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsSameTrueTest()
        {
            JArray json = JArray.Parse("[476, 247, [166, 593, 225]]");
            Script scriptA = ParseScript(json, ScriptScope.Stage, "stage");
            Script scriptB = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.AreEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsSelfTrueTest()
        {
            JArray json = JArray.Parse("[111, 619, [48, [\"a51\", 518], \"991\"]]");
            Script script = ParseScript(json, ScriptScope.Stage, "stage");

            Assert.AreEqual(script, script);
        }
    }
}
