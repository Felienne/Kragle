using System;
using System.IO;


namespace Scraper
{
    internal class JsonPropertiesReader
    {
        public static bool IsShared(string html)
        {
            return !html.Contains("Sorry this project is not shared");
        }


        public static void WriteProperties(string path)
        {
            //this method assumes that in you have scraped a number of Scratch files
            //it will then put all the corresponding properties in /properties
            DirectoryInfo d = new DirectoryInfo(path);

            FileInfo[] files = d.GetFiles(); //Getting files
            int i = 0;

            foreach (FileInfo file in files)
            {
                //get the id:
                string id = Path.GetFileNameWithoutExtension(file.Name);

                string projectUrl = @"https://scratch.mit.edu/projects/" + id + "/?x=" + DateTime.Now;
                //we are adding a fake quety string to prevent the browser form loading from the cache and getting old data

                string html = JsonGetter.GetJson(projectUrl);

                if (html != null)
                {
                    if (IsShared(html))
                    {
                        string pathForProperties = path + "properties\\properties.sb";

                        JsonGetter.WriteStringToFile(id + ",", pathForProperties, true, false);

                        FindCountandWritetoFile(html, "fav-count", pathForProperties);
                        FindCountandWritetoFile(html, "love-count", pathForProperties);

                        FindCountandWritetoFile(html, "icon views", pathForProperties);
                        FindCountandWritetoFile(html, "icon remix-tree", pathForProperties);

                        FindCountandWritetoFile(html, "Shared:", pathForProperties);
                        FindCountandWritetoFile(html, "Modified:", pathForProperties);

                        FindUserWritetoFile(html, pathForProperties);
                    }
                    else
                    {
                        string pathForProperties = path + "properties\\notShared.sb";
                        JsonGetter.WriteStringToFile(id, pathForProperties, true);
                    }
                }

                Console.WriteLine(i.ToString());
                i++;
            }
        }

        private static void FindCountandWritetoFile(string html, string toFind, string pathForProperties)
        {
            int found = html.IndexOf(toFind, StringComparison.Ordinal);

            if (found != -1)
            {
                int endofSpan = html.IndexOf("</span>", found, StringComparison.Ordinal);
                string item = html.Substring(found + toFind.Length + 2, endofSpan - found - toFind.Length - 2);

                string itemNoSpacesandComma = item.Replace(" ", "").Replace("&nbsp;", "").Replace("\n", "") + ",";
                if (itemNoSpacesandComma == ",")
                {
                    itemNoSpacesandComma = "0,";
                }

                JsonGetter.WriteStringToFile(itemNoSpacesandComma, pathForProperties, true, false);
            }
        }

        private static void FindUserWritetoFile(string html, string pathForProperties)
        {
            const string toFind = "id=\"owner";
            int found = html.IndexOf(toFind, StringComparison.Ordinal);

            if (found == -1)
            {
                return;
            }

            int endOfSpan = html.IndexOf("</span>", found, StringComparison.Ordinal);
            string item = html.Substring(found + toFind.Length + 2, endOfSpan - found - toFind.Length - 2);

            string itemNoSpaces = item.Replace(" ", "").Replace("&nbsp;", "").Replace("\n", "");
            JsonGetter.WriteStringToFile(itemNoSpaces, pathForProperties, true);
        }
    }
}
