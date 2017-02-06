using System;
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
        /// <param name="code">the code</param>
        protected void Parse(string code)
        {
            dynamic json;
            try
            {
                json = JObject.Parse(code);
            }
            catch (Exception)
            {
                Console.WriteLine("Failure!");
                return;
            }


            // Iterate over stage scripts
            foreach (dynamic script in json.scripts ?? Enumerable.Empty<dynamic>())
            {
                ParseScript(script, ScriptScope.Stage, "stage");
            }

            // Iterate over scripts in sprites
            foreach (dynamic sprite in json.children ?? Enumerable.Empty<dynamic>())
            {
                string spriteName = sprite.objName;

                foreach (dynamic script in sprite.scripts ?? Enumerable.Empty<dynamic>())
                {
                    ParseScript(script, ScriptScope.Script, spriteName);
                }
            }
        }

        /// <summary>
        ///     Constructs a <code>Script</code>.
        /// </summary>
        /// <param name="script">the contents of the script as a JSON object</param>
        /// <param name="scope">the scope the script is in</param>
        /// <param name="scopeName">the name of the sprite the script is in, or "stage"</param>
        /// <returns>a new <code>Script</code> object</returns>
        protected Script ParseScript(dynamic script, ScriptScope scope, string scopeName)
        {
            float x = float.Parse(script[0].ToString());
            float y = float.Parse(script[1].ToString());
            JArray code = JArray.Parse(script[2].ToString());

            return new Script(x, y, code, scope, scopeName);
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
            /// <param name="x">the x-coordinate</param>
            /// <param name="y">the y-coordinate</param>
            /// <param name="code">the blocks as a JSON array</param>
            /// <param name="scope">the scope the script was found in</param>
            /// <param name="scopeName">the name of the sprite the script is in, or "stage"</param>
            public Script(float x, float y, JArray code, ScriptScope scope, string scopeName)
            {
                X = x;
                Y = y;
                Code = code;
                Scope = scope;
                ScopeName = scopeName;
            }


            public float X { get; set; }
            public float Y { get; set; }
            public JArray Code { get; set; }
            public ScriptScope Scope { get; set; }
            public string ScopeName { get; set; }
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
