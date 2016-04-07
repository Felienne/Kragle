using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Json;
using System.Linq;
using System.Net;
using Scraper;


namespace Kragle
{
    public class JSONReader
    {
       public class Script
       {
          public JsonArray Code;
          public string Scope;
          public string ScopeName;
          public string Location;

          public Script(JsonArray code, string scope, string scopeName, string location)
          {
             Code = code;
             Scope = scope;
             ScopeName = scopeName;
             Location = location;
          }
       }

       public static void ProcessJSON(string path)
       {
          DirectoryInfo d = new DirectoryInfo(path);

          FileInfo[] Files = d.GetFiles();
          int i = 0;

          foreach (FileInfo file in Files)
          {
             int dot = file.Name.IndexOf(".");
             string id = file.Name.Substring(0, dot);

             string filename = file.FullName;

             System.IO.StreamReader fileRead = new System.IO.StreamReader(filename);
             string JSON = fileRead.ReadToEnd();

             var allScripts = new List<Script>();
             var allWaitScripts = new List<Script>();

             try
             {
                dynamic result = JsonValue.Parse(JSON);

                //in scripts we get all scripts at the level of the stage (the background)
                foreach (var script in result.scripts)
                {
                   string location = script[0] + "-" + script[1]; 

                   Script s = new Script(script[2], "stage", "stage", location); //we only take the third element, the first two are the location.
                   allScripts.Add(s);

                   string scopeType = "stage";
                   string scopeName = "stage";
                   foreach (var w in allWaits(script[2], ref scopeType, ref scopeName))
                   {
                      allWaitScripts.Add(w);
                   }

                   WriteToFile(id, script, "stage", path);
                }

                //the children are all the sprites, each one has its ows name and code
                foreach (var sprite in result.children)
                {
                   string spriteName = sprite.objName; //oh,oh sprite names can have commas, so we need quotes
                   foreach (var script in sprite.scripts)
                   {
                      string location = script[0] + "-" + script[1]; 

                      Script s = new Script(script[2], "sprite", spriteName, location); //we only take the third element, the first two are the location.
                      allScripts.Add(s);

                      string scopeType = "sprite";
                      string scopeName = spriteName;
                      foreach (var w in allWaits(script[2], ref scopeType, ref scopeName))
                      {
                         allWaitScripts.Add(w);
                      }


                      WriteToFile(id, script, spriteName, path);
                   }
                }

                cloneDetect(path, "clones", allScripts, id, true);
                cloneDetect(path, "waitClones", allWaitScripts, id,false);
             }
             catch (Exception E)
             {
                using (System.IO.StreamWriter failFile =
                new System.IO.StreamWriter(path + "output\\fail.csv", true))
                {
                   failFile.WriteLine(id);
                }
             }

             fileRead.Close();
             try
             {
                File.Move(filename, path+ "done\\" + id + ".sb");
             }
             catch (Exception)
             {
                
             }


             Console.WriteLine(i.ToString() + "-" + ((i * 100) / Files.Length).ToString() + "%");
             i++;

          }
          Console.ReadLine();
       }

       private static void cloneDetect(string path, string file, List<Script> allScripts, string id, bool WriteLoc)
       {

         var scriptsbyCode = allScripts.GroupBy(x => x.Code.ToString());

         // Loop over groups.
         foreach (var code in scriptsbyCode)
         {
            //if there is a group bigger than 1 (aka A CLONE) 
            //print that thang!
            int numOccurrences = code.Count();

            if (numOccurrences > 1)
            {
               //now group the clones by their location:
               var clonesbyScope = code.GroupBy(x => x.ScopeName);

               string scopesAndOccurrences = "";
               foreach (var scope in clonesbyScope)
               {
                  scopesAndOccurrences += "," + scope.Key + ",";
                  scopesAndOccurrences += scope.Count();
               }

               if (numOccurrences != clonesbyScope.Count())
               {
                  var b = true;
               }

               string toWrite;
               if (WriteLoc)
               {
                  var y = code.First();
                  toWrite = "\""+y.ScopeName +"" +"\"," + y.Location; //we write the location of the first clone
               }
               else
               {
                  toWrite = code.Key;                      
               }

               using (System.IO.StreamWriter analysisFile =
                  new System.IO.StreamWriter(path + "output\\" + file + ".csv", true))
               {
                  analysisFile.WriteLine(id + "," + numOccurrences + "," + clonesbyScope.Count() + "," + toWrite +
                                       scopesAndOccurrences );
               }
            }
         }
          
       }


       private static bool AllOneField(JsonArray script)
        {
            //determines whether all subfields of this have only one field

            //if this has more, no deal
            if (script.Count >1)
            {
                return false;
            }


            foreach (var innerScript in script)
            {
                if (innerScript is JsonArray)
                {
                    if (!AllOneField((JsonArray)innerScript))
                    {
                        return false;
                    }
                }

            }

            return true;
        }

