using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;

public class EOS_LobbyMember
{
    public EOSWrapper.ETC.PUID _localPUID { get; private set; }
    public LobbyMemberStatus _state { get; private set; }

    public Dictionary<string, Epic.OnlineServices.Lobby.Attribute> _attribute;
    public void SetState(LobbyMemberStatus state)
    {
        _state = state;
    }
    public EOS_LobbyMember(EOSWrapper.ETC.PUID localPUID)
    {
        _localPUID = localPUID;
        _state = LobbyMemberStatus.Joined;
        _attribute = new Dictionary<string, Attribute>();
    }
}
