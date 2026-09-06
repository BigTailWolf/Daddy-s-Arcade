using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class ArcadeSetup
{
    static ArcadeSetup() { EditorApplication.delayCall += EnsureScene; }
    static void EnsureScene()
    {
        const string path = "Assets/Arcade/Home.unity";
        if (System.IO.File.Exists(path) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        // Unity rejects additive creation while an untitled scene is open.
        // Leave the user's scene intact; the explicit menu handles saving it.
        for (int i = 0; i < SceneManager.sceneCount; i++)
            if (string.IsNullOrEmpty(SceneManager.GetSceneAt(i).path)) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        EditorSceneManager.CloseScene(scene, true);
    }

    [MenuItem("Daddy's Arcade/Open Home")]
    public static void OpenHome()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        const string path = "Assets/Arcade/Home.unity";
        if (!System.IO.File.Exists(path)) {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            EditorSceneManager.SaveScene(scene, path);
        } else {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        }
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
    }
}
