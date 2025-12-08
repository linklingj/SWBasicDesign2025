using UnityEngine;
using DG.Tweening;

public class PlayerCustomize : MonoBehaviour
{
    PlayerCustomization playerCustomization;
    [SerializeField] private SpriteRenderer playerBody;
    [SerializeField] private SpriteRenderer eye;
    [SerializeField] private SpriteRenderer hat;

    public void Init(PlayerCustomization playerCustomization)
    {
        this.playerCustomization = playerCustomization;
    }

    public void SetAll()
    {
        SetColor();
        SetHat(true);
    }

    public void SetColor(bool tween = false)
    {
        if (playerCustomization == null)
        {
            playerBody.color = Color.white;
            eye.color = Color.white;
            return;
        }
        
        if (tween)
        {
            playerBody.DOColor(playerCustomization.playerColor, 1f);
            eye.DOColor(playerCustomization.playerColor, 1f);
        }
        else
        {
            playerBody.color = playerCustomization.playerColor;
            eye.color = playerCustomization.playerColor;
        }
    }
    
    public void SetHat(bool active)
    {
        if (playerCustomization == null)
        {
            hat.sprite = null;
            return;
        }
        
        if (active)
        {
            hat.sprite = playerCustomization.hatSprite;
        }
        else
        {
            hat.sprite = null;
        }
    }
    
    public Color GetColor()
    {
        return playerBody.color;
    }
}
