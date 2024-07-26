using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class Tower : NetworkBehaviour, IDamageable
{
    public int maxHP = 100;
    public NetworkVariable<int> currentHP = new NetworkVariable<int>();
    public NetworkVariable<bool> isDisable = new NetworkVariable<bool>(false);
    private TowerManager towerManager;
    public Slider healthSlider;

    Turret turret;

    LayerMask targetLayer;
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
            targetLayer = LayerMask.GetMask("Projectiles");
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
        isDisable.Value = false;
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

    public void ReceiveHP(int HP, ServerCharacter inflicter = null)
    {
        TakeDamage(HP);
    }

    public IDamageable.SpecialDamageFlags GetSpecialDamageFlags()
    {
        return IDamageable.SpecialDamageFlags.None;
    }

    public bool IsDamageable()
    {
        return !isDisable.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(IsServer)
        {
            if(((1 << other.gameObject.layer) & targetLayer) != 0)
            {
                ReceiveHP(10);
            }
        }
    }
}