using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class SpecialObject : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] private Transform[] waypoints;  // 개구리가 따라갈 경로

    [Header("Jump Settings")]
    [SerializeField] private float jumpDuration = 0.6f;  // 한 점프에 걸리는 시간
    [SerializeField] private float jumpHeight = 1.5f;    // 점프 높이

    [Header("Wait Settings")]
    [SerializeField] private float waitAtPoint = 0.5f;   // 포인트에 도착 후 대기 시간

    [Header("Options")]
    [SerializeField] private bool playOnStart = true;    // 시작하자마자 자동 이동할지
    [SerializeField] private bool faceMoveDirection = true; // 이동 방향 바라보기 (2D면 flip 등으로 응용)

    private int currentIndex = 0;
    private Coroutine moveRoutine;

    private void Start()
    {
        if (playOnStart && waypoints != null && waypoints.Length > 0)
        {
            transform.position = waypoints[0].position;
            moveRoutine = StartCoroutine(FollowPathLoop());
        }
    }
    
    public void Init(Transform[] pathPoints)
    {
        waypoints = pathPoints;
        currentIndex = 0;
        transform.position = waypoints[0].position;

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

        // 방향 바라보기 (원하면 2D 기준으로 회전/flip으로 바꿔도 됨)
        if (faceMoveDirection)
        {
            Vector3 dir = (targetPos - startPos);
            dir.z = 0f;
            if (dir.sqrMagnitude > 0.0001f)
            {
                // 2D라면: x축 방향에 따라 flipX 하는 식으로 응용
                GetComponent<SpriteRenderer>().flipX = dir.x < 0;
                transform.right = dir.normalized; // 3D/2D 공용 간단 버전
            }
        }

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
    }

    // 외부에서 시작/정지 제어하고 싶으면 이런 메서드도 쓸 수 있음
    [Button]
    public void StartMoving()
    {
        if (moveRoutine == null && waypoints != null && waypoints.Length > 0)
        {
            moveRoutine = StartCoroutine(FollowPathLoop());
        }
    }

    public void StopMoving()
    {
        if (moveRoutine != null)
        {
            StopCoroutine(moveRoutine);
            moveRoutine = null;
        }
    }
}