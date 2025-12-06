using System.Collections;
using Sirenix.OdinInspector;
using UnityEngine;

public class Drone : SpecialObject
{
    [Header("Move Settings")]
    [SerializeField] private float moveSpeed = 3f;          // 웨이포인트 사이 이동 속도
    [SerializeField] private float waitAtPoint = 0.3f;      // 각 포인트 도착 후 대기 시간
    [SerializeField] private bool loop = true;
    [SerializeField] private bool smoothStop = false;
    [SerializeField] private bool rotateMode = false;

    [Header("Direction")] 
    [SerializeField] private float rotateSpeed = 8f;        // 회전 속도 (deg/sec 느낌)

    private Coroutine moveRoutine;

    public override void Init(Transform[] pathPoints)
    {
        base.Init(pathPoints);
    }

    private IEnumerator FollowPathLoop()
    {
        while (true)
        {
            if (waypoints == null || waypoints.Length == 0)
                yield break;

            Transform target = waypoints[currentIndex];

            // 목표 지점까지 부드럽게 이동
            while (true)
            {
                if (target == null)
                    break;

                Vector3 toTarget = target.position - transform.position;
                toTarget.z = 0f;

                // 목표에 거의 도달했으면 루프 종료
                if (toTarget.sqrMagnitude < 0.005f)
                    break;

                Vector3 dir = toTarget.normalized;

                if (faceMoveDirection)
                {
                    if (dir.magnitude > 0.001f)
                    {
                        sr.flipX = dir.x < 0;
                    }
                }

                // 위치 이동
                float speed = moveSpeed;

                // smooth stop: distance-based 속도 감소
                if (smoothStop)
                {
                    float dist = toTarget.magnitude;
                    float slowRange = 2.0f; // 감속이 시작되는 거리
                    float t = Mathf.Clamp01(dist / slowRange);
                    speed = moveSpeed * t;  // 목표에 가까울수록 점점 느려짐
                }

                transform.position += dir * (speed * Time.deltaTime);

                // 이동 방향을 바라보도록 회전 (2D 기준 Z축 회전)
                if (rotateMode && dir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    Quaternion targetRot = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
                    transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, rotateSpeed * Time.deltaTime);
                }

                yield return null;
            }

            // 정확히 목표 위치로 스냅
            if (target != null)
                transform.position = target.position;

            // 대기 시간
            if (waitAtPoint > 0f)
                yield return new WaitForSeconds(waitAtPoint);

            // 다음 인덱스로 진행 (루프 여부에 따라)
            currentIndex++;
            if (currentIndex >= waypoints.Length)
            {
                if (loop)
                    currentIndex = 0;
                else
                    yield break;
            }
        }
    }

    public override void StartMoving()
    {
        base.StartMoving();

        if (moveRoutine == null && waypoints != null && waypoints.Length > 0)
        {
            moveRoutine = StartCoroutine(FollowPathLoop());
        }
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