using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Frog : SpecialObject
{
    private static readonly int Jump = Animator.StringToHash("Jump");
    private static readonly int Idle = Animator.StringToHash("Idle");

    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.6f;  // 한 점프에 걸리는 시간
    [SerializeField] private float jumpHeight = 1.5f;    // 점프 높이

    [Header("Wait Settings")]
    [SerializeField] private float waitAtPoint = 0.5f;   // 포인트에 도착 후 대기 시간
    

    private Coroutine moveRoutine;

    public override void Init(Transform[] pathPoints)
    {
        base.Init(pathPoints);
        
    }

    private IEnumerator FollowPathLoop()
    {
        // 순환형으로 계속 반복
        while (true)
        {
            if (waypoints == null || waypoints.Length == 0)
                yield break;

            Transform target = waypoints[currentIndex];

            // 점프해서 해당 웨이포인트로 이동
            yield return StartCoroutine(JumpTo(target.position));

            // 도착 후 잠깐 대기
            if (waitAtPoint > 0f)
                yield return new WaitForSeconds(waitAtPoint);

            // 다음 인덱스 (순환)
            currentIndex = (currentIndex + 1) % waypoints.Length;
        }
    }

    private IEnumerator JumpTo(Vector3 targetPos)
    {
        Vector3 startPos = transform.position;

        // 방향 바라보기 (2D 기준으로 flip 사용)
        if (faceMoveDirection)
        {
            Vector3 dir = (targetPos - startPos);
            dir.z = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                if (sr != null)
                {
                    sr.flipX = dir.x < 0;
                }
                else
                {
                    var spriteRenderer = GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                        spriteRenderer.flipX = dir.x < 0;
                }
            }
        }

        anim.SetTrigger(Jump);

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / jumpDuration;
            float clampedT = Mathf.Clamp01(t);

            // 수평 이동 (시작~목표 직선 보간)
            Vector3 pos = Vector3.Lerp(startPos, targetPos, clampedT);

            // 포물선 높이: 4h * t(1-t) (0~1 구간에서 최고점 h를 가지는 포물선)
            float heightOffset = 4f * jumpHeight * clampedT * (1f - clampedT);
            pos.y += heightOffset;

            transform.position = pos;
            yield return null;
        }

        // 마지막에 딱 목표 위치로 스냅
        transform.position = targetPos;

        //anim.SetTrigger(Idle);
    }

    public void OnJumpEnd()
    {
        anim.SetTrigger(Idle);
    }

    public override void StartMoving()
    {
        base.StartMoving();
        
        if (moveRoutine == null)
        {
            moveRoutine = StartCoroutine(FollowPathLoop());
        }

        anim.SetTrigger(Idle);
    }

    public override void StopMoving()
    {
        base.StopMoving();
        
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }
}