using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public static class SkillFactory
{
    private static Dictionary<SkillID, ObjectPool<Skill>> SkillPools = new Dictionary<SkillID, ObjectPool<Skill>>();

    private static ObjectPool<Skill> GetSkillPool(SkillID skillID)
    {
        if (!SkillPools.TryGetValue(skillID, out var skillPool))
        {
            skillPool = new ObjectPool<Skill>
                (
                    createFunc: () => Object.Instantiate(GamePlayDataSource.Instance.GetSkillPrototypeByID(skillID)),
                    actionOnRelease: skill => skill.Reset(),
                    actionOnDestroy: Object.Destroy
                );
            SkillPools.Add(skillID, skillPool );
        }
        return skillPool;
    }
    public static Skill CreateSkillFromData(ref SkillRequestData data)
    {
        var skill = GetSkillPool(data.SkillID).Get();
        skill.Initialize(ref data);
        return skill;
    }
    public static void ReturnSkill(Skill skill)
    {
        var pool = GetSkillPool(skill.SkillID);
        pool.Release(skill);
    }
    public static void PurgePoolSkills()
    {
        foreach (var skill in SkillPools.Values)
            skill.Clear();
    }
}
