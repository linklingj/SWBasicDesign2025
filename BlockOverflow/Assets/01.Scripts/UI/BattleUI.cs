using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BattleUI : MonoBehaviour {
    [SerializeField] RectTransform countdownUI;
    [SerializeField] Slider countdownSlider;
    [SerializeField] RectTransform startUI;
    
    [SerializeField] HealthBarSliderUI healthBarSliderUI1;
    [SerializeField] HealthBarSliderUI healthBarSliderUI2;

    private void Start()
    {
        countdownUI.gameObject.SetActive(false);
        startUI.gameObject.SetActive(false);
    }

    public void CountDown(PlayerController player1, PlayerController player2)
    {
        healthBarSliderUI1?.gameObject.SetActive(true);
        healthBarSliderUI2?.gameObject.SetActive(true);
        healthBarSliderUI1?.SetPlayer(player1.transform);
        healthBarSliderUI2?.SetPlayer(player2.transform);
        StartCoroutine(CountDownSequence());
    }
    
    private IEnumerator CountDownSequence()
    {
        //카운트다운 3초 시작
        Timer timer = new Timer();
        timer.Set(3f);
        
        //준비
        countdownUI.localScale = Vector3.zero;
        countdownUI.gameObject.SetActive(true);
        var countdownCanvasgroup = countdownUI.GetComponent<CanvasGroup>();
        countdownCanvasgroup.alpha = 0f;
        
        //카운트다운 애니메이션
        countdownUI.DOScale(Vector3.one, 0.3f).SetEase(Ease.OutBack);
        countdownCanvasgroup.DOFade(1f, 0.3f);
        countdownSlider.fillRect.GetComponent<Image>().DOColor(Color.red, 3f).From(Color.yellow);
        
        //카운트다운 진행
        while (!timer.IsFinished)
        {
            countdownSlider.value = timer.Progress;
            yield return null;
        }
        
        //카운트다운 종료
        countdownCanvasgroup.DOFade(0f, 0.2f);
        
        //시작 애니메이션
        startUI.gameObject.SetActive(true);
        startUI.DOMoveX(Screen.width/2, 0.3f).From(new Vector3(-Screen.width, startUI.anchoredPosition.y, 0)).SetEase(Ease.OutBack);
        
        yield return new WaitForSeconds(1f);
        
        startUI.DOScale(Vector3.zero, 0.3f).SetEase(Ease.OutSine);
    }
}
