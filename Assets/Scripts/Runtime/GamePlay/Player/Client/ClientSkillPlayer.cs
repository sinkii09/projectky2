using System;
using System.Collections.Generic;

internal class ClientSkillPlayer
{
    private const float k_AnticipationTimeoutSeconds = 1;

    private List<Skill> m_PlayingSkills = new List<Skill>();

    public ClientCharacter ClientCharacter { get; private set; }

    public ClientSkillPlayer(ClientCharacter clientCharacter)
    { ClientCharacter = clientCharacter; }
    public void OnUpdate()
    {
        for (int i = m_PlayingSkills.Count - 1; i >= 0; --i)
        {
            var skill = m_PlayingSkills[i];
            bool keepGoing = skill.AnticipatedClient || skill.OnUpdateClient(ClientCharacter);
            bool expirable = skill.Config.DurationSeconds > 0f;
            bool timeExipred = expirable && skill.TimeRunning >= skill.Config.DurationSeconds;
            bool timeOut = skill.AnticipatedClient && skill.TimeRunning >= k_AnticipationTimeoutSeconds;
            if (!keepGoing || timeExipred || timeOut)
            {
                if (timeOut)
                {
                    skill.CancelClient(ClientCharacter);
                }
                else
                {
                    skill.EndClient(ClientCharacter);
                }

                m_PlayingSkills.RemoveAt(i);
                SkillFactory.ReturnSkill(skill);
            }
        }
    }
    private int FindSkill(SkillID skillID, bool anticipatedOnly)
    {
        return m_PlayingSkills.FindIndex(a => a.SkillID == skillID && (!anticipatedOnly || a.AnticipatedClient));
    }
    internal void OnAnimEvent(string id)
    {
        foreach (var skillFX in m_PlayingSkills)
        {
            skillFX.OnAnimEventClient(ClientCharacter, id);
        }
    }
    public void AnticipateSkill(ref SkillRequestData data)
    {
        if (!ClientCharacter.IsAnimating() && Skill.ShouldClientAnticipate(ClientCharacter, ref data))
        {
            var skillFX = SkillFactory.CreateSkillFromData(ref data);
            skillFX.AnticipateActionClient(ClientCharacter);
            m_PlayingSkills.Add(skillFX);
        }
    }

    public void PlaySKill(ref SkillRequestData data)
    {
        var anticipatedSkillIndex = FindSkill(data.SkillID, true);
        var skillFX = anticipatedSkillIndex >= 0 ? m_PlayingSkills[anticipatedSkillIndex] : SkillFactory.CreateSkillFromData(ref data);
        if (skillFX.OnStartClient(ClientCharacter))
        {
            if (anticipatedSkillIndex < 0)
            {
                m_PlayingSkills.Add(skillFX);
            }
        }
        else if (anticipatedSkillIndex >= 0)
        {
            var removedSkill = m_PlayingSkills[anticipatedSkillIndex];
            m_PlayingSkills.RemoveAt(anticipatedSkillIndex);
            SkillFactory.ReturnSkill(removedSkill);
        }
    }
    public void CancelAllSkills()
    {
        foreach (var skill in m_PlayingSkills)
        {
            skill.CancelClient(ClientCharacter);
            SkillFactory.ReturnSkill(skill);
        }
        m_PlayingSkills.Clear();
    }
    public void CancelAllSKillWithSamePrototypeID(SkillID skillID)
    {
        for(int i = m_PlayingSkills.Count -1; i >=0;--i)
        {
            if (m_PlayingSkills[i].SkillID == skillID)
            {
                var skill = m_PlayingSkills[i];
                skill.CancelClient(ClientCharacter);
                m_PlayingSkills.RemoveAt(i);
                SkillFactory.ReturnSkill(skill);
            }
        }
    }
}