using UnityEngine;

public class KillOutsideCamera : MonoBehaviour
{

    [SerializeField] private Camera cam;

    // 화면 바깥 여유 범위 (0.05 = 5% 여유)
    [SerializeField] private float margin = -0.1f;
    private PlayerHealth health;
    private bool _enabled = false;
    
    
    [SerializeField] private float startGraceTime = 3.5f;
    private float _timer;

    private void Awake()
    {
        if (cam == null)
            cam = Camera.main;
        health = GetComponent<PlayerHealth>();
        _timer = startGraceTime;
        _enabled = false;
        
    }

    private void Update()
    {
        if (cam == null) return;
        if (!_enabled)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0f)
                _enabled = true;
            return;
        }

        Vector3 vpPos = cam.WorldToViewportPoint(transform.position);

        // 카메라 뒤는 제외
        if (vpPos.z <= 0f)
            return;

        // 화면 + 여유 범위 밖인지 체크
        if (vpPos.x < 0f - margin ||
            vpPos.x > 1f + margin ||
            vpPos.y < 0f - margin ||
            vpPos.y > 1f + margin)
        {
            health.Die();
        }
    }
    
    

    public void EnableKill()
    {
        _enabled = true;
    }


}
