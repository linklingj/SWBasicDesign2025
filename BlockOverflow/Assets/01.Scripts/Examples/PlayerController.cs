// csharp
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Observable<int> health = new Observable<int>(100);
    public FSM<PlayerController> StateMachine { get; private set; }

    void Awake()
    {
        StateMachine = new FSM<PlayerController>(this);
    }

    void Start()
    {
        health.Set(100);
        // set initial state
        StateMachine.Set<IdleState>();
        // call once to trigger OnBegin of initial state
        StateMachine.Update();
    }

    void Update()
    {
        // update FSM (will call current state's OnUpdate)
        StateMachine.Update();
        
        if (Keyboard.current.hKey.wasPressedThisFrame)
        {
            health.Value -= 10;
        }
    }
}