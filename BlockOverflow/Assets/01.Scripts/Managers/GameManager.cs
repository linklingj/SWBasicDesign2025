using System;
using UnityEngine;

public class GameManager : Singleton<GameManager> 
{
    SceneLoader sceneLoader;
    
    GameState currentGameState;
    public GameState CurrentGameState { get => currentGameState; }
    public Action<GameState> OnGameStateChanged;

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

    public void StartNewGame()
    {
        SceneLoader.Instance.LoadScene(SceneName.CharacterSelection, () => ChangeGameState(GameState.CharacterSelection));
    }

    public void BattleStart()
    {
        SceneLoader.Instance.LoadScene(SceneName.Battle, () => ChangeGameState(GameState.Battle));

    }

    public void EndBattle(int winPlayerIndex)
    {
        SceneLoader.Instance.LoadScene(SceneName.Reward, () => ChangeGameState(GameState.Reward));
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
