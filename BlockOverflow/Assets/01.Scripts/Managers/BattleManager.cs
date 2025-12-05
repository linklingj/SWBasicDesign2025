using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class BattleManager : SerializedMonoBehaviour
{
    public Observable<bool> gameStarted = new Observable<bool>(false);
    
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<GameObject> mapPrefabs;
    [SerializeField] private Dictionary<MapType, GameObject> bgObjects;
    
    [SerializeField] private float prestartDelay = 2f;
    
    [SerializeField] private CameraController cameraController;
    [SerializeField] private BattleUI battleUI;

    [SerializeField] private Transform StageTransform;
    [SerializeField] private Transform PlayerTransform;
    
    [SerializeField] private SpecialObject frog;
    [SerializeField] private float frogSpawnTime;

    [SerializeField] private Dictionary<WeaponType, WeaponData> weaponDatas;
    
    [Header("Audio")]
    [SerializeField] private AudioData voice3;
    [SerializeField] private AudioData voice2;
    [SerializeField] private AudioData voice1;
    [SerializeField] private AudioData voiceGo;
    [SerializeField] private AudioData gameStart;
    
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
        Time.timeScale = 1;
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
        m.SetActive(true);

        foreach (var bg in bgObjects)
            bg.Value.SetActive(false);
        bgObjects[map.mapType].SetActive(true);
        
        GameManager.Instance.currentMapType = map.mapType;
        
        AudioPlayer.Instance.PlayBGM(map.mapBGM);
        AudioPlayer.Instance.FadeInBGM(0.5f);
        
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
        
        //플레이어 무기 적용
        var p1Weapon = player1.GetComponent<WeaponController>();
        var p2Weapon = player2.GetComponent<WeaponController>();
        p1Weapon.SetWeapon(weaponDatas[playerData1.selectedWeaponType], 1);
        p2Weapon.SetWeapon(weaponDatas[playerData2.selectedWeaponType], 2);
        
        //업그레이드 적용
        p1Weapon.SetUpgrades(playerData1.playerStats.damageIncrease, 
            playerData1.playerStats.fireRateIncrease, 
            playerData1.playerStats.bulletSizeMultiplier,
            playerData1.playerStats.finalRevengeMultiplier,
            playerData1.playerStats.reflectOnWalls
            );
        p2Weapon.SetUpgrades(playerData2.playerStats.damageIncrease,
            playerData2.playerStats.fireRateIncrease,
            playerData2.playerStats.bulletSizeMultiplier,
            playerData2.playerStats.finalRevengeMultiplier
            ,playerData2.playerStats.reflectOnWalls
            );
        player1.SetUpgrades(playerData1.playerStats.speedIncrease, playerData1.playerStats.jumpIncrease);
        player2.SetUpgrades(playerData2.playerStats.speedIncrease, playerData2.playerStats.jumpIncrease);
        
        //플레이어 체력 초기화 및 죽음 이벤트 연결
        var p1Health = player1.GetComponent<PlayerHealth>();
        var p2Health = player2.GetComponent<PlayerHealth>();
        
        p1Health.OnDeath += () => OnPlayerDeath(1);
        p2Health.OnDeath += () => OnPlayerDeath(2);
        p1Health.Spawn(playerData1.playerStats.healthIncrease);
        p2Health.Spawn(playerData2.playerStats.healthIncrease);

        var customization1 = player1.GetComponent<PlayerCustomize>();
        customization1.Init(playerData1.customization);
        customization1.SetAll();
        var customization2 = player2.GetComponent<PlayerCustomize>();
        customization2.Init(playerData2.customization);
        customization2.SetAll();
    }
    
    public void OnPlayerDeath(int playerIdx)
    {
        StartCoroutine(StopBattle(playerIdx == 1 ? 2 : 1));
    }

    private IEnumerator StopBattle(int winnerIndex)
    {
        //todo: 배틀 종료 처리
        //승리 플레이어 확대
        //승리 애니메이션
        //슬로우
        gameStarted.Value = false;
        frog.StopMoving();
        player1.GetComponent<KillOutsideCamera>().DisableKill();
        player2.GetComponent<KillOutsideCamera>().DisableKill();
        battleUI.Win(winnerIndex);
        AudioPlayer.Instance.FadeOutBGM(1f);
        DOTween.To(()=> Time.timeScale, x=> Time.timeScale = x, 0.3f, 1f).SetEase(Ease.InQuad).SetUpdate(true);
        cameraController.ZoomTo((winnerIndex == 1)? player1.transform.position : player2.transform.position, CameraController.zoomType.Normal, 1f, -10.5f);
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1;
        GameManager.Instance.EndBattle(winnerIndex);
    }

    private IEnumerator PrestartSequence()
    {
        yield return new WaitForSeconds(prestartDelay);
        
        //countdown
        gameTime = -3f;
        battleUI.CountDown(player1, player2);
        cameraController.ZoomTo(player1.transform.position, CameraController.zoomType.Normal, 0.3f, -10.5f);
        AudioPlayer.Instance.Play(voice3);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(player2.transform.position, CameraController.zoomType.Normal, 0.3f, -10.5f);
        AudioPlayer.Instance.Play(voice2);
        yield return new WaitForSeconds(1f);
        cameraController.ZoomTo(map.originalCameraPos.position, CameraController.zoomType.Wide);
        AudioPlayer.Instance.Play(voice1);
        yield return new WaitForSeconds(1f);
        gameTime = 0;
        cameraController.ShakeCamera(0.3f, 0.5f, 10);
        AudioPlayer.Instance.Play(voiceGo);
        AudioPlayer.Instance.Play(gameStart);
        
        gameStarted.Value = true;
        SetSpecialObject();
    }
    
    private void SetSpecialObject()
    {
        if (frog != null && map.specialWayPoints != null)
        {
            frog.gameObject.SetActive(true);
            frog.Init(map.specialWayPoints.ToArray());
        }

        frog.OnDeath += GiveSpecialAbility;
        StartCoroutine(SpawnSpecialObjectAfterDelay(frogSpawnTime));
    }

    public void GiveSpecialAbility(int playerIdx)
    {
        if (playerIdx == 1)
        {
            player1.GetComponent<PlayerController>().specialAbility.Value = true;
        }
        else if (playerIdx == 2)
        {
            player2.GetComponent<PlayerController>().specialAbility.Value = true;
        }
    }
    
    private IEnumerator SpawnSpecialObjectAfterDelay(float delay)
    {
        yield return new WaitUntil(() => gameTime >= delay);
        frog.StartMoving();
    }
}
