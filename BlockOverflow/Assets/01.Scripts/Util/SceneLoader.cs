using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum SceneName
{
    Title,
    CharacterSelection,
    Battle,
    Reward
}

public class SceneLoader : Singleton<SceneLoader>
{
    public void LoadScene(SceneName sceneNameId, Action onComplete = null)
    {
        if (SceneManager.GetActiveScene().name == sceneNameId.ToString())
            return;
        
        StartCoroutine(LoadSceneAsync(sceneNameId, onComplete));
    }
    
    public void ReLoadScene(Action onComplete = null)
    {
        var currentScene = SceneManager.GetActiveScene();
        if (currentScene.name == SceneName.Title.ToString())
            return;

        StartCoroutine(LoadSceneAsync((SceneName)Enum.Parse(typeof(SceneName), currentScene.name), onComplete));
    }

    public SceneName GetCurrentScene()
    {
        var currentScene = SceneManager.GetActiveScene();
        return (SceneName)Enum.Parse(typeof(SceneName), currentScene.name);
    }

    private IEnumerator LoadSceneAsync(SceneName sceneName, Action onComplete)
    {
        var asyncOp = SceneManager.LoadSceneAsync(sceneName.ToString());
        while (!asyncOp.isDone)
            yield return null;

        onComplete?.Invoke();
    }
}
