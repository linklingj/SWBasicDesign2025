using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawner : MonoBehaviour
{
    [SerializeField] private PlayerInput playerPrefab;
    [SerializeField] private Transform[] spawnPoints;

    private void Start()
    {
        var pads = Gamepad.all;
        if (pads.Count == 0)
        {
            Debug.LogWarning("🎮 No gamepads found.");
            return;
        }

        // 최대 2명까지만 스폰
        for (int i = 0; i < Mathf.Min(pads.Count, spawnPoints.Length); i++)
        {
            SpawnPlayer(i, pads[i]);
        }
    }

    private void SpawnPlayer(int index, Gamepad pad)
    {
        var spawnPos = spawnPoints.Length > index ? spawnPoints[index].position : Vector3.zero;

        // ✅ 핵심: PlayerInput.Instantiate() 사용
        var playerInput = PlayerInput.Instantiate(
            playerPrefab.gameObject,
            controlScheme: "Gamepad",
            pairWithDevice: pad
        );

        playerInput.transform.position = spawnPos;
        playerInput.gameObject.name = $"Player_{index + 1}";
        Debug.Log($"✅ Spawned {playerInput.name} paired with {pad.displayName}");
    }
}