using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu]
public class AvatarRegistry : ScriptableObject
{
    [SerializeField]
    PlayerAvatar[] m_Avatars;

    public PlayerAvatar[] Avatars { get {  return m_Avatars; } }
    public bool TryGetAvatar(Guid guid, out PlayerAvatar avatarValue)
    {
        avatarValue = Array.Find(m_Avatars, avatar => avatar.Guid == guid);

        return avatarValue != null;
    }

    public PlayerAvatar GetRandomAvatar()
    {
        if (m_Avatars == null || m_Avatars.Length == 0)
        {
            return null;
        }

        return m_Avatars[UnityEngine.Random.Range(0, m_Avatars.Length)];
    }
}
