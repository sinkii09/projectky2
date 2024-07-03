using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;

public class test : MonoBehaviour
{
    public GameObject bulletPrefab;
    [SerializeField] int amount = 10;
    public float radius = 5f;
    public float bulletSpeed = 5f;
    public List<GameObject> bullets;
    bool isLaunch;
    private void Start()
    {
        bullets = new List<GameObject>();
    }
    private void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if(bullets.Count > 0)
            {
                foreach(GameObject bullet in bullets)
                {
                    Destroy(bullet);
                }
                bullets.Clear();
                isLaunch = false;
            }
            SpawnBullet();
        }
        if(isLaunch)
        {
            foreach(GameObject bullet in bullets)
            {
                bullet.transform.Translate(bullet.transform.forward * bulletSpeed * Time.deltaTime);
            }
        }
    }
    void SpawnBullet()
    {
        for (int i = 0; i < amount; i++)
        {
            float angle = i * Mathf.PI * 2f / amount;
            Vector3 bulletPos = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * radius + transform.position;
            GameObject bullet = Instantiate(bulletPrefab, bulletPos, Quaternion.identity);
            bullet.transform.forward = -(bulletPos - transform.position).normalized;
            bullets.Add(bullet);

        }
        isLaunch = true;
    }
}

