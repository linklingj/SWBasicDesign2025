using System;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    
    public void SetHealthUI(int health)
    {
        text.text = health.ToString();
    }
}
