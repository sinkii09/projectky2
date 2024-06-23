using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class NetworkAvatarGuidState : NetworkBehaviour
{
    [FormerlySerializedAs("AvatarGuidArray")]
    [HideInInspector]
    public NetworkVariable<NetworkGuid> AvatarGuid = new NetworkVariable<NetworkGuid>();

    [SerializeField]
    AvatarRegistry m_AvatarRegistry;

    PlayerAvatar m_Avatar;
    public PlayerAvatar RegisteredAvatar
    {
        get
        {
            if (m_Avatar == null)
            {
                RegisterAvatar(AvatarGuid.Value.ToGuid());
            }

            return m_Avatar;
        }
    }

    public void SetRandomAvatar()
    {
        AvatarGuid.Value = m_AvatarRegistry.GetRandomAvatar().Guid.ToNetworkGuid();
    }

    void RegisterAvatar(Guid guid)
    {
        if (guid.Equals(Guid.Empty))
        {
            // not a valid Guid
            return;
        }

        // based on the Guid received, Avatar is fetched from AvatarRegistry
        if (!m_AvatarRegistry.TryGetAvatar(guid, out var avatar))
        {
            Debug.LogError("Avatar not found!");
            return;
        }

        if (m_Avatar != null)
        {
            // already set, this is an idempotent call, we don't want to Instantiate twice
            return;
        }

        m_Avatar = avatar;

        //if (TryGetComponent<ServerCharacter>(out var serverCharacter))
        //{
        //    serverCharacter.CharacterClass = avatar.CharacterClass;
        //}
    }
}
