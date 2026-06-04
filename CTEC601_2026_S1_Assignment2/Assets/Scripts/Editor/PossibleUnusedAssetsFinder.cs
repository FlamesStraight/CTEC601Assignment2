using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PossibleUnusedAssetsFinder : EditorWindow
{
    private Vector2 scrollPosition;
    private List<string> possiblyUnusedAssets = new List<string>();

    private bool includeScripts = false;
    private bool includeScenes = false;
    private bool includeResourcesFolder = false;
    private bool includeEditorFolder = false;
    private bool treatAllPrefabsAsUsed = true;

    [MenuItem("Tools/Asset Cleanup/Find Possibly Unused Assets - Hierarchy Scan")]
    public static void ShowWindow()
    {
        GetWindow<PossibleUnusedAssetsFinder>("Unused Assets Finder");
    }

    private void OnGUI()
    {
        GUILayout.Label("Find Possibly Unused Assets - Hierarchy Scan", EditorStyles.boldLabel);

        EditorGUILayout.HelpBox(
            "This version scans the currently open scene hierarchy directly. Open your final scene first, then click Scan Project. Still review results before deleting anything.",
            MessageType.Info
        );

        includeScripts = EditorGUILayout.Toggle("Include Scripts (.cs)", includeScripts);
        includeScenes = EditorGUILayout.Toggle("Include Scene Files (.unity)", includeScenes);
        includeResourcesFolder = EditorGUILayout.Toggle("Include Resources Folder", includeResourcesFolder);
        includeEditorFolder = EditorGUILayout.Toggle("Include Editor Folder", includeEditorFolder);
        treatAllPrefabsAsUsed = EditorGUILayout.Toggle("Treat All Prefabs As Used", treatAllPrefabsAsUsed);

        GUILayout.Space(10);

        if (GUILayout.Button("Scan Project"))
        {
            ScanProject();
        }

        GUILayout.Space(10);

        GUILayout.Label("Possibly Unused Assets: " + possiblyUnusedAssets.Count, EditorStyles.boldLabel);

        if (possiblyUnusedAssets.Count > 0)
        {
            if (GUILayout.Button("Copy List To Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = string.Join("\n", possiblyUnusedAssets);
                Debug.Log("Copied list to clipboard.");
            }
        }

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        foreach (string assetPath in possiblyUnusedAssets)
        {
            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.LabelField(assetPath);

            if (GUILayout.Button("Select", GUILayout.Width(70)))
            {
                Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                Selection.activeObject = asset;
                EditorGUIUtility.PingObject(asset);
            }

            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    private void ScanProject()
    {
        possiblyUnusedAssets.Clear();

        HashSet<string> allAssets = GetAllProjectAssets();
        HashSet<string> usedAssets = GetUsedAssetsFromOpenHierarchy();

        foreach (string asset in allAssets)
        {
            if (!usedAssets.Contains(asset))
            {
                possiblyUnusedAssets.Add(asset);
            }
        }

        possiblyUnusedAssets = possiblyUnusedAssets
            .OrderBy(path => path)
            .ToList();

        Debug.Log("Hierarchy scan complete. Possibly unused assets found: " + possiblyUnusedAssets.Count);
    }

    private HashSet<string> GetAllProjectAssets()
    {
        HashSet<string> assets = new HashSet<string>();

        string[] guids = AssetDatabase.FindAssets("", new[] { "Assets" });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (AssetDatabase.IsValidFolder(path))
                continue;

            if (ShouldIgnoreAsset(path))
                continue;

            assets.Add(path);
        }

        return assets;
    }

    private HashSet<string> GetUsedAssetsFromOpenHierarchy()
    {
        HashSet<string> usedAssets = new HashSet<string>();

        // Scan actual objects currently loaded in the open scenes
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            Scene scene = SceneManager.GetSceneAt(i);

            if (!scene.IsValid())
                continue;

            if (!string.IsNullOrEmpty(scene.path) && !ShouldIgnoreAsset(scene.path))
            {
                usedAssets.Add(scene.path);
            }

            GameObject[] rootObjects = scene.GetRootGameObjects();

            foreach (GameObject root in rootObjects)
            {
                Object[] dependencies = EditorUtility.CollectDependencies(new Object[] { root });

                foreach (Object dependency in dependencies)
                {
                    AddAssetAndItsDependencies(dependency, usedAssets);
                }
            }
        }

        // Extra safety: count enabled Build Settings scenes as used too
        foreach (EditorBuildSettingsScene buildScene in EditorBuildSettings.scenes)
        {
            if (!buildScene.enabled || string.IsNullOrEmpty(buildScene.path))
                continue;

            AddAssetPathAndDependencies(buildScene.path, usedAssets);
        }

        // Extra safety: if a prefab is not in scene but still important, this prevents false deletion
        if (treatAllPrefabsAsUsed)
        {
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            foreach (string guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                AddAssetPathAndDependencies(prefabPath, usedAssets);
            }
        }

        return usedAssets;
    }

    private void AddAssetAndItsDependencies(Object dependency, HashSet<string> usedAssets)
    {
        if (dependency == null)
            return;

        string path = AssetDatabase.GetAssetPath(dependency);

        if (string.IsNullOrEmpty(path))
            return;

        AddAssetPathAndDependencies(path, usedAssets);
    }

    private void AddAssetPathAndDependencies(string path, HashSet<string> usedAssets)
    {
        if (string.IsNullOrEmpty(path))
            return;

        if (ShouldIgnoreAsset(path))
            return;

        if (path.StartsWith("Assets/"))
        {
            usedAssets.Add(path);
        }

        string[] dependencies = AssetDatabase.GetDependencies(path, true);

        foreach (string dep in dependencies)
        {
            if (!ShouldIgnoreAsset(dep) && dep.StartsWith("Assets/"))
            {
                usedAssets.Add(dep);
            }
        }
    }

    private bool ShouldIgnoreAsset(string path)
    {
        if (string.IsNullOrEmpty(path))
            return true;

        string extension = Path.GetExtension(path).ToLower();

        if (extension == ".meta")
            return true;

        if (!includeScripts && extension == ".cs")
            return true;

        if (!includeScenes && extension == ".unity")
            return true;

        if (!includeResourcesFolder && path.Contains("/Resources/"))
            return true;

        if (!includeEditorFolder && path.Contains("/Editor/"))
            return true;

        return false;
    }
}