using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace Kragle
{
    internal class CodeParser
    {
        private readonly FileStore _fs;


        //
        public CodeParser(FileStore fs)
        {
            _fs = fs;
        }


        //
        public void ParseProjects()
        {
            FileInfo[] codes = _fs.GetFiles("code");

            Console.WriteLine("Parsing " + codes.Length + " codes.");

            foreach (FileInfo rawCode in codes)
            {
                try
                {
                    JObject.Parse(File.ReadAllText(rawCode.FullName));
                }
                catch (JsonException e)
                {
                    Console.WriteLine(rawCode.Name);
                }
            }
        }


        //
        protected void Parse(string code)
        {
            //
        }
    }
}
