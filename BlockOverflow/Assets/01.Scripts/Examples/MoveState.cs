using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        Debug.Log("Entering Move State");
    }

    public override void OnUpdate(PlayerController owner)
    {
        Debug.Log("Updating Move State");
        
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Set<IdleState>();
        }
    }

    public override void OnEnd(PlayerController owner)
    {
        Debug.Log("Exiting Move State");
    }
}