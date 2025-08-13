using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class SceneManagerUtility
{
    public static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public static void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}