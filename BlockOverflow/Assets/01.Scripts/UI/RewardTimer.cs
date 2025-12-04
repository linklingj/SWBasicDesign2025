using UnityEngine;
using TMPro;
using System.Collections;
using DG.Tweening;

public class RewardTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float waitTime = 30f; // 30초
    [SerializeField] UpgradeManager upgradeManager;
    [SerializeField] TextUIElement finishedText;

    private float remaining;

    private void Start()
    {
        // 초기: 작은 크기 + 회색
        timerText.color = Color.gray;
        timerText.transform.localScale = Vector3.one * 0.7f;

        remaining = waitTime;
        StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        int lastSecond = Mathf.CeilToInt(remaining);

        while (remaining > 0)
        {
            int currentSecond = Mathf.CeilToInt(remaining);

            if (currentSecond != lastSecond)
            {
                lastSecond = currentSecond;

                // 10초 미만이면 빨간색 + 더 큰 최대 크기
                if (currentSecond <= 10)
                {
                    timerText.color = Color.red;
                    timerText.transform.localScale = Vector3.one * 0.6f; // 시작은 더 작게
                    timerText.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
                }
                else
                {
                    timerText.color = Color.gray;
                    timerText.transform.localScale = Vector3.one * 0.8f;
                }

                // 초기 상태: 완전 불투명 + 원래 크기
                timerText.alpha = 1f;

                float maxScale = (currentSecond <= 10) ? 3f : 1f;
                timerText.transform.DOScale(maxScale, 0.1f).SetEase(Ease.OutBack);

                // 0.9초 동안 서서히 사라짐
                timerText.DOFade(0f, 0.9f);
                
                // 숫자 갱신
                timerText.text = currentSecond.ToString();
            }

            remaining -= Time.deltaTime;
            yield return null;
        }

        // 마지막 0 표시 연출
        timerText.text = "0";
        timerText.alpha = 1f;
        timerText.transform.localScale = Vector3.one * 0.8f;
        timerText.transform.DOScale(1f, 0.1f).SetEase(Ease.OutBack);
        timerText.DOFade(0f, 0.9f);
        
        finishedText.gameObject.SetActive(true);
        finishedText.SetSizeEmphasisTween();
        timerText.gameObject.SetActive(false);

        yield return new WaitForSeconds(3f);
        upgradeManager.OnCountdownFinished();
    }
}