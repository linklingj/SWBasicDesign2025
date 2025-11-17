using UnityEngine;
using TMPro;
using System.Collections;

public class RewardTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float waitTime = 30f; // 30초

    private float remaining;

    private void Start()
    {
        remaining = waitTime;
        StartCoroutine(TimerRoutine());
    }

    private IEnumerator TimerRoutine()
    {
        while (remaining > 0)
        {
            // SS:MS 형식 만들기
            int totalMilliseconds = Mathf.Max(0, (int)(remaining * 1000)); // 밀리초 단위 변환
            int seconds = totalMilliseconds / 1000;
            int ms = (totalMilliseconds % 1000) / 10; // 0~99까지만 보이게 (1/100초 단위)

            timerText.text = $"{seconds:00}:{ms:00}";

            remaining -= Time.deltaTime;
            yield return null;
        }

        // 마지막 00:00 표시
        timerText.text = "00:00";

        // 씬 이동
        SceneLoader.Instance.LoadScene(SceneName.Battle);
    }
}