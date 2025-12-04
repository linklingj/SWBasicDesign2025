using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class ResultManager : MonoBehaviour {
    [SerializeField] private List<GameObject> box1;
    [SerializeField] private List<GameObject> box2;
    [SerializeField] private GameObject boxGroup1;
    [SerializeField] private GameObject boxGroup2;
    [SerializeField] private PlayerController p1;
    [SerializeField] private PlayerController p2;
    [SerializeField] private TextUIElement winText;
    [SerializeField] private GameObject winEffect;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private AudioData resultBGM;
    [SerializeField] private AudioData congratulationSFX;
    [SerializeField] private Material bg;
    [SerializeField] private GameObject toTitleButton;
    

    private void Awake()
    {
        bg.DOColor(new Color(0.8f,0.8f,0.8f), "_Color", 0f);
        p1.SetControl(false);
        p2.SetControl(false);
    }

    private void Start()
    {
        StartCoroutine(ShowResult());
        AudioPlayer.Instance.FadeOutBGM(1);
    }

    IEnumerator ShowResult()
    {
        yield return new WaitForSeconds(1f);
        int wc1 = 0, wc2 = 0;
        foreach (int wp in GameManager.Instance.winPlayerIndices)
        {
            if (wp == 1)
            {
                box1[wc1].transform.DOScaleY(1, 0.5f).SetEase(Ease.OutSine);
                box1[wc1].transform.localScale = new Vector3(1, 0, 1);
                box1[wc1].SetActive(true);
                wc1++;
            }
            else if (wp == 2)
            {
                box2[wc2].SetActive(true);
                box2[wc2].transform.localScale = new Vector3(1, 0, 1);
                box2[wc2].transform.DOScaleY(1, 0.5f).SetEase(Ease.OutSine);
                wc2++;
            }
            yield return new WaitForSeconds(0.5f);
        }

        int finalWinner = GameManager.Instance.GetPreviousWinner();
        int finalLoser = finalWinner == 1 ? 2 : 1;
        
        GameObject loserBoxGroup = finalLoser == 1 ? boxGroup1 : boxGroup2;
        loserBoxGroup.transform.DORotate(new Vector3(0, 0, finalLoser == 1 ? 60 : -60), 3f).SetEase(Ease.InExpo);
        loserBoxGroup.transform.DOMoveY(-15, 2f).SetEase(Ease.InExpo).SetDelay(1.5f);
        AudioPlayer.Instance.Play("Player_Fall");
        
        yield return new WaitForSeconds(2.5f);
        cameraController.ShakeCamera(0.5f, 5, 20);
        yield return new WaitForSeconds(0.5f);
        
        winText.gameObject.SetActive(true);
        winText.SetText($"Player {finalWinner} Victory!");
        winText.SetSizeEmphasisTween();
        
        GameManager.Instance.GetPlayerData(out PlayerData data, finalWinner);
        bg.SetVector("_Tiling", new Vector2(10,10));
        bg.DOColor(Color.Lerp(data.customization.playerColor, Color.white, 0.5f), "_Color", 1f);
        winEffect.SetActive(true);
        
        AudioPlayer.Instance.Play(congratulationSFX);
        
        Transform winnerTransform = finalWinner == 1 ? p1.transform : p2.transform;
        cameraController.ZoomTo(winnerTransform.position, CameraController.zoomType.Close, 1f);
        
        yield return new WaitForSeconds(1f);
        AudioPlayer.Instance.PlayBGM(resultBGM);
        
        toTitleButton.SetActive(true);
    }

    public void ToTitle()
    {
        GameManager.Instance.ToTitle();
    }
}
