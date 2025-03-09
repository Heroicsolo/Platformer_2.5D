using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEngine.Rendering;

namespace HeroicEngine.Utils.Editor
{
    [InitializeOnLoad]
    public class RainShaderHandler
    {
        static string shaderFilePath = "Assets/Heroic Engine/Shaders/URP_RainShader.shader";
        static string shaderBackupPath = "Assets/Heroic Engine/Shaders/URP_RainShader.shader.txt";

        // Static constructor to handle when Unity starts
        static RainShaderHandler()
        {
            // Check if the pipeline is URP
            if (GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("Universal"))
            {
                // When URP is active, restore the shader file extension
                RestoreShaderFile();
                SwitchMaterialToURP();
            }
            else
            {
                // When not using URP, move the shader file to a hidden state
                HideShaderFile();
                SwitchMaterialToStandard();
            }

            // Register callback to handle pipeline changes
            EditorApplication.update += HandleRenderPipelineChange;
        }

        static void HandleRenderPipelineChange()
        {
            // Check if URP is activated
            if (GraphicsSettings.renderPipelineAsset != null && GraphicsSettings.renderPipelineAsset.GetType().Name.Contains("Universal"))
            {
                RestoreShaderFile();
            }
            else
            {
                HideShaderFile();
            }
        }

        static void SwitchMaterialToStandard()
        {
            // Find all materials and switch from URP Unlit to Standard
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (string materialGuid in materialGuids)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (material != null && material.shader != null && material.shader.name == "Custom/URP_RainShader")
                {
                    Color col = material.color;
                    Shader standardRainShader = Shader.Find("Custom/RainShader");
                    if (standardRainShader != null)
                    {
                        material.shader = standardRainShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            // Refresh the asset database to apply the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void SwitchMaterialToURP()
        {
            // Find all materials and switch from Standard to URP Lit
            string[] materialGuids = AssetDatabase.FindAssets("t:Material");

            foreach (string materialGuid in materialGuids)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(materialGuid);
                Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);

                if (material != null && material.shader != null && material.shader.name == "Custom/RainShader")
                {
                    Color col = material.color;
                    Shader urpRainShader = Shader.Find("Custom/URP_RainShader");
                    if (urpRainShader != null)
                    {
                        material.shader = urpRainShader;
                        material.color = col;
                        EditorUtility.SetDirty(material);
                    }
                }
            }

            // Refresh the asset database to apply the changes
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void RestoreShaderFile()
        {
            if (File.Exists(shaderBackupPath))
            {
                // Restore the original file (change extension back to .shader)
                File.Move(shaderBackupPath, shaderFilePath);
                AssetDatabase.Refresh();  // Refresh Asset Database to re-import the shader
            }
        }

        static void HideShaderFile()
        {
            if (File.Exists(shaderFilePath))
            {
                // Move the shader file to a hidden extension
                File.Move(shaderFilePath, shaderBackupPath);
                AssetDatabase.Refresh();  // Refresh Asset Database to remove it from the project
            }
        }
    }
}