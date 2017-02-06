using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    /// <summary>
    ///     A <code>CodeParser</code> parses all available project code. It generates several CSV files which can be
    ///     imported into a relational database for further analysis.
    /// </summary>
    public class CodeParser
    {
        private readonly FileStore _fs;


        /// <summary>
        ///     Constructs a new <code>CodeParser</code>.
        /// </summary>
        /// <param name="fs">the <code>FileStore</code> to interact with</param>
        public CodeParser(FileStore fs)
        {
            _fs = fs;
        }


        /// <summary>
        ///     Parses all downloaded code files.
        /// </summary>
        public void ParseProjects()
        {
            FileInfo[] codeFiles = _fs.GetFiles("code");

            Console.WriteLine("Parsing " + codeFiles.Length + " code files.");

            foreach (FileInfo codeFile in codeFiles)
            {
                string code = File.ReadAllText(codeFile.FullName);
                Parse(code);
            }
        }


        /// <summary>
        ///     Parses the given code.
        /// </summary>
        /// <param name="rawCode">the raw code</param>
        /// <returns>the <code>Code</code> object corresponding to the raw code</returns>
        protected Code Parse(string rawCode)
        {
            // Parse raw code
            JObject json;
            try
            {
                json = JObject.Parse(rawCode);
            }
            catch (Exception)
            {
                //Console.WriteLine("Failure!");
                return null;
            }


            // Parse code object
            Code code = new Code();

            // Iterate over stage scripts
            foreach (JToken script in json.GetValue("scripts") ?? Enumerable.Empty<dynamic>())
            {
                if (!(script is JArray) || script.Count() != 3 || !(script[2] is JArray))
                {
                    continue;
                }

                Script s = new Script((JArray) script[2], ScriptScope.Stage, "stage");
                code.AddScript(s);
            }

            // Iterate over scripts in sprites
            foreach (JToken spriteToken in json.GetValue("children") ?? Enumerable.Empty<dynamic>())
            {
                if (!(spriteToken is JObject))
                {
                    continue;
                }
                JObject sprite = (JObject) spriteToken;
                string spriteName = sprite.GetValue("objName").ToString();

                foreach (JToken script in sprite.GetValue("scripts") ?? Enumerable.Empty<dynamic>())
                {
                    if (!(script is JArray) || script.Count() != 3 || !(script[2] is JArray))
                    {
                        continue;
                    }

                    Script s = new Script((JArray) script[2], ScriptScope.Script, spriteName);
                    code.AddScript(s);

                    Console.WriteLine(s.HasExactlyOneField());
                }
            }

            return code;
        }


        //
        protected class Code
        {
            private readonly IList<Script> scripts;
            private readonly IList<Script> waitScripts;


            public Code()
            {
                scripts = new List<Script>();
                waitScripts = new List<Script>();
            }


            public void AddScript(Script script)
            {
                scripts.Add(script);
            }

            public void AddWaitScript(Script waitScript)
            {
                waitScripts.Add(waitScript);
            }

            public void DetectClones()
            {
                //
            }
        }

        /// <summary>
        ///     This class represents a script in a Scratch environment.
        ///     A script consists of a sequence of (nested) blocks, and has a physical position in a 2D environment.
        /// </summary>
        protected class Script
        {
            /// <summary>
            ///     Constructs a new <code>Script</code> object.
            /// </summary>
            /// <param name="blocks">the blocks as a JSON array</param>
            /// <param name="scope">the scope the script was found in</param>
            /// <param name="scopeName">the name of the sprite the script is in, or "stage"</param>
            public Script(JArray blocks, ScriptScope scope, string scopeName)
            {
                if (blocks == null)
                {
                    throw new ArgumentNullException(nameof(blocks));
                }

                Blocks = blocks;
                Scope = scope;
                ScopeName = scopeName;
            }

            public JArray Blocks { get; }
            public ScriptScope Scope { get; }
            public string ScopeName { get; }


            /// <summary>
            ///     Recursively checks whether this <code>Script</code> contains exactly one block. If a block contains
            ///     other blocks, it is checked whether this block contains exactly one block, et cetera.
            /// </summary>
            /// <returns>true if this <code>Script</code> recursively contains exactly one block</returns>
            public bool HasExactlyOneField()
            {
                return HasExactlyOneField(Blocks);
            }

            /// <summary>
            ///     Recursively detects wait blocks.
            /// </summary>
            /// <returns>an <code>IList</code> of all wait blocks</returns>
            public IList<Script> GetWaitBlocks()
            {
                return GetWaitBlocks(Blocks, Scope, ScopeName);
            }

            /// <summary>
            ///     Compares this <code>Script</code> against another object.
            /// </summary>
            /// <param name="obj">an object</param>
            /// <returns>true if this <code>Script</code> equals the given object</returns>
            public override bool Equals(object obj)
            {
                Script that = obj as Script;
                return that != null && Equals(that);
            }

            /// <summary>
            ///     Returns the hash code for this <code>Script</code>.
            /// </summary>
            /// <returns>the hash code for this <code>Script</code></returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = Blocks?.GetHashCode() ?? 0;
                    hashCode = (hashCode * 397) ^ (int) Scope;
                    hashCode = (hashCode * 397) ^ (ScopeName?.GetHashCode() ?? 0);
                    return hashCode;
                }
            }


            private static bool HasExactlyOneField(JArray blocks)
            {
                if (blocks.Count != 1)
                {
                    return false;
                }

                JArray block = blocks[0] as JArray;
                return block == null || HasExactlyOneField(block);
            }

            private static IList<Script> GetWaitBlocks(JArray blocks, ScriptScope scope, string scopeName)
            {
                IList<Script> waitScripts = new List<Script>();

                // Iterate over blocks
                foreach (JToken block in blocks)
                {
                    if (!(block is JArray))
                    {
                        continue;
                    }

                    JArray arrayBlock = block as JArray;

                    // Wait block requires multiple fields
                    if (HasExactlyOneField(arrayBlock))
                    {
                        continue;
                    }

                    // Check if this block is a wait block
                    if (arrayBlock[0].ToString() == "doWaitUntil")
                    {
                        Script waitScript = new Script(arrayBlock, scope, scopeName);
                        waitScripts.Add(waitScript);
                    }

                    // Recur over inner blocks
                    foreach (Script waitScript in GetWaitBlocks(arrayBlock, scope, scopeName))
                    {
                        waitScripts.Add(waitScript);
                    }
                }

                return waitScripts;
            }

            private bool Equals(Script that)
            {
                return JToken.DeepEquals(Blocks, that.Blocks)
                       && Scope == that.Scope
                       && string.Equals(ScopeName, that.ScopeName);
            }
        }

        /// <summary>
        ///     The possible scopes for a script to be in.
        /// </summary>
        protected enum ScriptScope
        {
            Stage,
            Script
        }
    }
}
