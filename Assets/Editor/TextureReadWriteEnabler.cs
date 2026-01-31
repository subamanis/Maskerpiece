#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

public static class TextureReadWriteEnabler
{
    [MenuItem("Tools/Textures/Enable Read-Write On Selected")]
    private static void EnableReadWriteOnSelected()
    {
        Object[] selection = Selection.GetFiltered(typeof(Texture2D), SelectionMode.Assets);
        int changed = 0;

        foreach (Object obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null || importer.isReadable)
            {
                continue;
            }

            importer.isReadable = true;
            importer.SaveAndReimport();
            changed++;
        }

        Debug.Log($"Enabled Read/Write on {changed} texture(s).");
    }
}
#endif
