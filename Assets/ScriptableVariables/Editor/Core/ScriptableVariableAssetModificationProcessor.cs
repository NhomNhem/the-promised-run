#if UNITY_EDITOR

using System.IO;
using UnityEditor;
using UnityEngine;

namespace OpenUtility.Data.Editor
{
    public class ScriptableVariableAssetModificationProcessor : AssetModificationProcessor
    {
        private static AssetDeleteResult OnWillDeleteAsset(string assetPath, RemoveAssetOptions options)
        {
            string extension = Path.GetExtension(assetPath);
            if (extension != ".asset")
                return (AssetDeleteResult.DidNotDelete);

            ScriptableObject asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(assetPath);

            if (asset is not ICanLoadValueFromPlayerPrefs loader) 
                return AssetDeleteResult.DidNotDelete;
            
            Optional<string> preference = loader.PlayerPref;
            if (!preference.HasValue) 
                return AssetDeleteResult.DidNotDelete;
            
            Debug.Log($"Deleting PlayerPref key '{preference.Value}' associated with asset '{asset.name}'.");
                    
            PlayerPrefs.DeleteKey(preference.Value);

            return AssetDeleteResult.DidNotDelete;
        }
    }
}

#endif