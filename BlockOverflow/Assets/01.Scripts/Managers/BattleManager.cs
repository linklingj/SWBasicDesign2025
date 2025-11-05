using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public Observable<bool> gameStarted = new Observable<bool>(false);
    
    [SerializeField] private float prestartDelay = 2f;
    
    [SerializeField] private BattleUI battleUI;

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
        battleUI.CountDown();
        yield return new WaitForSeconds(3f);
        
        gameStarted.Value = true;
    }
}
