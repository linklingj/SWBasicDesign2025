using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Observable<bool> gameStarted = new Observable<bool>(false);
    
    [SerializeField] private GameObject playerPrefab;
    
    [SerializeField] private float prestartDelay = 2f;
    
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;
    
    [SerializeField] private CameraController cameraController;
    [SerializeField] private BattleUI battleUI;

    private PlayerController player1;
    private PlayerController player2;


    private void Awake()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChanged;

    }

    public void GameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Battle)
            StartBattle();
    }

    
    [Button]
    public void StartBattle()
    {
        gameStarted.Value = false;
        StartCoroutine(PrestartSequence());
        
        //플레이어 스폰
        player1 = Instantiate(playerPrefab, player1SpawnPoint.position, player1SpawnPoint.rotation)
            .GetComponent<PlayerController>();
        player2 = Instantiate(playerPrefab, player2SpawnPoint.position, player2SpawnPoint.rotation)
            .GetComponent<PlayerController>();
        
        
    }

    [Button]
    public void StopBattle()
    {
        //todo: 배틀 종료 처리
        //승리 플레이어 확대
        //승리 애니메이션
        //슬로우
        GameManager.Instance.EndBattle(1);
    }

    private IEnumerator PrestartSequence()
    {
        yield return new WaitForSeconds(prestartDelay);
        
        //countdown
        battleUI.CountDown(player1, player2);
        cameraController.ZoomTo(player1.transform.position, CameraController.zoomType.Close);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(player2.transform.position, CameraController.zoomType.Close);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(Vector3.zero, CameraController.zoomType.Wide); //todo: 중앙 포인트로 변경
        yield return new WaitForSeconds(1f);
        cameraController.ShakeCamera(0.3f, 0.5f, 10);
        
        gameStarted.Value = true;
    }
}
