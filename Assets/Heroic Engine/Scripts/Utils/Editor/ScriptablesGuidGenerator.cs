using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    public class ScriptablesGuidGenerator : AssetPostprocessor
    {
        // This method is called when an asset is imported or created in the project
        static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            foreach (var assetPath in importedAssets)
            {
                // Check if the asset is a ScriptableObject
                if (assetPath.EndsWith(".asset"))
                {
                    // Load the ScriptableObject
                    var obj = AssetDatabase.LoadAssetAtPath<GuidScriptable>(assetPath);

                    // Only assign GUID if it is not already set (i.e., new object)
                    if (obj != null && string.IsNullOrEmpty(obj.Guid))
                    {
                        // Generate a new GUID and assign it
                        obj.Guid = System.Guid.NewGuid().ToString();

                        // Save the changes to the asset
                        EditorUtility.SetDirty(obj);

                        // Optionally, you can log the GUID for debugging purposes
                        Debug.Log($"Generated GUID for {assetPath}: {obj.Guid}");

                        // Save the changes to the asset
                        AssetDatabase.SaveAssets();
                    }
                }
            }
        }
    }
}