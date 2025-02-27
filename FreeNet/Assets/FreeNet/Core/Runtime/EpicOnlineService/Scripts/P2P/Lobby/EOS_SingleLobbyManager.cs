using Epic.OnlineServices;
using Epic.OnlineServices.Lobby;
using System;
using System.Collections.Generic;
using static EOSWrapper;

/*
 * NGO는 하나의 네트워크 매니저를 두어야 하기 때문에  
 * 단일 로비를 유지해야하며 또한 호스트 마이그레이션 지원 안됨으로 아래와 같은 구조로 게임 진행
 * [로비 검색 -> Active 로비 -> Session -> Active 로비]
 */
public class EOS_SingleLobbyManager : SingletonMonoBehaviour<EOS_SingleLobbyManager>
{
    public enum LobbySecurityType
    {
        Public,
        Protected,
    }
    EOS_Lobby _currentLobby;
    EOS_Core _eosCore;
    EOS_LocalUser _localUser;
    private void Awake()
    {
        SingletonSpawn(this);
    }
    public class LobbySearchResult
    {
        public LobbyDetailsInfo _info;
        EOS_SingleLobbyManager _lobbyManager;
        public Dictionary<string, Epic.OnlineServices.Lobby.Attribute> _attribute;
        LobbyDetails _details;
        public LobbySearchResult(EOS_SingleLobbyManager lobbyManager, LobbyDetails details)
        {
            _lobbyManager = lobbyManager;
            _details = details;
             _attribute = new Dictionary<string, Epic.OnlineServices.Lobby.Attribute>();

            uint attrCount = EOSWrapper.LobbyControl.GetLobbyAttributeCount(details);
            for (uint k = 0; k < attrCount; k++)
            {
                if (ETC.ErrControl(EOSWrapper.LobbyControl.GetLobbyAttributeByIndex(details, k, out var attr)))
                {
                    _attribute.Add(attr.Value.Data.Value.Key, attr.Value);
                }
            }
            if (ETC.ErrControl(EOSWrapper.LobbyControl.GetLobbyDetailsInfo(_details, out var info)))
            {
                _info = info.Value;
            }
        }
        public void Release()
        {
            if(_details != null)
            {
                _details.Release();
            }
        }
        public void JoinLobby(Action<Result,EOS_Lobby> onComplete = null)
        {
            if(_details == null)
            {
                onComplete?.Invoke(Result.InvalidParameters,null);
                return;
            }
            else 
            {
               _lobbyManager.JoinLobby(_details, onComplete);
            }
            _details = null;
        }
    }
    public override void OnRelease()
    {
        RemoveLobbyCallback();
    }
    public void Init(EOS_Core eosCore, EOS_LocalUser localUser)
    {
        _eosCore = eosCore;
        _localUser = localUser;
        _currentLobby = null;
        AddLobbyCallback();
        SingletonInitialize();
    }
    #region EOScallbacks
    ulong _onLobbyUpdateReceivedHandle;
    ulong _onLobbyMemberUpdateReceivedHandle;
    ulong _onLobbyMemberStatusReceivedHandle;
    ulong _onLeaveLobbyRequestedHandle;
    ulong _onLobbyInviteReceivedHandle;
    ulong _onLobbyInviteAcceptedHandle;
    ulong _onLobbyInviteRejectedHandle;
    ulong _onJoinLobbyAcceptedHandle;
    #endregion
    #region callbacks
    public event Action<Epic.OnlineServices.Lobby.Attribute> _onLobbyAttributeUpdate;
    public event Action<EOS_LobbyMember, Epic.OnlineServices.Lobby.Attribute> _onMemberAttributeUpdate;
    public event Action<EOS_LobbyMember> _onMemberUpdate;
    public event Action<EOS_LobbyMember> _onMemberStateUpdate;
    public event Action<LobbyInviteRejectedCallbackInfo> _onInviteRejected;
    public event Action<LobbyInviteReceivedCallbackInfo> _onInviteReceived;
    #endregion
    public void CreateLobby(uint maxLobbyMember, string lobbyType, LobbySecurityType securityType, string lobbyInfo , Action<Result,EOS_Lobby> onComplete)
    {
        Epic.OnlineServices.Lobby.AttributeData[] searchParams = new Epic.OnlineServices.Lobby.AttributeData[4]
        { 
            new AttributeData{ Key = "LOBBYTYPE", Value = lobbyType},
            new AttributeData{ Key = "LOBBYSECURITY", Value = securityType.ToString()},
            new AttributeData { Key = "LOBBYCODE", Value = GenerateLobbyCode()},
            new AttributeData { Key = "LOBBYINFO", Value = lobbyInfo}
        };
        var options = new CreateLobbyOptions()
        {
            LocalUserId = _localUser._localPUID._PUID,
            MaxLobbyMembers = maxLobbyMember,
            PermissionLevel = LobbyPermissionLevel.Publicadvertised,
            BucketId = securityType.ToString(),
            PresenceEnabled = true,
            AllowInvites = true,
            DisableHostMigration = true,
            EnableRTCRoom = false, // 음성 채팅 
            EnableJoinById = true,
        };
        EOSWrapper.LobbyControl.CreateLobby(_eosCore._ILobby, options, (ref CreateLobbyCallbackInfo info) =>
        {
            if(EOSWrapper.ETC.ErrControl<EOS_Lobby>(info.ResultCode, onComplete))
            {
                if (ETC.ErrControl<EOS_Lobby>(EOSWrapper.LobbyControl.GetLobbyModification(_eosCore._ILobby, info.LobbyId, _localUser._localPUID._PUID, out var modification), onComplete))
                {
                    foreach (var item in searchParams)
                    {
                        if(ETC.ErrControl<EOS_Lobby>(EOSWrapper.LobbyControl.SetModificationAddLobbyAttribute(modification, new Epic.OnlineServices.Lobby.Attribute()
                        {
                            Visibility = LobbyAttributeVisibility.Public,
                            Data = item
                        }), onComplete))
                        {
                            continue;
                        }
                    }
                    EOSWrapper.LobbyControl.UpdateLobby(_eosCore._ILobby, modification, (ref UpdateLobbyCallbackInfo info) =>
                    {
                        if(ETC.ErrControl<EOS_Lobby>(info.ResultCode, onComplete))
                        {
                            var lobby = CreateJoinedLobby(info.LobbyId);
                            modification.Release();
                            onComplete?.Invoke(info.ResultCode, lobby);
                        }
                    });
                }
            }
        });
    }
    public void FindLobbyByCode(uint findNum,string lobbyCode, Action<Result, List<LobbySearchResult>> onComplete = null)
    {
        List<Epic.OnlineServices.Lobby.AttributeData> searchParams = new List<AttributeData>();
        searchParams.Add(new AttributeData { Key = "LOBBYCODE", Value = lobbyCode });
        FindLobby(findNum, searchParams, onComplete);
    }
    public void FindPublicLobby(uint findNum, string lobbyType, string lobbyCode = null, string lobbyInfo = null, Action<Result, List<LobbySearchResult>> onComplete = null)
    {
        List<Epic.OnlineServices.Lobby.AttributeData> searchParams = new List<AttributeData>();
        searchParams.Add(new AttributeData { Key = "LOBBYSECURITY", Value = LobbySecurityType.Public.ToString()});
        if (lobbyCode != null)
        {
            searchParams.Add(new AttributeData { Key = "LOBBYCODE", Value = lobbyCode });
            searchParams.Add(new AttributeData { Key = "LOBBYTYPE", Value = lobbyType });
        }

        FindLobby(findNum, searchParams,onComplete);
    }
    void FindLobby(uint findNum, List<Epic.OnlineServices.Lobby.AttributeData> searchParams, Action<Result, List<LobbySearchResult>> onComplete= null)
    {
        if (EOSWrapper.ETC.ErrControl(EOSWrapper.LobbyControl.GetLobbySearch(_eosCore._ILobby, findNum, out var search), onComplete))
        {
            foreach (var item in searchParams)
            {
                if (!ETC.ErrControl(EOSWrapper.LobbyControl.SetSearchParamAttribute(search, item, ComparisonOp.Equal), onComplete))
                {
                    return;
                }
            }
            EOSWrapper.LobbyControl.SearchLobby(search, _localUser._localPUID._PUID, (ref LobbySearchFindCallbackInfo info) =>
            {
                if (ETC.ErrControl(info.ResultCode, onComplete))
                {
                    uint count = EOSWrapper.LobbyControl.GetSearchResultCount(search);
                    var findlobbies = new List<LobbySearchResult>();
                    List<Epic.OnlineServices.Lobby.AttributeData> attrList = new List<AttributeData>();
                    for (uint i = 0; i < count; i++)
                    {
                        if (ETC.ErrControl(EOSWrapper.LobbyControl.CopySearchResultByIndex(search, i, out var details),onComplete))
                        {
                            var lobby = new LobbySearchResult(this, details);
                            findlobbies.Add(lobby);
                        }
                    }
                    onComplete?.Invoke(Result.Success,findlobbies);
                }
                search.Release();
            });
        }
    }
    public void JoinLobby(string lobbyID, Action<Result,EOS_Lobby> onComplete = null)
    {
        if (IsJoined(lobbyID))
        {
            onComplete?.Invoke(Result.LobbyLobbyAlreadyExists, _currentLobby);
            return;
        }
        LeaveLobby();
        EOSWrapper.LobbyControl.JoinLobbyById(_eosCore._ILobby, true, lobbyID, _localUser._localPUID._PUID, (ref JoinLobbyByIdCallbackInfo info) =>
        {
            if (EOSWrapper.ETC.ErrControl(info.ResultCode, onComplete))
            {
                var lobby = CreateJoinedLobby(info.LobbyId);
                onComplete?.Invoke(info.ResultCode, lobby);
            }
        });
    }
    public void JoinLobby(LobbyDetails details, Action<Result,EOS_Lobby> onComplete = null)
    {
        if (IsJoined(details))
        {
            onComplete?.Invoke(Result.LobbyLobbyAlreadyExists, _currentLobby);
            return;
        }
        LeaveLobby();
        EOSWrapper.LobbyControl.JoinLobbyByDetails(_eosCore._ILobby, true, details, _localUser._localPUID._PUID, (ref JoinLobbyCallbackInfo info) =>
        {
            if (EOSWrapper.ETC.ErrControl(info.ResultCode, onComplete))
            {
                var lobby = CreateJoinedLobby(info.LobbyId);
                details.Release();
                onComplete?.Invoke(info.ResultCode, lobby);
            }
        });

    }
    public void LeaveLobby(Action<Result, EOS_Lobby> onComplete = null)
    {
        if (_currentLobby == null)
        {
            onComplete?.Invoke(Result.Success, null);
        }
        else
        {
            EOS_Lobby lobby = _currentLobby;
            _currentLobby = null;
            EOSWrapper.LobbyControl.LeaveLobby(_eosCore._ILobby, lobby._lobbyID, lobby._localPUID._PUID, (ref LeaveLobbyCallbackInfo info) =>
            {
                if (ETC.ErrControl(info.ResultCode, onComplete))
                {
                    lobby.SetJoined(false);
                    onComplete?.Invoke(info.ResultCode, lobby);
                }
            });
        }
    }
    public void RemoveLobbyCallback()
    {
        _eosCore._ILobby.RemoveNotifyLobbyUpdateReceived(_onLobbyUpdateReceivedHandle);
        _eosCore._ILobby.RemoveNotifyLobbyMemberUpdateReceived(_onLobbyMemberUpdateReceivedHandle);
        _eosCore._ILobby.RemoveNotifyLobbyMemberStatusReceived(_onLobbyMemberStatusReceivedHandle);
        _eosCore._ILobby.RemoveNotifyLeaveLobbyRequested(_onLeaveLobbyRequestedHandle);
        _eosCore._ILobby.RemoveNotifyLobbyInviteReceived(_onLobbyInviteReceivedHandle);
        _eosCore._ILobby.RemoveNotifyLobbyInviteAccepted(_onLobbyInviteAcceptedHandle);
        _eosCore._ILobby.RemoveNotifyLobbyInviteRejected(_onLobbyInviteRejectedHandle);
        _eosCore._ILobby.RemoveNotifyJoinLobbyAccepted(_onJoinLobbyAcceptedHandle);
    }
    public void AddLobbyCallback()
    {
        _onLobbyUpdateReceivedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyUpdateReceived(_eosCore._ILobby, OnLobbyUpdateReceived);
        _onLobbyMemberUpdateReceivedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyMemberUpdateReceived(_eosCore._ILobby, OnLobbyMemberUpdateReceived);
        _onLobbyMemberStatusReceivedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyMemberStatusReceived(_eosCore._ILobby, OnLobbyMemberStatusReceived);
        _onLeaveLobbyRequestedHandle = EOSWrapper.LobbyControl.AddCBNotifyLeaveLobbyRequested(_eosCore._ILobby, OnLeaveLobbyRequested);
        _onLobbyInviteReceivedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyInviteReceived(_eosCore._ILobby, OnLobbyInviteReceived);
        _onLobbyInviteAcceptedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyInviteAccepted(_eosCore._ILobby, OnLobbyInviteAccepted);
        _onLobbyInviteRejectedHandle = EOSWrapper.LobbyControl.AddCBNotifyLobbyInviteRejected(_eosCore._ILobby, OnLobbyInviteRejected);
        _onJoinLobbyAcceptedHandle = EOSWrapper.LobbyControl.AddCBNotifyJoinLobbyAccepted(_eosCore._ILobby, OnJoinLobbyAccepted);
    }
    void OnLobbyUpdateReceived(ref LobbyUpdateReceivedCallbackInfo info)
    {
        if (_currentLobby == null) return;
        if(_currentLobby._lobbyID == info.LobbyId)
        {
            _currentLobby.OnLobbyUpdateReceived(info);
        }
    }
    void OnLobbyMemberUpdateReceived(ref LobbyMemberUpdateReceivedCallbackInfo info)
    {
        if (_currentLobby == null) return;
        if (_currentLobby._lobbyID == info.LobbyId)
        {
            _currentLobby.OnLobbyMemberUpdateReceived(info);
        }
    }
    void OnLobbyMemberStatusReceived(ref LobbyMemberStatusReceivedCallbackInfo info)
    {
        if (_currentLobby == null) return;
        if (_currentLobby._lobbyID == info.LobbyId)
        {
            _currentLobby.OnLobbyMemberStatusReceived(info);
            if (info.CurrentStatus == LobbyMemberStatus.Left)
            {
                if (info.TargetUserId.ToString() == _currentLobby._lobbyOwner.ToString())
                {
                    _currentLobby = null;
                }
            }
        }

    }
    void OnLeaveLobbyRequested(ref LeaveLobbyRequestedCallbackInfo info)
    {
        LeaveLobby();
    }
    void OnLobbyInviteAccepted(ref LobbyInviteAcceptedCallbackInfo info)
    {
        if (EOSWrapper.LobbyControl.CopyLobbyDetailsByInviteID(_eosCore._ILobby, info.InviteId, out var details))
        {
            JoinLobby(details);
        }
    }
    void OnLobbyInviteRejected(ref LobbyInviteRejectedCallbackInfo info)
    {
        _onInviteRejected?.Invoke(info);
    }
    void OnLobbyInviteReceived(ref LobbyInviteReceivedCallbackInfo info)
    {
        _onInviteReceived?.Invoke(info);
    }
    void OnJoinLobbyAccepted(ref JoinLobbyAcceptedCallbackInfo info)
    {
        if (EOSWrapper.LobbyControl.CopyLobbyDetailsByUI(_eosCore._ILobby, info.UiEventId, out var details))
        {
            JoinLobby(details);
        }
    }
    bool IsJoined(string lobbyID)
    {
        if (IsJoined())
        {
            if (lobbyID == _currentLobby._lobbyID)
            {
                return true;
            }
        }
        return false;
    }
    bool IsJoined()
    {
        return _currentLobby != null;
    }
    bool IsJoined(LobbyDetails details)
    {
        if (IsJoined())
        {
            if (ETC.ErrControl(EOSWrapper.LobbyControl.GetLobbyDetailsInfo(details, out var detailsInfo)))
            {
                if (detailsInfo.Value.LobbyId == _currentLobby._lobbyID)
                {
                    return true;
                }
            }
        }
        return false;
    }
    public string GenerateLobbyCode()
    {
        /*
         * 공개방 검색에 노출을 줄이고 싶은 경우 로비 코드로 검색하게 해두었음 
         * 로비 코드는 중복될 가능성이 있음
         * 가장 좋은건 알아서 할당해 주는 고유 LobbyID 를 쓰는건데 쓰기에 너무 길다
         */
        string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string digits = "0123456789";
        string code = "";
        for (int i = 0; i < 5; i++)
        {
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                code += digits[UnityEngine.Random.Range(0, digits.Length)];
            }
            else
            {
                code += letters[UnityEngine.Random.Range(0, letters.Length)];
            }
        }
        return code;
    }
    private EOS_Lobby CreateJoinedLobby(string lobbyID)
    {
        _currentLobby = new EOS_Lobby(lobbyID, _localUser._localPUID);
        return _currentLobby;
    }
    public class EOS_Lobby
    {
        EOS_Core _eosNet;
        EOS_SingleLobbyManager _singleLobbyManager;
        public bool _joined { get; private set; }
        public string _lobbyID { get; private set; }
        public EOSWrapper.ETC.PUID _localPUID { get; private set; }
        public EOSWrapper.ETC.PUID _lobbyOwner { get; private set; }
        public Dictionary<string, Epic.OnlineServices.Lobby.Attribute> _attribute;
        public Dictionary<EOSWrapper.ETC.PUID, EOS_LobbyMember> _members;
        public EOS_Lobby(string lobbyID, EOSWrapper.ETC.PUID localPUID)
        {
            _lobbyID = lobbyID;
            _localPUID = localPUID;
            _members = new Dictionary<EOSWrapper.ETC.PUID, EOS_LobbyMember>();
            _attribute = new Dictionary<string, Epic.OnlineServices.Lobby.Attribute>();
            _eosNet = SingletonMonoBehaviour<EOS_Core>._instance;
            _singleLobbyManager = SingletonMonoBehaviour<EOS_SingleLobbyManager>._instance;
            _joined = true;
            if (ETC.ErrControl(EOSWrapper.LobbyControl.CopyLobbyDetailsByLobbyID(_eosNet._ILobby, _lobbyID, _localPUID._PUID, out var details)))
            {
                UpdateLobbyOwner(details);
                UpdateLobbyAttribute(details);
                UpdateMembers(details);
                details.Release();
            }
        }
        public void SetJoined(bool joined)
        {
            _joined = joined;
        }
        public bool GetLobbyInfo(out string info)
        {
            info = null;
            if (_attribute.TryGetValue("LOBBYINFO", out Epic.OnlineServices.Lobby.Attribute attr))
            {
                info = attr.Data.Value.Value.AsUtf8;
                return true;
            }
            return false;
        }
        public bool GetLobbySecurity(out LobbySecurityType type)
        {
            type = LobbySecurityType.Public;
            if (_attribute.TryGetValue("LOBBYSECURITY", out Epic.OnlineServices.Lobby.Attribute attr))
            {
                string typeAsString = attr.Data.Value.Value.AsUtf8;
                type = (LobbySecurityType)Enum.Parse(typeof(LobbySecurityType), typeAsString);
                return true;
            }
            return false;
        }
        public bool GetLobbyType(out string type)
        {
            type = null;
            if (_attribute.TryGetValue("LOBBYTYPE", out Epic.OnlineServices.Lobby.Attribute attr))
            {
                type = attr.Data.Value.Value.AsUtf8;
                return true;
            }
            return false;
        }
        public bool GetLobbyCode(out string code)
        {
            code = null;
            if (_attribute.TryGetValue("LOBBYCODE", out Epic.OnlineServices.Lobby.Attribute attr))
            {
                code = attr.Data.Value.Value.AsUtf8;
                return true; 
            }
            return false;
        }   
        public void UpdateMembers(LobbyDetails details)
        {
            uint memberCount = EOSWrapper.LobbyControl.GetCurrentMemberCount(details);
            Dictionary<EOSWrapper.ETC.PUID, EOS_LobbyMember> newMembers = new Dictionary<EOSWrapper.ETC.PUID, EOS_LobbyMember>();
            for (uint i = 0; i < memberCount; i++)
            {
                if (EOSWrapper.LobbyControl.GetMemberByIndex(details, i, out var puid))
                {
                    var memberPUID = new EOSWrapper.ETC.PUID(puid);

                    if (_members.TryGetValue(memberPUID, out var member))
                    {
                        newMembers.Add(memberPUID, member);
                        _members.Remove(memberPUID);
                    }
                    else
                    {
                        member = new EOS_LobbyMember(memberPUID);
                        newMembers.TryAdd(memberPUID, member);
                        _singleLobbyManager._onMemberUpdate?.Invoke(member);
                    }
                    UpdateMembersAttribute(details, memberPUID);
                }
            }
            _members = newMembers;
        }
        public void UpdateLobbyOwner(LobbyDetails details)
        {
            if (EOSWrapper.LobbyControl.GetLobbyOwner(details, out var owner))
            {
                if (_lobbyOwner?.ToString() != owner.ToString())
                {
                    _lobbyOwner = new EOSWrapper.ETC.PUID(owner);
                }
            }
        }
        public void UpdateLobbyAttribute(LobbyDetails details)
        {
            uint attrCount = EOSWrapper.LobbyControl.GetLobbyAttributeCount(details);
            _attribute.Clear();
            for (uint i = 0; i < attrCount; i++)
            {
                if (ETC.ErrControl(EOSWrapper.LobbyControl.GetLobbyAttributeByIndex(details, i, out var attr)))
                {

                    if (!_attribute.TryGetValue(attr.Value.Data.Value.Key, out var _))
                    {
                        _attribute.TryAdd(attr.Value.Data.Value.Key, attr.Value);
                        _singleLobbyManager._onLobbyAttributeUpdate?.Invoke(attr.Value);
                    }
                    else
                    {
                        if (EOSWrapper.ETC.Equal(_attribute[attr.Value.Data.Value.Key], attr.Value))
                        {
                            _attribute[attr.Value.Data.Value.Key] = attr.Value;
                            _singleLobbyManager._onLobbyAttributeUpdate?.Invoke(attr.Value);
                        }
                    }
                }
            }
        }
        public void UpdateMembersAttribute(LobbyDetails details, EOSWrapper.ETC.PUID memberPUID)
        {
            if (_members.TryGetValue(memberPUID, out var member))
            {
                uint attrCount = EOSWrapper.LobbyControl.GetMemberAttributeCount(details, memberPUID._PUID);
                member._attribute.Clear();
                for (uint i = 0; i < attrCount; i++)
                {
                    if (EOSWrapper.LobbyControl.GetMemberAttributeByIndex(details, memberPUID._PUID, i, out var attr))
                    {
                        if (!member._attribute.TryGetValue(attr.Value.Data.Value.Key, out var _))
                        {
                            member._attribute.TryAdd(attr.Value.Data.Value.Key, attr.Value);
                            _singleLobbyManager._onMemberAttributeUpdate?.Invoke(member, attr.Value);
                        }
                        else
                        {
                            if (EOSWrapper.ETC.Equal(member._attribute[attr.Value.Data.Value.Key], attr.Value))
                            {
                                member._attribute[attr.Value.Data.Value.Key] = attr.Value;
                                _singleLobbyManager._onMemberAttributeUpdate?.Invoke(member, attr.Value);
                            }
                        }
                    }
                }
            }
        }
        public void UpdateMemberState(EOSWrapper.ETC.PUID memberPUID, LobbyMemberStatus state)
        {
            if (_members.TryGetValue(memberPUID, out var member))
            {
                if (member._state != state)
                {
                    member.SetState(state);
                    _singleLobbyManager._onMemberStateUpdate?.Invoke(member);
                }
                if (member._state == LobbyMemberStatus.Left)
                {
                    _members.Remove(member._localPUID);
                }
            }
            else if (state == LobbyMemberStatus.Joined)
            {
                member = new EOS_LobbyMember(memberPUID);
                _members.TryAdd(memberPUID, member);
                _singleLobbyManager._onMemberStateUpdate?.Invoke(member);
            }
        }
        public void OnLobbyUpdateReceived(LobbyUpdateReceivedCallbackInfo info)
        {
            if (ETC.ErrControl(EOSWrapper.LobbyControl.CopyLobbyDetailsByLobbyID(_eosNet._ILobby, _lobbyID, _localPUID._PUID, out var details)))
            {
                UpdateLobbyAttribute(details);
                details.Release();
            }
        }
        public void OnLobbyMemberUpdateReceived(LobbyMemberUpdateReceivedCallbackInfo info)
        {
            if (ETC.ErrControl(EOSWrapper.LobbyControl.CopyLobbyDetailsByLobbyID(_eosNet._ILobby, _lobbyID, _localPUID._PUID, out var details)))
            {
                var puid = new EOSWrapper.ETC.PUID(info.TargetUserId);
                UpdateMembersAttribute(details, puid);
                details.Release();
            }
        }
        public void OnLobbyMemberStatusReceived(LobbyMemberStatusReceivedCallbackInfo info)
        {
            var puid = new EOSWrapper.ETC.PUID(info.TargetUserId);
            UpdateMemberState(puid, info.CurrentStatus);
        }
    }
}