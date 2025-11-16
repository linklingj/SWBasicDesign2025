using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerJoinHandler : MonoBehaviour
{
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        // ✅ 각 플레이어의 Input Action Asset을 독립 복제
        playerInput.actions = Instantiate(playerInput.actions);

        // (선택) 디버그 로그
        Debug.Log($"[PlayerJoinHandler] Player {playerInput.playerIndex} joined with {playerInput.currentControlScheme}");
    }
}