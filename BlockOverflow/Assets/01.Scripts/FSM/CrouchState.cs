using UnityEngine;

public class CrouchState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        owner.StartCrouch();
        owner.ApplyMovement(); // 속도 낮춰서 유지
    }

    public override void OnUpdate(PlayerController owner)
    {
        // 숙인 상태에서도 사격 가능 (각도 제한은 무기 쪽 로직에서)
        if (owner.Player.Attack.WasPressedThisFrame())
        {
            owner.Fire();
        }

        // 점프는 허용(숙인 상태에서 점프 버튼 → 일어나며 점프할지 여부는 기획에 따라)
        if (owner.TryGroundOrBufferedJump())
        {
            owner.EndCrouch();
            owner.EnterAir();
            Set<AirState>();
            return;
        }

        // 아래키 해제 시 복귀
        if (!owner.IsPressingDown() && !owner.Player.Crouch.IsPressed())
        {
            // 이동 입력에 따라 Idle/Move 분기
            if (owner.GetMoveInput().sqrMagnitude > 0.0001f) Set<MoveState>();
            else Set<IdleState>();
            return;
        }

        // 계속 이동 반영(느린 속도)
        owner.ApplyMovement();
        owner.ClearWallStickLockoutOnLand();
    }

    public override void OnEnd(PlayerController owner)
    {
        owner.EndCrouch();
    }
}