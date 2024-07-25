using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Tower : NetworkBehaviour
{
    public int maxHP = 100;
    public NetworkVariable<int> currentHP = new NetworkVariable<int>();
    private TowerManager towerManager;
    public Slider healthSlider;

    Turret turret;
    void Start()
    {
            turret = GetComponent<Turret>();

        towerManager = FindObjectOfType<TowerManager>();
    }
    public override void OnNetworkSpawn()
    {
        if(IsServer)
        {
            currentHP.Value = maxHP;
        }
        if(IsClient)
        {
            UpdateHealthBar(currentHP.Value);
            currentHP.OnValueChanged += OnCurrentHealthChanged;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            currentHP.OnValueChanged -= OnCurrentHealthChanged;
        }
    }

    private void OnCurrentHealthChanged(int previousValue, int newValue)
    {
        UpdateHealthBar(newValue);
    }

    public void TakeDamage(int damage)
    {
        currentHP.Value -= damage;
        Debug.Log("Tower HP: " + currentHP);

        if(currentHP.Value <= 0)
        {
            towerManager.HandleTowerDeath(this);
        }
    }

    public void Restore()
    {
        currentHP.Value = maxHP;
        Debug.Log("Tower is back with full HP: " + currentHP.Value);
        SetTurretFiring(true);
    }
    public void SetTurretFiring(bool canshoot)
    {
        if (turret)
        {
            turret.CanShoot = canshoot;
        }
    }
    void UpdateHealthBar(int value)
    {
        if(healthSlider != null)
        {
            SetHealth(value);
        }
    }
    

    public void SetHealth(int currentHP)
    {
        healthSlider.value = (float)currentHP;
    }

}