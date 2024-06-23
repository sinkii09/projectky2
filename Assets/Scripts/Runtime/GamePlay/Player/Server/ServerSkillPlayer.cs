using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ServerSkillPlayer
{
    private const float k_MaxQueueTimeDepth = 1.6f;


    private ServerCharacter m_ServerCharacter;

    private CharacterMovement m_CharacterMovement;

    private List<Skill> m_Queue;

    private List<Skill> m_NonBlockingSkills;

    private SkillRequestData m_SkillRequestData;

    private bool m_HasPendingSynthesizedAction;

    private SkillRequestData m_PendingSynthesizedAction = new SkillRequestData();

    private Dictionary<SkillID, float> m_LastUsedTimestamps;

    public ServerSkillPlayer(ServerCharacter serverCharacter, CharacterMovement characterMovement)
    {
        m_ServerCharacter = serverCharacter;
        m_CharacterMovement = characterMovement;
        m_Queue = new List<Skill>();
        m_NonBlockingSkills = new List<Skill>();
        m_LastUsedTimestamps = new Dictionary<SkillID, float>();  
    }

    public void PlaySkill(ref SkillRequestData skillRequestData)
    {
        if(!skillRequestData.ShouldQueue && m_Queue.Count > 0 && (m_Queue[0].Config.ActionInterruptible || m_Queue[0].Config.CanBeInterruptedBy(skillRequestData.SkillID)))
        {
            ClearActions(false); 
        }
        if (GetQueueTimeDepth() >= k_MaxQueueTimeDepth)
        {
            return;
        }
        var newAction = SkillFactory.CreateSkillFromData(ref skillRequestData);
        m_Queue.Add(newAction);
        if (m_Queue.Count == 1) { StartAction(); }

    }
    private void StartAction()
    {
        if (m_Queue.Count > 0)
        {
            float reuseTime = m_Queue[0].Config.ReuseTimeSeconds;
            if (reuseTime > 0
                && m_LastUsedTimestamps.TryGetValue(m_Queue[0].SkillID, out float lastTimeUsed)
                && Time.time - lastTimeUsed < reuseTime)
            {
                AdvanceQueue(false);
                return;
            }
            m_Queue[0].TimeStarted = Time.time;
            bool play = m_Queue[0].OnStart(m_ServerCharacter);
            if (!play)
            {
                AdvanceQueue(false);
                return;
            }
            if (m_Queue[0].Config.ActionInterruptible && !m_CharacterMovement.IsPerformingForcedMovement())
            {
                m_CharacterMovement.CancelMove();
            }
            m_LastUsedTimestamps[m_Queue[0].SkillID] = Time.time;
            if (m_Queue[0].Config.ExecTimeSeconds == 0 && m_Queue[0].Config.BlockingMode == BlockingModeType.OnlyDuringExecTime)
            {
                m_NonBlockingSkills.Add(m_Queue[0]);
                AdvanceQueue(false);
                return;
            }
        }
    }
    private void AdvanceQueue(bool endRemoved)
    {
        if (m_Queue.Count > 0)
        {
            if (endRemoved)
            {
                m_Queue[0].End(m_ServerCharacter);
                if (m_Queue[0].ChainIntoNewSkill(ref m_PendingSynthesizedAction))
                {
                    m_HasPendingSynthesizedAction = true;
                }
            }
            var action = m_Queue[0];
            m_Queue.RemoveAt(0);
            TryReturnAction(action);
        }

        if (!m_HasPendingSynthesizedAction || m_PendingSynthesizedAction.ShouldQueue)
        {
            StartAction();
        }
    }
    private void TryReturnAction(Skill skill)
    {
        if (m_Queue.Contains(skill))
        {
            return;
        }

        if (m_NonBlockingSkills.Contains(skill))
        {
            return;
        }

        SkillFactory.ReturnSkill(skill);
    }
    public void OnUpdate()
    {
        if (m_HasPendingSynthesizedAction)
        {
            m_HasPendingSynthesizedAction = false;
            PlaySkill(ref m_PendingSynthesizedAction);
        }

        if (m_Queue.Count > 0 && m_Queue[0].ShouldBecomeNonBlocking())
        {
            m_NonBlockingSkills.Add(m_Queue[0]);
            AdvanceQueue(false);
        }
        if (m_Queue.Count > 0)
        {
            if (!UpdateAction(m_Queue[0]))
            {
                AdvanceQueue(true);
            }
        }

        // if there's non-blocking actions, update them! We do this in reverse-order so we can easily remove expired actions.
        for (int i = m_NonBlockingSkills.Count - 1; i >= 0; --i)
        {
            Skill runningAction = m_NonBlockingSkills[i];
            if (!UpdateAction(runningAction))
            {
                runningAction.End(m_ServerCharacter);
                m_NonBlockingSkills.RemoveAt(i);
                TryReturnAction(runningAction);
            }
        }
    }
    private bool UpdateAction(Skill skill)
    {
        bool keepGoing = skill.OnUpdate(m_ServerCharacter);
        bool expirable = skill.Config.DurationSeconds > 0f; //non-positive value is a sentinel indicating the duration is indefinite.
        var timeElapsed = Time.time - skill.TimeStarted;
        bool timeExpired = expirable && timeElapsed >= skill.Config.DurationSeconds;
        return keepGoing && !timeExpired;
    }
    public void ClearActions(bool cancelNonBlocking)
    {
        if (m_Queue.Count > 0)
        {
            m_LastUsedTimestamps.Remove(m_Queue[0].SkillID);
            m_Queue[0].Cancel(m_ServerCharacter);
        }
        {
            var removedActions = ListPool<Skill>.Get();

            foreach (var action in m_Queue)
            {
                removedActions.Add(action);
            }

            m_Queue.Clear();

            foreach (var action in removedActions)
            {
                TryReturnAction(action);
            }

            ListPool<Skill>.Release(removedActions);
        }
        if (cancelNonBlocking)
        {
            var removedActions = ListPool<Skill>.Get();

            foreach (var action in m_NonBlockingSkills)
            {
                action.Cancel(m_ServerCharacter);
                removedActions.Add(action);
            }
            m_NonBlockingSkills.Clear();

            foreach (var action in removedActions)
            {
                TryReturnAction(action);
            }

            ListPool<Skill>.Release(removedActions);
        }
    }
    public bool GetActiveSkillInfo(out SkillRequestData data)
    {
        if (m_Queue.Count > 0)
        {
            data = m_Queue[0].Data;
            return true;
        }
        else
        {
            data = new SkillRequestData();
            return false;
        }
    }
    public bool IsReuseTimeElapsed(SkillID skillID)
    {
        if (m_LastUsedTimestamps.TryGetValue(skillID, out float lastTimeUsed))
        {
            var abilityConfig = GamePlayDataSource.Instance.GetSkillPrototypeByID(skillID).Config;

            float reuseTime = abilityConfig.ReuseTimeSeconds;
            if (reuseTime > 0 && Time.time - lastTimeUsed < reuseTime)
            {
                // still needs more time!
                return false;
            }
        }
        return true;
    }
    public int RunningActionCount
    {
        get
        {
            return m_NonBlockingSkills.Count + (m_Queue.Count > 0 ? 1 : 0);
        }
    }
    private float GetQueueTimeDepth()
    {
        if (m_Queue.Count == 0) { return 0; }

        float totalTime = 0;
        foreach (var action in m_Queue)
        {
            var info = action.Config;
            float actionTime = info.BlockingMode == BlockingModeType.OnlyDuringExecTime ? info.ExecTimeSeconds :
                                info.BlockingMode == BlockingModeType.EntireDuration ? info.DurationSeconds :
                                throw new System.Exception($"Unrecognized blocking mode: {info.BlockingMode}");
            totalTime += actionTime;
        }

        return totalTime - m_Queue[0].TimeRunning;
    }
    public void CollisionEntered(Collision collision)
    {
        if (m_Queue.Count > 0)
        {
            m_Queue[0].CollisionEntered(m_ServerCharacter, collision);
        }
    }
    public float GetBuffedValue(Skill.BuffableValue buffType)
    {
        float buffedValue = Skill.GetUnbuffedValue(buffType);
        if (m_Queue.Count > 0)
        {
            m_Queue[0].BuffValue(buffType, ref buffedValue);
        }
        foreach (var action in m_NonBlockingSkills)
        {
            action.BuffValue(buffType, ref buffedValue);
        }
        return buffedValue;
    }
    public virtual void OnGameplayActivity(Skill.GameplayActivity activityThatOccurred)
    {
        if (m_Queue.Count > 0)
        {
            m_Queue[0].OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
        }
        foreach (var action in m_NonBlockingSkills)
        {
            action.OnGameplayActivity(m_ServerCharacter, activityThatOccurred);
        }
    }
    public void CancelRunningActionsByLogic(SkillLogic logic, bool cancelAll, Skill exceptThis = null)
    {
        for (int i = m_NonBlockingSkills.Count - 1; i >= 0; --i)
        {
            var action = m_NonBlockingSkills[i];
            if (action.Config.Logic == logic && action != exceptThis)
            {
                action.Cancel(m_ServerCharacter);
                m_NonBlockingSkills.RemoveAt(i);
                TryReturnAction(action);
                if (!cancelAll) { return; }
            }
        }

        if (m_Queue.Count > 0)
        {
            var action = m_Queue[0];
            if (action.Config.Logic == logic && action != exceptThis)
            {
                action.Cancel(m_ServerCharacter);
                m_Queue.RemoveAt(0);
                TryReturnAction(action);
            }
        }
    }
}