       private static ArrayList allWaits(JsonArray scripts, ref string scopeType, ref string scopeName)
       {
          var result = new ArrayList();

          if (scopeName[0] != '"')
          {
             //not in quotes? add them
             scopeName = "\"" + scopeName + "\"";
          }


          foreach (var innerScript in scripts)
          {
             if (innerScript is JsonArray)
             {
                if (AllOneField((JsonArray) innerScript))
                {

                }
                else
                {
                   //we save all conditions for clone detection:
                   if (innerScript.Count > 0 && innerScript[0].ToString() == "\"doWaitUntil\"")
                   {
                      Script S = new Script((JsonArray) innerScript, scopeType, scopeName, "00");
                      result.Add(S);
                   }

                   foreach (var item in allWaits((JsonArray) (innerScript), ref scopeType, ref scopeName))
                   {
                      result.Add(item);
                   }

                }
             }

          }
          return result;
       }


       static ArrayList flatten(JsonArray scripts, ref string scopeType, ref string scopeName, ref int indent, string path, string id, ref int maxIndent)
        {
            var result = new ArrayList();

            if (scopeName[0] != '"')
            {
               //not in quotes? add them
               scopeName = "\"" + scopeName + "\"";
            }


            //by default we add the type of the scope (scene, sprite, or proc) the name of the scope and the indent
            string toPrint = scopeType + "," + scopeName + "," + indent.ToString();
            bool added = false;

            foreach (var innerScript in scripts)
            {
               //if the script is primitive, we just print it.
                if (innerScript is JsonPrimitive)
                {
                    toPrint += "," + innerScript;
                    added = true; //it could be that there will be more primitives (arguments) so we only print at the end
                }
                if (innerScript is JsonArray)
                {
                    if (AllOneField((JsonArray)innerScript))
                    {
                        if (innerScript.Count == 0)
                        {
                            //this is an empy array
                            toPrint += ",[]";
                        }
                        else
                        {
                           int j = indent + 1;
                           if (j>maxIndent)
	                        {
                              maxIndent = j;
	                        }
                           foreach (var item in flatten((JsonArray)(innerScript), ref scopeType, ref scopeName, ref j, id, path, ref maxIndent))
                           {
                              result.Add(item);
                           }
                        }

                    }
                    else
                    {

                        if (innerScript.Count > 0 && innerScript[0].ToString() == "\"procDef\"")
                        {
                            //first save this definition to a separate file
                            string procdef = id + ","+ scopeName+",procDef," + innerScript[1].ToString()+"," + innerScript[2].Count.ToString(); //procdef plus name of the proc plus number of arguments
                            JSONGetter.writeStringToFile(procdef, path+"output\\procedures.csv", true);

                            toPrint += ",procdef";
                            //now set the other blocks to the scope of this proc
                            scopeType = "procDef";
                            scopeName = innerScript[1].ToString();

                            added = true;
                        }
                        else
                        {
                           int j = indent + 1;
                           if (j > maxIndent)
                           {
                              maxIndent = j;
                           }
                            foreach (var item in flatten((JsonArray)(innerScript), ref scopeType, ref scopeName, ref j, id, path, ref maxIndent))
                            {
                                result.Add(item);
                            }
                        }
                    }


                }

            }

            if (added)
            {
                result.Add(toPrint);
            }


            return result;

        }

   public static void WriteToFile(string fileID, dynamic script, string SpriteName, string path)
   {
      string saveLocation = path;

      string location = script[0] +"-" + script[1]; 
      // scripts have a location that is unique so we use that as an ID.

      JsonArray innerCode = (script[2]);

      int indent = 0;
      string scopeType = "sprite";
      string scopeName = SpriteName;

      var maxIndent = 0;

      var allStatements = flatten(innerCode,ref scopeType, ref scopeName, ref indent, path,fileID, ref maxIndent);
      foreach (var statement in allStatements)
      {
         using (System.IO.StreamWriter analysisFile =
         new System.IO.StreamWriter(saveLocation+"output\\analysis.csv", true))
         {
            analysisFile.WriteLine(fileID + "," + location + "," + statement);
         }
      };

      var allWaitStatements = allWaits(innerCode, ref scopeType, ref scopeName);



      //to calculate Long Method smell we need the length of all scripts, which is the number of items in flattenSimple
      //we also save the number of statements at the top level
      //and the maximum depth.

      using (System.IO.StreamWriter scriptsFile = new System.IO.StreamWriter(saveLocation + "output\\scripts.csv", true))
      {
         scriptsFile.WriteLine(fileID + "," + location + "," + scopeType + "," + scopeName + "," + innerCode.Count + "," + allStatements.Count + "," + maxIndent);
      }

   }



            


            


     }


    
}
