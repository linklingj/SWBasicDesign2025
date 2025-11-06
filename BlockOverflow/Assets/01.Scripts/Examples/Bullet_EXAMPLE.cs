using System;
using UnityEngine;

public class Bullet_EXAMPLE : PoolObject {
    public void Init()
    {
        
    }
    private void Update()
    {
        Move();
    }

    public void Move()
    {
        transform.Translate(new Vector3(1,0,0) * Time.deltaTime);
        if (transform.position.x > 10 || transform.position.x < -10)
        {
            Release();
        }
    }
}