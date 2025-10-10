using UnityEngine;

public class MoveState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner) { }

    public override void OnUpdate(PlayerController owner)
    {
        owner.ApplyMovement();

        // 이동 중 입력 0 → Idle
        if (owner.GetMoveInput().sqrMagnitude < 0.0001f)
        {
            Set<IdleState>();
            return;
        }

        // 아래키 → Crouch
        if (owner.IsPressingDown() || owner.Player.Crouch.IsPressed())
        {
            Set<CrouchState>();
            return;
        }

        // 점프(버퍼/코요테)
        if (owner.TryGroundOrBufferedJump())
        {
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 이동 중에도 사격 가능
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }

        owner.ClearWallStickLockoutOnLand();
    }
}