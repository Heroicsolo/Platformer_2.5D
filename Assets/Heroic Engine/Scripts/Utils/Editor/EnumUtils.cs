using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace HeroicEngine.Utils.Editor
{
    public class EnumUtils
    {
        public static void WriteToEnum<T>(string path, string name, ICollection<T> data)
        {
            string fullPath = path + name + ".cs";

            using (StreamWriter file = File.CreateText(fullPath))
            {
                file.WriteLine("namespace HeroicEngine.Enums {");
                file.WriteLine("public enum " + name + " {");

                int i = 0;
                foreach (var line in data)
                {
                    string lineRep = line.ToString().Replace(" ", string.Empty);
                    if (!string.IsNullOrEmpty(lineRep))
                    {
                        file.WriteLine(string.Format("	{0} = {1},",
                            lineRep, i));
                        i++;
                    }
                }

                file.WriteLine("}");
                file.WriteLine("}");

                AssetDatabase.ImportAsset(fullPath);
            }
        }
    }
}