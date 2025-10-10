using UnityEngine;

public class IdleState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StopImmediately();
        owner.EndCrouch(); // 안전하게 서있는 상태로
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 지상 입력 확인
        if (owner.GetMoveInput().sqrMagnitude > 0.0001f)
        {
            Set<MoveState>();
            return;
        }

        // 아래키로 숙이기
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

        // 사격
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }

        // 지상 유지 중에는 벽 부착 락아웃 해제 가능
        owner.ClearWallStickLockoutOnLand();
    }
}