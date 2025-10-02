using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager> {
    [SerializeField] private GameObject bullet;
    private void Start()
    {
        
    }

    private void Update()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            GameObject myBullet = ObjectPoolManager.Instance.Get(bullet, new Vector3(Random.Range(0,10), Random.Range(0,10),0), Quaternion.identity);
            Bullet b = myBullet.GetComponent<Bullet>();
            b.Init();
        }
        
    }
}