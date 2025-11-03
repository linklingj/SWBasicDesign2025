using Sirenix.OdinInspector;
using UnityEngine;

public class TitleSceneUI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    [Button]
    public void StartGame()
    {
        SceneLoader.Instance.LoadScene(SceneName.CharacterSelection);
    }
    
    [Button]
    public void QuitGame()
    {
        Application.Quit();
    }
}
