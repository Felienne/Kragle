using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Kragle;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            JArray json = JArray.Parse("[[41, \"fd2\"], {}]");
            Script script = new Script(json, ScriptScope.Script, "my_script");

            Assert.AreEqual(ScriptScope.Script, script.Scope);
            Assert.AreEqual("my_script", script.ScopeName);
        }

        [TestMethod]
        public void ParseScriptSimpleCodeTest()
        {
            JArray json = JArray.Parse("[]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(JToken.DeepEquals(new JArray(), script.Blocks));
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void ParseScriptNullTest()
        {
            new Script(null, ScriptScope.Script, "something");
        }


        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleFalseTest()
        {
            JArray json = JArray.Parse("[483, 21]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedFalseTest()
        {
            JArray json = JArray.Parse("[[[[[[38]]], \"no\"]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleEmptyFalseTest()
        {
            JArray json = JArray.Parse("[]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedEmptyFalseTest()
        {
            JArray json = JArray.Parse("[[[[[[[[]]]]]]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsFalse(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldSimpleTrueTest()
        {
            JArray json = JArray.Parse("[917]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptHasExactlyOneFieldNestedTrueTest()
        {
            JArray json = JArray.Parse("[[[[[[[[[[[[[3]]]]]]]]]]]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(script.HasExactlyOneField());
        }

        [TestMethod]
        public void ScriptGetWaitBlocksNoneTest()
        {
            JArray json = JArray.Parse("[85, \"41\", [\"5\"]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.IsTrue(new List<Script>().SequenceEqual(script.GetWaitBlocks()));
        }

        [TestMethod]
        public void ScriptGetWaitBlocksSingleTest()
        {
            JArray json = JArray.Parse("[[\"doWaitUntil\", [true]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            IList<Script> expected = new List<Script>();
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [true]]"), ScriptScope.Stage, "stage"));

            Assert.IsTrue(expected.SequenceEqual(script.GetWaitBlocks()));
        }

        [TestMethod]
        public void ScriptGetWaitBlocksMultipleTest()
        {
            JArray json = JArray.Parse("[[\"doWaitUntil\", [15]], [51, [\"doWaitUntil\", [9, \"51\"]]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            IList<Script> expected = new List<Script>();
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [15]]"), ScriptScope.Stage, "stage"));
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [9, \"51\"]]"), ScriptScope.Stage, "stage"));

            Assert.IsTrue(expected.SequenceEqual(script.GetWaitBlocks()));
        }

        [TestMethod]
        public void ScriptGetWaitBlocksNestedTest()
        {
            JArray json = JArray.Parse("[[\"doWaitUntil\", [\"doWaitUntil\", [\"text\"]]]]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            IList<Script> expected = new List<Script>();
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [\"doWaitUntil\", [\"text\"]]]"), ScriptScope.Stage, "stage"));
            expected.Add(new Script(JArray.Parse("[\"doWaitUntil\", [\"text\"]]"), ScriptScope.Stage, "stage"));

            Assert.IsTrue(expected.SequenceEqual(script.GetWaitBlocks()));
        }

        [TestMethod]
        public void ScriptEqualsNullFalseTest()
        {
            JArray json = JArray.Parse("[613]");
            Script scriptA = new Script(json, ScriptScope.Stage, "stage");

            Assert.AreNotEqual(scriptA, null);
        }

        [TestMethod]
        public void ScriptEqualsJsonFalseTest()
        {
            JArray jsonA = JArray.Parse("[\"fj1\"]");
            JArray jsonB = JArray.Parse("[\"551\"]");
            Script scriptA = new Script(jsonA, ScriptScope.Script, "bla");
            Script scriptB = new Script(jsonB, ScriptScope.Script, "bla");

            Assert.AreNotEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsScopeFalseTest()
        {
            JArray json = JArray.Parse("[\"g01\", [], [\"551\", [51]]]");
            Script scriptA = new Script(json, ScriptScope.Script, "script_name");
            Script scriptB = new Script(json, ScriptScope.Stage, "stage");

            Assert.AreNotEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsSameTrueTest()
        {
            JArray json = JArray.Parse("[166, 593, 225]");
            Script scriptA = new Script(json, ScriptScope.Stage, "stage");
            Script scriptB = new Script(json, ScriptScope.Stage, "stage");

            Assert.AreEqual(scriptA, scriptB);
        }

        [TestMethod]
        public void ScriptEqualsSelfTrueTest()
        {
            JArray json = JArray.Parse("[48, [\"a51\", 518], \"991\"]");
            Script script = new Script(json, ScriptScope.Stage, "stage");

            Assert.AreEqual(script, script);
        }
    }
}
