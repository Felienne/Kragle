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
            DirectoryInfo[] days = _fs.GetDirectories("code");
            int dayCount = 0;

            Console.WriteLine("Accessing " + days.Length + " days of code.");

            foreach (DirectoryInfo day in days)
            {
                FileInfo[] files = day.GetFiles();
                dayCount++;

                Console.WriteLine("Parsing " + files.Length + " files for day " + dayCount);

                foreach (FileInfo file in files)
                {
                    string rawCode = File.ReadAllText(file.FullName);

                    JObject json;
                    try
                    {
                        json = JObject.Parse(rawCode);
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Failure!");
                        continue;
                    }

                    Code code = new Code(json);
                }
            }
        }


        /// <summary>
        ///     This class represents an instance of a project's code at a single moment in time.
        /// </summary>
        protected class Code
        {
            private readonly List<Script> _scripts;
            private readonly List<Script> _waitScripts;


            /// <summary>
            ///     Constructs a new <code>Code</code> object.
            /// </summary>
            /// <param name="json">the parsed JSON of the entire code</param>
            public Code(JObject json)
            {
                _scripts = new List<Script>();
                _waitScripts = new List<Script>();


                // Add scripts from root level
                AddScripts(json, ScriptScope.Stage, "stage");

                // Add scripts from each sprite
                foreach (JToken spriteToken in json.GetValue("children") ?? Enumerable.Empty<dynamic>())
                {
                    if (!(spriteToken is JObject))
                    {
                        continue;
                    }

                    JObject sprite = (JObject) spriteToken;
                    string spriteName = sprite.GetValue("objName")?.ToString();
                    if (spriteName == null)
                    {
                        continue;
                    }

                    AddScripts(sprite, ScriptScope.Script, spriteName);
                }
            }


            /// <summary>
            ///     Returns all duplicates present in this piece of code.
            /// </summary>
            /// <returns>all duplicates present in this piece of code</returns>
            public IEnumerable<Duplicate> DetectDuplicates()
            {
                // Sort all scripts by their code; an IGrouping now contains all Scripts with the same code
                IEnumerable<IGrouping<string, Script>> duplicates = _scripts.GroupBy(x => x.Blocks.ToString());

                return
                (
                    from script in duplicates
                    // Select if duplicate:
                    where script.Count() > 1
                    // Sort by scope in code:
                    let groupBy = script.GroupBy(x => x.ScopeName)
                    select new Duplicate(script.Key, groupBy)
                ).ToList();
            }


            private void AddScripts(JObject json, ScriptScope scope, string scopeName)
            {
                // Iterate over scripts
                foreach (JToken scriptToken in json.GetValue("scripts") ?? Enumerable.Empty<dynamic>())
                {
                    // Validate script block
                    if (!(scriptToken is JArray)
                        || scriptToken.Count() != 3 // Must be of form [x, y, code]
                        || !(scriptToken[2] is JArray)) // Code must be a JArray
                    {
                        continue;
                    }

                    // Add scripts to Code object
                    Script script = new Script((JArray) scriptToken[2], scope, scopeName);
                    _scripts.Add(script);
                    _waitScripts.AddRange(script.GetWaitBlocks());
                }
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

                    JArray arrayBlock = (JArray) block;

                    // Wait block requires multiple fields
                    if (HasExactlyOneField(arrayBlock) || arrayBlock.Count == 0)
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
        ///     This class represents a single piece of a script that is also present in other scripts.
        /// </summary>
        protected class Duplicate
        {
            public readonly string Blocks;
            public readonly IEnumerable<Script> Occurrences;


            /// <summary>
            ///     Constructs a new <code>Duplicate</code>.
            /// </summary>
            /// <param name="blocks">the code that was duplicated</param>
            /// <param name="scripts">the <code>Script</code>s containing the code</param>
            public Duplicate(string blocks, IEnumerable<Script> scripts)
            {
                Blocks = blocks;
                Occurrences = scripts;
            }

            /// <summary>
            ///     Constructs a new <code>Duplicate</code>.
            /// </summary>
            /// <param name="blocks">the code that was duplicated</param>
            /// <param name="duplicates">the <code>Script</code>s containing the code</param>
            public Duplicate(string blocks, IEnumerable<IGrouping<string, Script>> duplicates)
            {
                Blocks = blocks;
                Occurrences = duplicates.SelectMany(duplicate => duplicate).ToList();
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
