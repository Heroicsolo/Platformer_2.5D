using System.IO;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class SystemsCreator : EditorWindow
    {
        private string className = "";
        private bool createSystemBtnClicked;

        [MenuItem("Tools/HeroicEngine/Create System", false, 22)]
        public static void ShowWindow()
        {
            GetWindow<SystemsCreator>("Systems Creator");
        }

        private void OnGUI()
        {
            GUILayout.Label("Create System class", EditorStyles.boldLabel);

            string prevClassName = className;

            className = EditorGUILayout.TextField("Class Name", className);

            if (className != prevClassName)
            {
                createSystemBtnClicked = false;
            }

            if (GUILayout.Button("Create System"))
            {
                CreateSystem();
                createSystemBtnClicked = true;
            }

            if (createSystemBtnClicked && IsValidClassName(className))
            {
                if (GUILayout.Button("Instantiate System on scene"))
                {
                    InstantiateSystem();
                }
            }
        }

        private void CreateSystem()
        {
            // Ensure the class name is valid
            if (!IsValidClassName(className))
            {
                Debug.LogError("Invalid class name. Please provide a valid class name.");
                return;
            }

            // Define the script content
            string scriptContent = $@"using HeroicEngine.Systems.DI;
using UnityEngine;

public class {className} : SystemBase
{{
}}
";

            // Define the path where the script will be saved
            string path = "Assets/Scripts/Systems/" + className + ".cs";

            // Check if the file already exists
            if (File.Exists(path))
            {
                Debug.LogError($"A script with the name '{className}' already exists.");
                return;
            }

            // Create the script file and write the content
            File.WriteAllText(path, scriptContent);
            AssetDatabase.Refresh(); // Refresh the AssetDatabase so Unity recognizes the new file

            Debug.Log($"Script '{className}.cs' has been created at {path}");

            Repaint();
        }

        private void InstantiateSystem()
        {
            var scriptType = TypeUtility.GetTypeByName(className);

            if (scriptType != null)
            {
                // Create a new GameObject and add the script as a component
                GameObject newObject = new GameObject(className);
                newObject.AddComponent(scriptType);
                AssetDatabase.Refresh();
            }
        }

        // Simple check for valid class name (can be expanded if needed)
        private bool IsValidClassName(string className)
        {
            return !string.IsNullOrEmpty(className) && char.IsLetter(className[0]) && !className.Contains(" ");
        }
    }
}