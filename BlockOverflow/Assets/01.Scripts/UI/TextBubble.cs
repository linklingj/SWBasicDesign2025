using System;
using System.Text;
using UnityEngine;
using DG.Tweening;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine.UI;

public class TextBubble : MonoBehaviour
{
    [SerializeField] GameObject bubble;
    [SerializeField] GameObject icon;
    [SerializeField] TextUIElement text;
    [SerializeField] int charsPerLine = 30;

    public float animationDuration = 0.3f;

    private bool isActive = false;

    private void Awake()
    {
        isActive = false;
        bubble.SetActive(false);
        bubble.transform.localScale = Vector3.zero;
        icon.SetActive(false);
    }

    [Button]
    public void ShowTextBubble()
    {
        if (isActive) return;
        isActive = true;
        bubble.SetActive(true);
        bubble.transform.localScale = Vector3.zero;
        bubble.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
    }

    [Button]
    public void HideTextBubble()
    {
        if (!isActive) return;
        isActive = false;
        bubble.transform.DOScale(Vector3.zero, animationDuration)
            .SetEase(Ease.OutSine)
            .OnComplete(() => bubble.SetActive(false));
    }
    
    [Button]
    public void TextBubbleSetText(string t, bool format = true)
    {
        icon.SetActive(false);
        string formatted = (format)? InsertLineBreaks(t, charsPerLine) : t;
        if (!isActive)
        {
            ShowTextBubble();
            text.SetTextTween(formatted, animationDuration);
        }
        else text.SetTextTween(formatted);
    }

    public void TextBubbleSetIconText(Sprite sprite, string t)
    {
        icon.SetActive(true);
        icon.transform.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack);
        icon.GetComponent<Image>().sprite = sprite;
        if (!isActive)
        {
            ShowTextBubble();
            text.SetTextTween(t, animationDuration, 150);
        }
        else text.SetTextTween(t, 0, 150);
    }
    
    [Button]
    public void SetAnchorBattle()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.15f, 0.85f);
        rt.anchorMax = new Vector2(0.15f, 0.85f);
        rt.anchoredPosition = Vector2.zero;
    }
    
    [Button]
    public void SetAnchorStage()
    {
        var rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.85f);
        rt.anchorMax = new Vector2(0.5f, 0.85f);
        rt.anchoredPosition = Vector2.zero;
    }

    //mode 0: 단순 n글자마다 줄바꿈, 1: 단어 단위 줄바꿈
    private static string InsertLineBreaks(string source, int n, bool mode = false)
    {
        if (string.IsNullOrEmpty(source) || n <= 0) return source;
        var sb = new StringBuilder(source.Length + source.Length / n + 8);
        if (mode == false)
        {
            for (int i = 0; i < source.Length; i++)
            {
                sb.Append(source[i]);
                if ((i + 1) % n == 0 && (i + 1) != source.Length)
                    sb.Append('\n');
            }
        }
        else if (mode == true)
        {
            int charCount = 0;
            string[] words = source.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                string word = words[i];
                if (charCount + word.Length > n)
                {
                    if (i != 0)
                        sb.Append('\n');
                    charCount = 0;
                }
                else if (i != 0)
                {
                    sb.Append(' ');
                    charCount++;
                }
                sb.Append(word);
                charCount += word.Length;
            }
        }
        return sb.ToString();
    }
}