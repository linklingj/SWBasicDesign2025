using TMPro;
using UnityEngine;

// 풀링되는 데미지 텍스트
public class DamageText : PoolObject
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float moveUpSpeed = 0.1f;  // 위로 떠오르는 속도
    [SerializeField] private float lifeTime = 0.1f;   // 유지 시간
    [SerializeField] private float fadeTime = 0.1f;   // 사라지는 시간

    private float _time;
    private Color _startColor;

    private void Awake()
    {
        if (text == null)
            text = GetComponent<TMP_Text>(); 

        _startColor = text.color;
    }


    public void Setup(float damage)
    {
        _time = 0f;

        var c = _startColor;
        c.a = 1f;
        text.color = c;

        text.text = damage.ToString();
        
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _time += Time.deltaTime;

        // 위로 이동
        transform.position += Vector3.up * (moveUpSpeed * Time.deltaTime);

        // 유지 시간 이후 페이드
        if (_time > lifeTime)
        {
            float t = (_time - lifeTime) / fadeTime;
            var c = _startColor;
            c.a = Mathf.Lerp(1f, 0f, t);
            text.color = c;

           
            if (_time > lifeTime + fadeTime)
            {
                Debug.Log("released!");
                Release();
            }
        }
    }
}