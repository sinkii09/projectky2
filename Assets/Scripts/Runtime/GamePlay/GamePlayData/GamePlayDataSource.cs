using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GamePlayDataSource : MonoBehaviour 
{
    public static GamePlayDataSource Instance { get; private set; }

    [SerializeField]
    private Skill[] m_SkillPrototypes;

    List<Skill> m_AllSkills;

    [SerializeField]
    private WeaponData[] m_weaponDatas;

    List<WeaponData> m_AllWeapons;

    private void Awake()
    {
        if (Instance != null)
        {
            throw new System.Exception("Multiple GameDataSources defined!");
        }
        DontDestroyOnLoad(gameObject);
        Instance = this;

        BuildSkillIDs();
        BuildWeaponIDs();
    }
    public Skill GetSkillPrototypeByID(SkillID index)
    {
        return m_AllSkills[index.ID];
    }
    public bool TryGetSkillPrototypeByID(SkillID index, out Skill skill)
    {
        for (int i = 0; i < m_AllSkills.Count; i++)
        {
            if (m_AllSkills[i].SkillID == index)
            {
                skill = m_AllSkills[i];
                return true;
            }
        }
        skill = null;
        return false;
    }
    private void BuildSkillIDs()
    {
        var uniqueSkills = new HashSet<Skill>(m_SkillPrototypes);

        m_AllSkills = new List<Skill>(uniqueSkills.Count);

        int i = 0;
        foreach (Skill skill in uniqueSkills)
        {
            skill.SkillID = new SkillID { ID = i };
            m_AllSkills.Add(skill);
            i++;
        }
    }
    public bool TryGetWeaponPrototypeByID(WeaponID index, out WeaponData weapon)
    {
        for (int i = 0; i < m_AllSkills.Count; i++)
        {
            if (m_AllWeapons[i].Id == index)
            {
                weapon = m_AllWeapons[i];
                return true;
            }
        }
        weapon = null;
        return false;
    }
    public WeaponData GetWeaponPrototypeByID(WeaponID weaponId)
    {
        return m_AllWeapons[weaponId.ID];
    }
    public List<WeaponData> GetAllWeapon()
    {
        return m_AllWeapons;
    }
    public List<WeaponData> GetAllWeaponDataExcept(WeaponID weaponID)
    {
        List<WeaponData> weapons = new List<WeaponData>();
        foreach(var weapon in m_AllWeapons)
        {
            if (weapon.Id!=weaponID)
            {
                weapons.Add(weapon);
            }
        }
        return weapons;
    }
    private void BuildWeaponIDs()
    {
        var weaponHashes = new HashSet<WeaponData>(m_weaponDatas);

        m_AllWeapons = new List<WeaponData>(weaponHashes.Count);

        int i = 0;
        foreach (WeaponData weapon in weaponHashes)
        {
            weapon.Id = new  WeaponID { ID = i };
            m_AllWeapons.Add(weapon);
            i++;
        }
    }
}
