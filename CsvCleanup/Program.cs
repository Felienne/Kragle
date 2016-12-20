using System.IO;
using System.Text;


namespace CsvCleanup
{
    internal class Program
    {
        private static void Main()
        {
            using (
                StreamReader input =
                    File.OpenText(@"C:\Users\Efthimia\Documents\GitHub\Kragle\KragleData\analysisutf8.txt"))
            {
                using (
                    StreamWriter output =
                        new StreamWriter(
                            @"C:\Users\Efthimia\Documents\GitHub\Kragle\KragleData\analysisutf8Replaced.txt", true,
                            Encoding.UTF8))
                {
                    string line;
                    while (null != (line = input.ReadLine()))
                    {
                        line = line.Replace(",\"I have no talent.\", said Sprite 44.,",
                            ",\"I have no talent., said Sprite 44.\",");
                        line = line.Replace(",\"I lagged so much, so I decided, \"Whatever\"\",",
                            ",\"I lagged so much, so I decided, Whatever\",");
                        if (line.Contains(",sprite,\",jefsdfaorjsdnf\""))
                        {
                            line = line.Replace(",sprite,\",jefsdfaorjsdnf\",", ",sprite,\"jefsdfaorjsdnf\",");
                        }
                        if (line.Contains(",\" onclick=\\\"alert(\\\\\\\"\""))
                        {
                            line = line.Replace(",\" onclick=\\\"alert(\\\\\\\"\"", ",\" onclick=\\alert(\\\\\\\"");
                        }
                        if (line.Contains(",sprite,\",-0\","))
                        {
                            line = line.Replace(",sprite,\",-0\",", ",sprite,\"-0\",");
                        }
                        if (line.Contains("sprite,\",assssss\","))
                        {
                            line = line.Replace("sprite,\",assssss\",", "sprite,\"assssss\",");
                        }
                        if (line.Contains(",\"Server IP: \\\\\\\"$key\\\\\\\" Port: \\\\\\\"&nbsp;\\\\\\\"..>19132\""))
                        {
                            line =
                                line.Replace(
                                    ",\"Server IP: \\\\\\\"$key\\\\\\\" Port: \\\\\\\"&nbsp;\\\\\\\"..>19132\"",
                                    ",\"Server IP: \\\\\\$key\\\\\\ Port: \\\\\\&nbsp;\\\\\\..>19132\"");
                        }
                        line = line.Replace(",\"\\\\\\\");\\\">\"", ",\"\\\\\\);\\>\"");
                        if (line.Contains("\"*scratch* \"Ow, that hurt...\"\""))
                        {
                            line = line.Replace("\"*scratch* \"Ow, that hurt...\"\"", "\"*scratch* Ow, that hurt...\"");
                        }
                        if (!line.Contains("\"bob is the adrihgl\\") && !line.Contains(",\"agario\\\"") &&
                            !line.StartsWith("88909171") && !line.StartsWith("89272317") &&
                            !line.StartsWith("98879854") &&
                            !line.Contains(@"\\"""))
                        {
                            line = line.Replace(@"\""", "*");
                        }
                        if (line.StartsWith("87560430,10-10,6,sprite,"))
                        {
                            line = line.Replace("\"Draw\" Icon", "Draw Icon");
                        }


                        string[] splits = line.Split(',');
                        if (splits.Length > 4)
                        {
                            string[] parts = splits[4].Split('\"');
                            if (splits[4] == @"""" || parts.Length > 3 ||
                                parts.Length == 3 && (parts[0].Length > 0 || parts[2].Length > 0))
                            {
                                if (!line.Contains(",\",\","))
                                {
                                    line = line.Replace("," + splits[4] + ",",
                                        ",\"" + splits[4].Replace("\"", "") + "\",");
                                }
                            }

                            if (splits.Length > 8 && splits[8].Length > 1900)
                            {
                                line = line.Replace(splits[8], "(shortened)" + splits[8].Substring(0, 1899));
                            }
                        }

                        if (!line.StartsWith("93746209"))
                        {
                            output.WriteLine(line);
                        }
                    }
                }
            }
        }
    }
}
