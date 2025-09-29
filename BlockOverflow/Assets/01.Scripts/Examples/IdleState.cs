using UnityEngine;
public class IdleState : State<PlayerController>
{
    public override void OnBegin(PlayerController owner)
    {
        //애니메이션 넣기 (위 아래 움직임) -> 이거 지금 당장 넣어야하는건가
        Debug.Log("Entering Idle State");
    }

    public override void OnUpdate(PlayerController owner)
    {
        if (owner.GetMoveInput().sqrMagnitude > 0.0001f)
            Set<MoveState>();  // 움직임 생기면 Move로
    }

    public override void OnEnd(PlayerController owner)
    {
        Debug.Log("Exiting Idle State");
    }
}