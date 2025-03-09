using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public static class SavesCleaner
    {
        [MenuItem("Tools/HeroicEngine/Clear Saves", false, 1)]
        public static void ClearSaves()
        {
            PlayerPrefs.DeleteAll();
            
            DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);

            foreach (var file in dir.GetFiles())
            {
                File.Delete(file.FullName);
            }
        }
    }
}