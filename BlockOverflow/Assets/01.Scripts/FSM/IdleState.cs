using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StopImmediately();
        owner.EndCrouch();
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 숙이기 입력
        if (owner.IsPressingDown() && owner.IsGrounded())
        {
            Set<CrouchState>();
            return;
        }

        // 이동 시작
        if (Mathf.Abs(owner.GetMoveInput().x) > 0.01f)
        {
            Set<MoveState>();
            return;
        }

        // 점프
        if (owner.Player.Jump.WasPressedThisFrame())
        {
            if (owner.TryGroundOrBufferedJump())
            {
                
                Set<AirState>();
                return;
            }
        }

        // 낙하 (지면 이탈)
        if (!owner.IsGrounded())
        {
            Set<AirState>();
            return;
        }

        // 공격
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }
    }
}