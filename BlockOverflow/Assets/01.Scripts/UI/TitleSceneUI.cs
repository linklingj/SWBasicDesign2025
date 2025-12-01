using System;
using Sirenix.OdinInspector;
using UnityEngine;

public class TitleSceneUI : MonoBehaviour {
    [SerializeField] private AudioData titleBGM;

    private void Start()
    {
        AudioPlayer.Instance.PlayBGM(titleBGM);
    }

    [Button]
    public void StartGame()
    {
        AudioPlayer.Instance.FadeOutBGM(0.5f);
        AudioPlayer.Instance.Play("UI_Click_2");
        GameManager.Instance.StartNewGame();
    }
    
    [Button]
    public void QuitGame()
    {
        AudioPlayer.Instance.Play("UI_Click_2");
        Application.Quit();
    }
}
