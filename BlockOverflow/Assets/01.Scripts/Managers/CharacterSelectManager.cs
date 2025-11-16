using UnityEngine;

public class CharacterSelectManager : MonoBehaviour
{
    
    public void StartGame()
    {
        GameManager.Instance.BattleStart();
    }
}
