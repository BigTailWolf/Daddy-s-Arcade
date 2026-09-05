using UnityEditor;
using UnityEditor.SceneManagement;

[InitializeOnLoad]
public static class ArcadeSetup
{
    static ArcadeSetup() { EditorApplication.delayCall += EnsureScene; }
    static void EnsureScene()
    {
        const string path = "Assets/Arcade/Home.unity";
        if (System.IO.File.Exists(path) || EditorApplication.isPlayingOrWillChangePlaymode) return;
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
        EditorSceneManager.SaveScene(scene, path);
        EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(path, true) };
        EditorSceneManager.CloseScene(scene, true);
    }
}
