using System;
using UnityEngine;
using System.Collections;
using Sirenix.OdinInspector;

public class SpecialObject : MonoBehaviour
{
    [Header("Waypoints")]
    [SerializeField] protected Transform[] waypoints;  // 경로를 따라갈 웨이포인트
    protected int currentIndex = 0;
    
    [Header("Options")]
    [SerializeField] protected bool faceMoveDirection = true; // 이동 방향 바라보기 (2D면 flip 등으로 응용)
    [SerializeField] protected Vector3 startingPos;

    [Header("Hurt Flash Settings")] 
    [SerializeField] private Color hurtColor;

    public Action<int> OnDeath;
    
    protected NPCHealth health;
    protected SpriteRenderer sr;
    protected Animator anim;
    Color originalColor;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        health = GetComponent<NPCHealth>();
        sr = GetComponent<SpriteRenderer>();
    }


    public virtual void Init(Transform[] pathPoints)
    {
        waypoints = pathPoints;
        currentIndex = 0;
        transform.position = startingPos;
        originalColor = sr != null ? sr.color : GetComponent<SpriteRenderer>().color;
        health.OnDeath += Death;
        health.OnHit += Damaged;
    }
    
    public virtual void StartMoving()
    {
        if (waypoints == null && waypoints.Length <= 0) return;
        transform.position = startingPos;
    }

    public virtual void StopMoving()
    {
        
    }

    void Death(int damagingPlayerIdx)
    {
        anim.SetTrigger("Death");
        OnDeath?.Invoke(damagingPlayerIdx);
        StartCoroutine(Kill());
    }

    void Damaged()
    {
        StartCoroutine(HitFlash());
    }
    
    private IEnumerator HitFlash()
    {
        if (sr == null)
        {
            yield break;
        }
        sr.color = hurtColor;
        yield return new WaitForSeconds(0.1f);
        sr.color = originalColor;
    }

    private IEnumerator Kill()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}