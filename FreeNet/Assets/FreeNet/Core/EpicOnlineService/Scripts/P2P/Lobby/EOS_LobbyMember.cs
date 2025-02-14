using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using UnityEngine;

public class EOS_LobbyMember
{
    public ProductUserId _localPUID { get; private set; }
    public LobbyMemberStatus _state { get; private set; }

    public Dictionary<string, Epic.OnlineServices.Lobby.Attribute> _attribute;
    public void SetState(LobbyMemberStatus state)
    {
        _state = state;
    }
    public EOS_LobbyMember(ProductUserId localPUID)
    {
        _localPUID = localPUID;
        _state = LobbyMemberStatus.Joined;
        _attribute = new Dictionary<string, Attribute>();
    }
}
