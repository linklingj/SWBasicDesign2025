using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        Debug.Log("Entering Idle State");
    }

    public override void OnUpdate(PlayerController owner)
    {
        Debug.Log("Updating Idle State");
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Set<MoveState>();
        }
    }

    public override void OnEnd(PlayerController owner)
    {
        Debug.Log("Exiting Idle State");
    }
}