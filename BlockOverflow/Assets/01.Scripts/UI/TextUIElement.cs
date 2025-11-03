using System;
using DG.Tweening;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

public class TextUIElement : MonoBehaviour
{
    protected TMP_Text text;
    protected RectTransform rt;

    public bool hideOnInit;

    public virtual void Awake()
    {
        text = GetComponent<TMP_Text>();
        rt = GetComponent<RectTransform>();
        if (hideOnInit) text.enabled = false;
    }

    public void HideText()
    {
        text.enabled = false;
    }

    public void ShowText()
    {
        text.enabled = true;
    }

    public void SetText(string t)
    {
        if(text) text.text = t;
    }

    public void SetText(int n)
    {
        text.text = n.ToString();
    }
    
    public void SetTextTween(string t, float delay = 0, int charPerSec = 100)
    {
        float duration = t.Length / (float)charPerSec;
        text.text = t;
        text.maxVisibleCharacters = 0;
        DOTween.To(x => text.maxVisibleCharacters = (int)x, 0, t.Length, duration)
            .SetDelay(delay)
            .SetEase(Ease.Linear);
    }
    
    public void StrikeThroughText(bool enable)
    {
        if (enable)
            text.fontStyle |= FontStyles.Strikethrough;
        else
            text.fontStyle &= ~FontStyles.Strikethrough;
    }

    //int에 대해서 부드럽게 숫자 전환
    public void SetNumTextTween(int n, float duration = 0.5f)
    {
        if (Int32.TryParse(text.text, out int prev))
        {
            DOTween.To((x) => text.text = ((int)x).ToString(), prev, n, duration);
        }
        else
        {
            text.text = n.ToString();
        }
    }

    public void SetEmphasisTextTween(float duration = 0.5f, float scaleUp = 1.3f, float shakeStrength = 5)
    {
        Quaternion originalRotation = text.transform.rotation;
        rt.DOScale(scaleUp, duration/2).SetEase(Ease.InOutCubic).SetLoops(2, LoopType.Yoyo);
        rt.DOShakeRotation(duration, new Vector3(0,0,shakeStrength), 5).OnComplete(() => rt.rotation = originalRotation);
    }

    public void SetTextColor(Color color)
    {
        text.color = color;
    }
    
    public void SetWrapText(bool enable)
    {
        text.textWrappingMode = enable ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
    }
}
