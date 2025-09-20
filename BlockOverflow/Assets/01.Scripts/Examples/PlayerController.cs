// csharp
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public FSM<PlayerController> StateMachine { get; private set; }

    void Awake()
    {
        StateMachine = new FSM<PlayerController>(this);
    }

    void Start()
    {
        // set initial state
        StateMachine.Set<IdleState>();
        // call once to trigger OnBegin of initial state
        StateMachine.Update();
    }

    void Update()
    {
        // update FSM (will call current state's OnUpdate)
        StateMachine.Update();
    }
}