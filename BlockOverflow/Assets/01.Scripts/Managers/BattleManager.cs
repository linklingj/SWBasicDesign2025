using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public Observable<bool> gameStarted = new Observable<bool>(false);
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> mapPrefabs;
    
    [SerializeField] private float prestartDelay = 2f;
    
    [SerializeField] private CameraController cameraController;
    [SerializeField] private BattleUI battleUI;

    [SerializeField] private Transform StageTransform;
    [SerializeField] private Transform PlayerTransform;
    
    private PlayerController player1;
    private PlayerController player2;
    
    private Maps map;


    private void Awake()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChanged;
    }

    private void Start()
    {
        GenerateMap();
        StartBattle();
    }

    public void GameStateChanged(GameState gameState)
    {
        if (gameState == GameState.Battle)
            StartBattle();
    }
    
    public int GenerateMap()
    {
        int randIdx = Random.Range(0, mapPrefabs.Count);
        
        GameObject m = Instantiate(mapPrefabs[randIdx], StageTransform);
        m.transform.position = Vector3.zero;
        map = m.GetComponent<Maps>();

        return randIdx;
    }
    
    [Button]
    public void StartBattle()
    {
        gameStarted.Value = false;
        StartCoroutine(PrestartSequence());
        
        //플레이어 스폰
        player1 = Instantiate(playerPrefab, map.player1Spawn.position, map.player1Spawn.rotation)
            .GetComponent<PlayerController>();
        player2 = Instantiate(playerPrefab, map.player2Spawn.position, map.player2Spawn.rotation)
            .GetComponent<PlayerController>();
        player1.transform.SetParent(PlayerTransform);
        player2.transform.SetParent(PlayerTransform);
        player1.GetComponent<PlayerHealth>().OnDeath += () => OnPlayerDeath(1);
        player2.GetComponent<PlayerHealth>().OnDeath += () => OnPlayerDeath(2);
    }
    
    public void OnPlayerDeath(int playerIdx)
    {
        StartCoroutine(StopBattle(playerIdx == 1 ? player2.transform : player1.transform));
    }

    private IEnumerator StopBattle(Transform winnerTransform)
    {
        //todo: 배틀 종료 처리
        //승리 플레이어 확대
        //승리 애니메이션
        //슬로우
        gameStarted.Value = false;
        DOTween.To(()=> Time.timeScale, x=> Time.timeScale = x, 0.2f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        cameraController.ZoomTo(winnerTransform.position, CameraController.zoomType.Close, 1f);
        yield return new WaitForSeconds(2f);
        Time.timeScale = 1;
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
        cameraController.ZoomTo(map.originalCameraPos.position, CameraController.zoomType.Wide); //todo: 중앙 포인트로 변경
        yield return new WaitForSeconds(1f);
        cameraController.ShakeCamera(0.3f, 0.5f, 10);
        
        gameStarted.Value = true;
    }
}
