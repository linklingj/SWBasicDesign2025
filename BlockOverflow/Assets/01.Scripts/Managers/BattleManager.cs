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
    private PlayerData playerData1;
    private PlayerData playerData2;
    
    private Maps map;

    private float gameTime;
    public float GameTime => gameTime;


    private void Awake()
    {
        GameManager.Instance.OnGameStateChanged += GameStateChanged;
    }

    private void Start()
    {
        GenerateMap();
        StartBattle();
    }

    private void Update()
    {
        gameTime += Time.deltaTime;
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
        cameraController.SetTargetPositions(map.originalCameraPos, map.finalCameraPos);
        StartCoroutine(PrestartSequence());
        
        SpawnPlayers();
    }
    
    void SpawnPlayers()
    {
        //플레이어 데이터 가져오기
        GameManager.Instance.GetPlayerData(out playerData1, 1);
        GameManager.Instance.GetPlayerData(out playerData2, 2);
        
        //플레이어 스폰
        player1 = Instantiate(playerPrefab, map.player1Spawn.position, map.player1Spawn.rotation)
            .GetComponent<PlayerController>();
        player2 = Instantiate(playerPrefab, map.player2Spawn.position, map.player2Spawn.rotation)
            .GetComponent<PlayerController>();
        
        player1.transform.SetParent(PlayerTransform);
        player2.transform.SetParent(PlayerTransform);
        
        //플레이어 무기 업그레이드 적용
        var p1Weapon = player1.GetComponent<WeaponController>();
        var p2Weapon = player2.GetComponent<WeaponController>();
        p1Weapon.SetUpgrades(playerData1.playerStats.damageIncrease, playerData1.playerStats.fireRateIncrease);
        p2Weapon.SetUpgrades(playerData2.playerStats.damageIncrease, playerData2.playerStats.fireRateIncrease);
        
        //플레이어 체력 초기화 및 죽음 이벤트 연결
        var p1Health = player1.GetComponent<PlayerHealth>();
        var p2Health = player2.GetComponent<PlayerHealth>();
        
        p1Health.OnDeath += () => OnPlayerDeath(1);
        p2Health.OnDeath += () => OnPlayerDeath(2);
        p1Health.Spawn(playerData1.playerStats.healthIncrease);
        p2Health.Spawn(playerData2.playerStats.healthIncrease);
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
        DOTween.To(()=> Time.timeScale, x=> Time.timeScale = x, 0.3f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        cameraController.ZoomTo(winnerTransform.position, CameraController.zoomType.Close, 1f);
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1;
        GameManager.Instance.EndBattle(1);
    }

    private IEnumerator PrestartSequence()
    {
        yield return new WaitForSeconds(prestartDelay);
        
        //countdown
        gameTime = -3f;
        battleUI.CountDown(player1, player2);
        cameraController.ZoomTo(player1.transform.position, CameraController.zoomType.Close);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(player2.transform.position, CameraController.zoomType.Close);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(map.originalCameraPos.position, CameraController.zoomType.Wide); //todo: 중앙 포인트로 변경
        yield return new WaitForSeconds(1f);
        gameTime = 0;
        cameraController.ShakeCamera(0.3f, 0.5f, 10);
        
        gameStarted.Value = true;
    }
}
