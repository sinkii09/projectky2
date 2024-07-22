using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public int maxHP = 100;
    public int currentHP;
    private TowerManager towerManager;
    public Slider healthSlider;
    void Start()
    {
        currentHP = maxHP;
        towerManager = FindObjectOfType<TowerManager>();
        UpdateHealthBar();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Q))
        {
            TakeDamage(10);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHP -= damage;
        Debug.Log("Tower HP: " + currentHP);
        UpdateHealthBar();

        if(currentHP <= 0)
        {
            towerManager.HandleTowerDeath(this);
        }
    }

    public void Restore()
    {
        currentHP = maxHP;
        Debug.Log("Tower is back with full HP: " + currentHP);
        UpdateHealthBar();
        gameObject.SetActive(true);
    }

    void UpdateHealthBar()
    {
        if(healthSlider != null)
        {
            SetHealth(currentHP);
        }
    }
    

    public void SetHealth(int currentHP)
    {
        healthSlider.value = (float)currentHP;
    }
}