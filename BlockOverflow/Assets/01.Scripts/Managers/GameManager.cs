using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : Singleton<GameManager> 
{
    SceneLoader sceneLoader;
    
    GameState currentGameState;
    public GameState CurrentGameState { get => currentGameState; }
    public Action<GameState> OnGameStateChanged;

    [SerializeField] private PlayerData playerData1;
    [SerializeField] private PlayerData playerData2;
    
    private List<int> winPlayerIndices = new List<int>();

    protected override void Awake()
    {
        base.Awake();
        
        sceneLoader = gameObject.AddComponent<SceneLoader>();
        ChangeGameState(GetGameStateFromScene(SceneLoader.Instance.GetCurrentScene()));
    }
    
    
    //게임 상태 변화시켜주는 함수
    public void ChangeGameState(GameState gameState)
    {
        currentGameState = gameState;
        Debug.Log("Game State: " + gameState.ToString());

        OnGameStateChanged?.Invoke(gameState);
    }
    
    void GeneratePlayerData()
    {
        playerData1 = ScriptableObject.CreateInstance<PlayerData>();
        playerData2 = ScriptableObject.CreateInstance<PlayerData>();
    }

    public void StartNewGame()
    {
        winPlayerIndices.Clear();
        GeneratePlayerData();
        SceneLoader.Instance.LoadScene(SceneName.CharacterSelection, () => ChangeGameState(GameState.CharacterSelection));
    }

    public void BattleStart()
    {
        if (playerData1 == null || playerData2 == null) GeneratePlayerData();
        SceneLoader.Instance.LoadScene(SceneName.Battle);
    }

    public void EndBattle(int winPlayerIndex)
    {
        winPlayerIndices.Add(winPlayerIndex);
        SceneLoader.Instance.LoadScene(SceneName.Reward, () => ChangeGameState(GameState.Reward));
    }

    public int GetPreviousWinner()
    {
        if (winPlayerIndices.Count == 0)
            return 1;
        return winPlayerIndices[winPlayerIndices.Count - 1];
    }
    
    public int GetPreviousLoser()
    {
        if (winPlayerIndices.Count == 0)
            return 2;
        return winPlayerIndices[winPlayerIndices.Count - 1] == 1 ? 2 : 1;
    }
    
    public void GetPlayerData(out PlayerData playerData, int playerIndex = -1)
    {
        if (playerIndex == -1)
            playerIndex = GetPreviousWinner();

        if (playerData1 == null || playerData2 == null) GeneratePlayerData();
        playerData = playerIndex == 1 ? playerData1 : playerData2;
    }

    private static GameState GetGameStateFromScene(SceneName sceneName)
    {
        switch (sceneName)
        {
            case SceneName.Title:
                return GameState.Title;
            case SceneName.CharacterSelection:
                return GameState.CharacterSelection;
            case SceneName.Battle:
                return GameState.Battle;
            case SceneName.Reward:
                return GameState.Reward;
            default:
                throw new ArgumentOutOfRangeException(nameof(sceneName), sceneName, null);
        }
    }
}
