using Epic.OnlineServices.Auth;
using Epic.OnlineServices.Connect;
using Epic.OnlineServices;
using Epic.OnlineServices.Platform;
using UnityEngine;
using Epic.OnlineServices.Lobby;
using System.Collections.Generic;
using Epic.OnlineServices.Sessions;
using System;
using Epic.OnlineServices.P2P;
using System.Text.RegularExpressions;
using Epic.OnlineServices.Version;

public class EOSWrapper
{
    public class ETC
    {
        public class PUID
        {
            public string _puid { get; private set; }
            public ProductUserId _PUID { get; private set; }

            public static bool operator ==(PUID left , PUID right)
            {
                return left._puid == right._puid; 
            }
            public static bool operator !=(PUID left, PUID right)
            {
                return left._puid != right._puid;
            }
            public PUID(string localpuid)
            {
                _puid = localpuid;
                _PUID = ProductUserId.FromString(_puid);
            }

            public PUID(ProductUserId localPUID)
            {
                _PUID = localPUID;
                _puid = _PUID.ToString();
            }

            public void SetlocaPUID(string localPUID)
            {
                _puid = localPUID;
                _PUID = ProductUserId.FromString(_puid);
            }

            public override bool Equals(object obj)
            {
                return obj is PUID pUID &&
                       _puid == pUID._puid;
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(_puid);
            }
        }


        public static bool Equal(Epic.OnlineServices.Lobby.Attribute left, Epic.OnlineServices.Lobby.Attribute right)
        {
            if(left.Visibility != right.Visibility)
            {
                return false;
            }
            return Equal(left.Data.Value, right.Data.Value);
        }
        public static bool Equal(Epic.OnlineServices.Lobby.AttributeData left, Epic.OnlineServices.Lobby.AttributeData right)
        {
            if ((left.Value.ValueType != right.Value.ValueType) ||
                (left.Key != right.Key))
            {
                return false;
            }
            switch (left.Value.ValueType)
            {
                case AttributeType.String:
                    return left.Value.AsUtf8 == right.Value.AsUtf8;
                case AttributeType.Boolean:
                    return left.Value.AsBool == right.Value.AsBool;
                case AttributeType.Int64:
                    return left.Value.AsInt64 == right.Value.AsInt64;
                case AttributeType.Double:
                    return left.Value.AsDouble == right.Value.AsDouble;
            }
            return false;
        }
        public static bool ErrControl(Result result, Action<Result> onComplete = null)
        {
            if (result != Result.Success)
            {
                onComplete?.Invoke(result);
                return false;
            }
            return true;
        }
        public static bool ErrControl<T>(Result result, Action<Result, T> onComplete = null)
        {
            if (result != Result.Success)
            {
                if (typeof(T) == typeof(string) || typeof(T).IsClass)
                {
                    onComplete?.Invoke(result, (T)(object)null);
                }
                else if (typeof(T).IsValueType)
                {
                    onComplete?.Invoke(result, default(T));
                }
                return false;
            }
            return true;
        }
        public static bool SetApplicationStatus(PlatformInterface IPlatform, ApplicationStatus status)
        {
            if (IPlatform != null)
            {
                if (IPlatform.SetApplicationStatus(status) != Result.Success)
                {
                    Debug.LogError("Cant set application status");
                    return false;
                }
            }
            return true;
        }
    }
    public class LoginControl
    {
        static public void DeveloperToolLogin(Epic.OnlineServices.Auth.AuthInterface IAuth, string host, string credential, Epic.OnlineServices.Auth.OnLoginCallback onComplete = null)
        {
            var loginOptions = new Epic.OnlineServices.Auth.LoginOptions
            {
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
                Credentials = new Epic.OnlineServices.Auth.Credentials
                {
                    Type = LoginCredentialType.Developer,
                    ExternalType = ExternalCredentialType.Epic,
                    Id = host,
                    Token = credential,
                },
            };
            IAuth.Login(ref loginOptions, null, onComplete);
        }
        static public void EpicPortalLogin(Epic.OnlineServices.Auth.AuthInterface IAuth, Epic.OnlineServices.Auth.OnLoginCallback onComplete)
        {
            var loginOptions = new Epic.OnlineServices.Auth.LoginOptions
            {
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
                Credentials = new Epic.OnlineServices.Auth.Credentials
                {
                    Type = LoginCredentialType.AccountPortal,
                    ExternalType = ExternalCredentialType.Epic,
                    Id = null,
                    Token = null,
                },
            };
            IAuth.Login(ref loginOptions, null, onComplete);
        }
        static public void LoginByRefreshToken(Epic.OnlineServices.Auth.AuthInterface IAuth, EpicAccountId localEAID, Epic.OnlineServices.Auth.OnLoginCallback onComplete)
        {
            var copyUserTokenOptions = new CopyUserAuthTokenOptions();
            IAuth.CopyUserAuthToken(ref copyUserTokenOptions, localEAID, out Token? authToken);
            var loginOptions = new Epic.OnlineServices.Auth.LoginOptions
            {
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
                Credentials = new Epic.OnlineServices.Auth.Credentials
                {
                    Type = LoginCredentialType.RefreshToken,
                    ExternalType = ExternalCredentialType.Epic,
                    Id = null,
                    Token = authToken.Value.RefreshToken,
                },
            };
            IAuth.Login(ref loginOptions, null, onComplete);
        }
        static public void LogOut(Epic.OnlineServices.Auth.AuthInterface IAuth, EpicAccountId localEAID, Epic.OnlineServices.Auth.OnLogoutCallback onComplete)
        {
            Epic.OnlineServices.Auth.LogoutOptions options = new Epic.OnlineServices.Auth.LogoutOptions { LocalUserId = localEAID };
            IAuth?.Logout(ref options, null, onComplete);
        }
    }
    public class ConnectControl
    {
        #region Device Connect
        static public void CreateDeviceID(Epic.OnlineServices.Connect.ConnectInterface IConnect, OnCreateDeviceIdCallback onComplete)
        {
            var options = new CreateDeviceIdOptions()
            {
                DeviceModel = UnityEngine.Device.SystemInfo.deviceModel
            };
            IConnect.CreateDeviceId(ref options, null, onComplete);
        }
        static void DeleteDeviceID(Epic.OnlineServices.Connect.ConnectInterface IConnect, OnDeleteDeviceIdCallback onComplete)
        {
            var options = new DeleteDeviceIdOptions();
            IConnect.DeleteDeviceId(ref options, null, onComplete);
        }
        static public void DeviceIDConnect(Epic.OnlineServices.Connect.ConnectInterface IConnect, string userName, Epic.OnlineServices.Connect.OnLoginCallback onComplete = null)
        {
            var options = new Epic.OnlineServices.Connect.LoginOptions()
            {
                Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = null,
                    Type = ExternalCredentialType.DeviceidAccessToken
                },
                UserLoginInfo = new UserLoginInfo
                {
                    DisplayName = userName
                }
            };
            IConnect.Login(ref options, null, onComplete);
        }
        #endregion
        #region EpicAccount Connect
        static public void EpicIDConnect(Epic.OnlineServices.Auth.AuthInterface IAuth, Epic.OnlineServices.Connect.ConnectInterface IConnect, EpicAccountId localEAID, Epic.OnlineServices.Connect.OnLoginCallback onComplete)
        {
            var copyUserTokenOptions = new CopyUserAuthTokenOptions();
            IAuth.CopyUserAuthToken(ref copyUserTokenOptions, localEAID, out Token? authToken);
            var loginOptions = new Epic.OnlineServices.Auth.LoginOptions
            {
                ScopeFlags = AuthScopeFlags.BasicProfile | AuthScopeFlags.FriendsList | AuthScopeFlags.Presence,
                Credentials = new Epic.OnlineServices.Auth.Credentials
                {
                    Type = LoginCredentialType.RefreshToken,
                    ExternalType = ExternalCredentialType.Epic,
                    Id = null,
                    Token = authToken.Value.RefreshToken,
                },
            };
            var options = new Epic.OnlineServices.Connect.LoginOptions()
            {
                Credentials = new Epic.OnlineServices.Connect.Credentials
                {
                    Token = authToken.Value.AccessToken,
                    Type = ExternalCredentialType.Epic
                },
            };
            IConnect.Login(ref options, null, onComplete);
        }
        static public void CreateUser(Epic.OnlineServices.Connect.ConnectInterface IConnect, ContinuanceToken token, Epic.OnlineServices.Connect.OnCreateUserCallback onComplete)
        {
            var options = new CreateUserOptions();
            options.ContinuanceToken = token;
            IConnect.CreateUser(ref options, null, onComplete);
        }
        #endregion
    }
    public class LobbyControl
    {
        #region Wrapper
        static public void CreateLobby(LobbyInterface ILobby, CreateLobbyOptions Createoptions, OnCreateLobbyCallback callback = null)
        {
            ILobby.CreateLobby(ref Createoptions, null, callback);
        }
        static public void LeaveLobby(LobbyInterface ILobby,string lobbyID, ProductUserId localPUID, OnLeaveLobbyCallback callback = null)
        {
            var options = new LeaveLobbyOptions()
            {
                LobbyId = lobbyID,
                LocalUserId = localPUID
            };
            ILobby.LeaveLobby(ref options, null, callback);
        }
        static public void DestroyLobby(LobbyInterface ILobby, string lobbyID, ProductUserId localPUID, OnDestroyLobbyCallback callback = null)
        {
            DestroyLobbyOptions options = new DestroyLobbyOptions()
            {
                LobbyId = lobbyID,
                LocalUserId = localPUID
            };
            ILobby.DestroyLobby(ref options, null, callback);
        }
        static public void JoinLobbyById(LobbyInterface ILobby, bool presence, string lobbyID, ProductUserId localPUID, OnJoinLobbyByIdCallback callback = null)
        {
            var options = new JoinLobbyByIdOptions()
            {
                LocalUserId = localPUID,
                LobbyId = lobbyID,
                PresenceEnabled = presence
            };
            ILobby.JoinLobbyById(ref options, null, callback);
        }
        static public void JoinLobbyByDetails(LobbyInterface ILobby, bool presence, LobbyDetails details, ProductUserId localPUID, OnJoinLobbyCallback callback = null)
        {
            var joinOptions = new JoinLobbyOptions()
            {
                LobbyDetailsHandle = details,
                LocalUserId = localPUID,
                PresenceEnabled = presence,
            };
           ILobby.JoinLobby(ref joinOptions, null, callback);
        }
        static public uint GetInviteCount(LobbyInterface ILobby, ProductUserId localPUID)
        {
            var options = new Epic.OnlineServices.Lobby.GetInviteCountOptions()
            {
                LocalUserId = localPUID
            };
            return ILobby.GetInviteCount(ref options);
        }
        static public bool GetInviteIDByIndex(LobbyInterface ILobby, uint index, ProductUserId localPUID, out string inviteID)
        {
            inviteID = null;
            var options = new Epic.OnlineServices.Lobby.GetInviteIdByIndexOptions()
            {
                LocalUserId = localPUID,
                Index = index
            };
            Result result = ILobby.GetInviteIdByIndex(ref options, out Utf8String outinviteID);
            inviteID = outinviteID;
            return result == Result.Success;
        }
        static public void SendInvite(LobbyInterface ILobby, string lobbyID, ProductUserId targetUserId, ProductUserId localPUID, Epic.OnlineServices.Lobby.OnSendInviteCallback callback = null)
        {
            var options = new Epic.OnlineServices.Lobby.SendInviteOptions()
            {
                LobbyId = lobbyID,
                LocalUserId = localPUID,
                TargetUserId = targetUserId
            };
            ILobby.SendInvite(ref options, null, callback);
        }
        static public void RejectInvite(LobbyInterface ILobby, string inviteID, ProductUserId localPUID, Epic.OnlineServices.Lobby.OnRejectInviteCallback callback = null)
        {
            var rejectOptions = new Epic.OnlineServices.Lobby.RejectInviteOptions()
            {
                InviteId = inviteID,
                LocalUserId = localPUID
            };
            ILobby.RejectInvite(ref rejectOptions, null, callback);
        }
        static public bool CopyLobbyDetailsByUI(LobbyInterface ILobby, ulong uiEventID, out LobbyDetails lobbydetails)
        {
            lobbydetails = null;
            var options = new CopyLobbyDetailsHandleByUiEventIdOptions()
            {
                UiEventId = uiEventID
            };
            Result result = ILobby.CopyLobbyDetailsHandleByUiEventId(ref options, out var details);
            if (result == Result.Success)
            {
                lobbydetails = details;
            }
            return result == Result.Success;
        }
        static public bool CopyLobbyDetailsByInviteID(LobbyInterface ILobby, string inviteID, out LobbyDetails lobbydetails)
        {
            lobbydetails = null;
            var options = new CopyLobbyDetailsHandleByInviteIdOptions()
            {
                InviteId = inviteID
            };
            Result result = ILobby.CopyLobbyDetailsHandleByInviteId(ref options, out var details);
            if (result == Result.Success)
            {
                lobbydetails = details;
            }
            return result == Result.Success;
        }
        static public Result CopyLobbyDetailsByLobbyID(LobbyInterface ILobby, string lobbyID, ProductUserId localPUID, out LobbyDetails details)
        {
            details = null;
            var options = new CopyLobbyDetailsHandleOptions()
            {
                LobbyId = lobbyID,
                LocalUserId = localPUID
            };
            return ILobby.CopyLobbyDetailsHandle(ref options, out details);
        }
        static public uint GetCurrentMemberCount(LobbyDetails details)
        {
            var options = new LobbyDetailsGetMemberCountOptions();
            uint membercount = details.GetMemberCount(ref options);
            return membercount;
        }
        static public Result GetLobbyDetailsInfo(LobbyDetails details, out LobbyDetailsInfo? outLobbyDetailsInfo)
        {
            var options = new LobbyDetailsCopyInfoOptions();
            return details.CopyInfo(ref options, out outLobbyDetailsInfo);
        }
        static public Result GetMemberDetailsInfo(LobbyDetails details,ProductUserId targetPUID ,out LobbyDetailsMemberInfo? outMemberDetailsInfo)
        {
            var option = new LobbyDetailsCopyMemberInfoOptions()
            {
                TargetUserId = targetPUID
            };
            return details.CopyMemberInfo(ref option, out outMemberDetailsInfo);
        }
        static public bool GetMemberByIndex(LobbyDetails details,uint index,out ProductUserId meberPUID)
        {
            var MemberIndexOptions = new LobbyDetailsGetMemberByIndexOptions() { MemberIndex = index };
            meberPUID = details.GetMemberByIndex(ref MemberIndexOptions);
            return meberPUID != null;
        }
        static public bool GetLobbyOwner(LobbyDetails details,out ProductUserId ownerPUID)
        {
            var options = new LobbyDetailsGetLobbyOwnerOptions();
            ownerPUID = details.GetLobbyOwner(ref options);
            return ownerPUID!=null;
        }
        static public Epic.OnlineServices.Lobby.Attribute? GetLobbyAttribute(LobbyDetails details, string key)
        {
            var Options = new LobbyDetailsCopyAttributeByKeyOptions()
            {
                AttrKey = key
            };
            Result copyAttrResult = details.CopyAttributeByKey(ref Options, out Epic.OnlineServices.Lobby.Attribute? outAttribute);
            return outAttribute;
        }
        static public uint GetLobbyAttributeCount(LobbyDetails details)
        {
            var options = new LobbyDetailsGetAttributeCountOptions();
            return details.GetAttributeCount(ref options);
        }
        static public Result GetLobbyAttributeByIndex(LobbyDetails details, uint index, out Epic.OnlineServices.Lobby.Attribute? outAttribute)
        {
            var attrOptions = new LobbyDetailsCopyAttributeByIndexOptions()
            {
                AttrIndex = index
            };
            return details.CopyAttributeByIndex(ref attrOptions, out outAttribute);
        }
        static public Result GetLobbyAttributeByKey(LobbyDetails details, string key, out Epic.OnlineServices.Lobby.Attribute? outAttribute)
        {
            var options = new LobbyDetailsCopyAttributeByKeyOptions()
            {
                AttrKey = key
            };
            return details.CopyAttributeByKey(ref options, out outAttribute);
        }

        static public uint GetMemberAttributeCount(LobbyDetails details, ProductUserId puid)
        {
            var options = new LobbyDetailsGetMemberAttributeCountOptions() { TargetUserId = puid };
            return details.GetMemberAttributeCount(ref options);
        }
        static public bool GetMemberAttributeByIndex(LobbyDetails details, ProductUserId puid, uint index, out Epic.OnlineServices.Lobby.Attribute? outAttribute)
        {
            var options = new LobbyDetailsCopyMemberAttributeByIndexOptions()
            {
                AttrIndex = index,
                TargetUserId = puid
            };
            Result result = details.CopyMemberAttributeByIndex(ref options, out outAttribute);
            return result == Result.Success;
        }
        static public bool GetMemberAttributeByKey(LobbyDetails details, ProductUserId puid, string key, out Epic.OnlineServices.Lobby.Attribute? outAttribute)
        {
            var options = new LobbyDetailsCopyMemberAttributeByKeyOptions()
            {
                AttrKey = key,
                TargetUserId = puid
            };
            Result result = details.CopyMemberAttributeByKey(ref options, out outAttribute);
            return result == Result.Success;
        }
        static public void KickMember(LobbyInterface ILobby, string lobbyID, ProductUserId localPUID, ProductUserId targetPUID, OnKickMemberCallback callback = null)
        {
            KickMemberOptions kickOptions = new KickMemberOptions()
            {
                LocalUserId = localPUID,
                TargetUserId = targetPUID,
                LobbyId = lobbyID
            };
            ILobby.KickMember(ref kickOptions, null, callback);
        }
        static public void PromoteMember(LobbyInterface ILobby, string lobbyID, ProductUserId localPUID, ProductUserId targetPUID, OnPromoteMemberCallback callback = null)
        {
            PromoteMemberOptions promoteOptions = new PromoteMemberOptions()
            {
                LocalUserId = localPUID,
                TargetUserId = targetPUID,
                LobbyId = lobbyID
            };
            ILobby.PromoteMember(ref promoteOptions, null, callback);
        }
        static public Result GetLobbyModification(LobbyInterface ILobby, string lobbyID, ProductUserId localPUID, out LobbyModification modification)
        {
            var options = new UpdateLobbyModificationOptions()
            {
                LobbyId = lobbyID,
                LocalUserId = localPUID
            };
            return ILobby.UpdateLobbyModification(ref options, out modification);
        }
        static public bool SetModificationBucketID(LobbyModification modification, string bucketID)
        {
            var optinos = new LobbyModificationSetBucketIdOptions() { BucketId = bucketID };
            Result result = modification.SetBucketId(ref optinos);
            return result == Result.Success;
        }
        static public bool SetModificationMaxMembers(LobbyModification modification, uint maxMembers)
        {
            var optinos = new LobbyModificationSetMaxMembersOptions() { MaxMembers = maxMembers };
            Result result = modification.SetMaxMembers(ref optinos);
            return result == Result.Success;
        }
        static public bool SetModificationInvitesAllowed(LobbyModification modification, bool inviteAllowed)
        {
            var options = new LobbyModificationSetInvitesAllowedOptions() { InvitesAllowed = inviteAllowed };
            Result result = modification.SetInvitesAllowed(ref options);
            return result == Result.Success;
        }
        static public bool SetModificationPermissionLevel(LobbyModification modification, LobbyPermissionLevel permissionLevel)
        {
            var options = new LobbyModificationSetPermissionLevelOptions()
            {
                PermissionLevel = permissionLevel
            };
            Result result = modification.SetPermissionLevel(ref options);
            return result == Result.Success;
        }
        static public bool SetModificationRemoveLobbyAttribute(LobbyModification modification, string attributeKey)
        {
            var options = new LobbyModificationRemoveAttributeOptions()
            {
                Key = attributeKey
            };

            Result result = modification.RemoveAttribute(ref options);
            return result == Result.Success;
        }
        static public Result SetModificationAddLobbyAttribute(LobbyModification modification, Epic.OnlineServices.Lobby.Attribute attribute)
        {
            var options = new LobbyModificationAddAttributeOptions()
            {
                Attribute = attribute.Data,
                Visibility = attribute.Visibility
            };

            return modification.AddAttribute(ref options);
        }
        static public bool SetModificationRemoveMemberAttribute(LobbyModification modification, string attributeKey)
        {
            var options = new LobbyModificationRemoveMemberAttributeOptions()
            {
                Key = attributeKey
            };

            Result result = modification.RemoveMemberAttribute(ref options);
            return result == Result.Success;
        }
        static public bool SetModificationAddMemberAttribute(LobbyModification modification, Epic.OnlineServices.Lobby.Attribute attribute)
        {
            var options = new LobbyModificationAddMemberAttributeOptions()
            {
                Attribute = attribute.Data,
                Visibility = attribute.Visibility
            };

            Result result = modification.AddMemberAttribute(ref options);
            return result == Result.Success;
        }
        static public void UpdateLobby(LobbyInterface ILobby, LobbyModification modification, OnUpdateLobbyCallback callback = null)
        {
            var options = new UpdateLobbyOptions() { LobbyModificationHandle = modification };
            ILobby.UpdateLobby(ref options, null, callback);
        }
        static public Result GetLobbySearch(LobbyInterface ILobby, uint maxResult, out LobbySearch search)
        {
            var options = new CreateLobbySearchOptions() { MaxResults = maxResult };
            return ILobby.CreateLobbySearch(ref options, out search);
        }
        static public bool SetSearchParamUserID(LobbySearch search, ProductUserId targetPUID)
        {
            var Options = new LobbySearchSetTargetUserIdOptions()
            {
                TargetUserId = targetPUID
            };
            Result result = search.SetTargetUserId(ref Options);
            return result == Result.Success;
        }
        static public Result SetSearchParamAttribute(LobbySearch search, Epic.OnlineServices.Lobby.AttributeData attribute, ComparisonOp op)
        {
            var paramOptions = new LobbySearchSetParameterOptions()
            {
                ComparisonOp = op,
                Parameter = new Epic.OnlineServices.Lobby.AttributeData()
                {
                    Key = attribute.Key,
                    Value = attribute.Value
                }
            };
            return search.SetParameter(ref paramOptions);
        }
        static public bool SetSearchParamLobbyId(LobbySearch search, string lobbyId)
        {
            LobbySearchSetLobbyIdOptions setLobbyOptions = new LobbySearchSetLobbyIdOptions()
            {
                LobbyId = lobbyId
            };
            Result result = search.SetLobbyId(ref setLobbyOptions);
            return result == Result.Success;
        }
        static public void SearchLobby(LobbySearch search, ProductUserId localPUID, LobbySearchOnFindCallback onComplete = null)
        {
            var options = new LobbySearchFindOptions()
            {
                LocalUserId = localPUID
            };
            search.Find(ref options,null, onComplete);
        }
        static public uint GetSearchResultCount(LobbySearch search)
        {
            var options = new LobbySearchGetSearchResultCountOptions();
            return search.GetSearchResultCount(ref options);
        }
        static public Result CopySearchResultByIndex(LobbySearch search, uint index, out LobbyDetails outLobbyDetails)
        {
            var options = new LobbySearchCopySearchResultByIndexOptions()
            {
                LobbyIndex = index
            };
            return search.CopySearchResultByIndex(ref options, out outLobbyDetails);
        }
        
        static public ulong AddCBNotifyLobbyUpdateReceived(LobbyInterface ILobby, OnLobbyUpdateReceivedCallback callback)
        {
            var options = new AddNotifyLobbyUpdateReceivedOptions();
            return ILobby.AddNotifyLobbyUpdateReceived(ref options, null, callback);

        }
        static public ulong AddCBNotifyLobbyMemberUpdateReceived(LobbyInterface ILobby, OnLobbyMemberUpdateReceivedCallback callback)
        {
            var options = new AddNotifyLobbyMemberUpdateReceivedOptions();
            return ILobby.AddNotifyLobbyMemberUpdateReceived(ref options, null, callback);
        }
        static public ulong AddCBNotifyLobbyMemberStatusReceived(LobbyInterface ILobby, OnLobbyMemberStatusReceivedCallback callback)
        {
            var options = new AddNotifyLobbyMemberStatusReceivedOptions();
            return ILobby.AddNotifyLobbyMemberStatusReceived(ref options, null, callback);
        }
        static public ulong AddCBNotifyLeaveLobbyRequested(LobbyInterface ILobby,  OnLeaveLobbyRequestedCallback callback)
        {
            var options = new AddNotifyLeaveLobbyRequestedOptions();
            return ILobby.AddNotifyLeaveLobbyRequested(ref options, null, callback);
        }
        static public ulong AddCBNotifyLobbyInviteReceived(LobbyInterface ILobby, OnLobbyInviteReceivedCallback callback)
        {
            var options = new AddNotifyLobbyInviteReceivedOptions();
            return ILobby.AddNotifyLobbyInviteReceived(ref options, null, callback);
        }
        static public ulong AddCBNotifyLobbyInviteAccepted(LobbyInterface ILobby, OnLobbyInviteAcceptedCallback callback)
        {
            var options = new AddNotifyLobbyInviteAcceptedOptions();
            return ILobby.AddNotifyLobbyInviteAccepted(ref options, null, callback);
        }
        static public ulong AddCBNotifyLobbyInviteRejected(LobbyInterface ILobby, OnLobbyInviteRejectedCallback callback)
        {
            var options = new AddNotifyLobbyInviteRejectedOptions();
            return ILobby.AddNotifyLobbyInviteRejected(ref options, null, callback);
        }
        static public ulong AddCBNotifyJoinLobbyAccepted(LobbyInterface ILobby, OnJoinLobbyAcceptedCallback callback)
        {
            var options = new AddNotifyJoinLobbyAcceptedOptions();
            return ILobby.AddNotifyJoinLobbyAccepted(ref options, null, callback);
        }
        #endregion
    }
    public class SessionControl
    {
        static public void CreateSession(SessionsInterface ISession, CreateSessionModificationOptions Createoptions, ProductUserId localPUID, Action<Result> onComplete)
        {
            Result result = ISession.CreateSessionModification(ref Createoptions, out SessionModification modification);
            if (result != Result.Success)
            {
                Debug.LogError($"CreateSessionModification fail : {result}");
                modification.Release();
                onComplete(result);
                return;
            }
            UpdateSession(ISession,modification,(ref UpdateSessionCallbackInfo info) =>
            {
                if (info.ResultCode == Result.Success)
                {
                    RegisterPlayer(ISession,info.SessionName, localPUID);
                }
                onComplete(info.ResultCode);
                modification.Release();
            });
        }
        static public void JoinSession(SessionsInterface ISession, bool presence, string sessionName, ProductUserId localPUID, SessionDetails details, Action<JoinSessionCallbackInfo> onComplete = null)
        {
            var Options = new JoinSessionOptions()
            {
                SessionHandle = details,
                SessionName = sessionName,
                LocalUserId = localPUID,
                PresenceEnabled = presence
            };
            ISession.JoinSession(ref Options, null, (ref JoinSessionCallbackInfo info) =>
            {
                onComplete?.Invoke(info);
            });
        }
        static public void RegisterPlayer(SessionsInterface ISession, string sessionName, ProductUserId userIdToRegister)
        {
            RegisterPlayersOptions registerOptions = new RegisterPlayersOptions()
            {
                SessionName = sessionName,
                PlayersToRegister = new ProductUserId[] { userIdToRegister }
            };
            ISession.RegisterPlayers(ref registerOptions, sessionName, null);
        }
        static public void UnregisterPlayer(SessionsInterface ISession, string sessionName, ProductUserId userIdToUnRegister)
        {
            UnregisterPlayersOptions unregisterOptions = new UnregisterPlayersOptions()
            {
                SessionName = sessionName,
                PlayersToUnregister = new ProductUserId[] { userIdToUnRegister }
            };
            ISession.UnregisterPlayers(ref unregisterOptions, sessionName, null);
        }
        static public void StartSession(SessionsInterface ISession, string sessionName, OnStartSessionCallback OnComplete)
        {
            StartSessionOptions sessionOptions = new StartSessionOptions()
            {
                SessionName = sessionName
            };
            ISession.StartSession(ref sessionOptions, null, (ref StartSessionCallbackInfo data) =>
            {
                if (data.ResultCode != Result.Success)
                {
                    Debug.LogError($" StartSession fail{data.ResultCode} :");
                    return;
                }
                // 세션 멤버들에게 알릴 것.
            });
        }
        static public void EndSession(SessionsInterface ISession, string sessionName, Action<EndSessionCallbackInfo> onComplete = null)
        {
            var Options = new EndSessionOptions()
            {
                SessionName = sessionName
            };
            ISession.EndSession(ref Options, null, (ref EndSessionCallbackInfo info) => {
                onComplete?.Invoke(info);
            });
        }
        static public void DestroySession(SessionsInterface ISession, string sessionName, Action<DestroySessionCallbackInfo> onComplete = null)
        {
            var Options = new DestroySessionOptions()
            {
                SessionName = sessionName
            };
            ISession.DestroySession(ref Options, null, (ref DestroySessionCallbackInfo info) =>
            {
                onComplete?.Invoke(info);
            });
        }
        static public bool CopyActiveSessionByName(SessionsInterface ISession, string sessionName, out ActiveSession activeSession)
        {
            activeSession = null;
            CopyActiveSessionHandleOptions copyOptions = new CopyActiveSessionHandleOptions()
            {
                SessionName = sessionName
            };

            Result result = ISession.CopyActiveSessionHandle(ref copyOptions, out var active);
            if (result == Result.Success)
            {
                activeSession = active;
            }
            return result == Result.Success;
        }
        static public ActiveSessionInfo GetActiveSessionInfo(ActiveSession active)
        {
            var options = new ActiveSessionCopyInfoOptions();
            Result infoResult = active.CopyInfo(ref options, out ActiveSessionInfo? info);
            if (infoResult != Result.Success)
            {
                Debug.LogError( $"CopyDetailsInfo fail");
            }
            return info.Value;
        }
        static public SessionDetailsInfo GetSessionDetailsInfo(SessionDetails details)
        {
            SessionDetailsCopyInfoOptions copyOptions = new SessionDetailsCopyInfoOptions();
            Result result = details.CopyInfo(ref copyOptions, out SessionDetailsInfo? outSessionInfo);
            if (result != Result.Success)
            {
                Debug.LogError($"CopyDetailsInfo fail");
            }
            return outSessionInfo.Value;
        }
        static public SessionDetailsAttribute? GetSessionAttribute(SessionDetails details, string key)
        {
            var Options = new SessionDetailsCopySessionAttributeByKeyOptions()
            {
                AttrKey = key
            };
            Result copyAttrResult = details.CopySessionAttributeByKey(ref Options, out SessionDetailsAttribute? outAttribute);
            return outAttribute;
        }
        static public Dictionary<string, SessionDetailsAttribute> GetSessionAttribute(SessionDetails details)
        {
            Dictionary<string, SessionDetailsAttribute> Attribute = new Dictionary<string, SessionDetailsAttribute>();
            var options = new SessionDetailsGetSessionAttributeCountOptions();
            uint attrCount = details.GetSessionAttributeCount(ref options);
            for (uint i = 0; i < attrCount; i++)
            {
                var attrOptions = new SessionDetailsCopySessionAttributeByIndexOptions();
                attrOptions.AttrIndex = i;
                Result copyAttrResult = details.CopySessionAttributeByIndex(ref attrOptions, out SessionDetailsAttribute? outAttribute);
                if (copyAttrResult == Result.Success)
                {
                    SessionDetailsAttribute data = outAttribute.Value;
                    Attribute[data.Data.Value.Key] = data;
                }
            }
            return Attribute;
        }
        static public bool GetModification(SessionsInterface ISession, string sessionName, out SessionModification modification)
        {
            modification = null;
            var options = new UpdateSessionModificationOptions()
            {
                SessionName = sessionName
            };

            Result result = ISession.UpdateSessionModification(ref options, out modification);
            return result == Result.Success;
        }
        static public void SetModificationBucketID(SessionModification modification, string bucketID)
        {
            var options = new SessionModificationSetBucketIdOptions()
            {
                BucketId = bucketID
            };
            Result result = modification.SetBucketId(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationHostAddress(SessionModification modification, string addr)
        {
            var options = new SessionModificationSetHostAddressOptions()
            {
                HostAddress = addr
            };
            Result result = modification.SetHostAddress(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationMaxPlayers(SessionModification modification, uint maxplayers)
        {
            var options = new SessionModificationSetMaxPlayersOptions()
            {
                MaxPlayers = maxplayers
            };
            Result result = modification.SetMaxPlayers(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationPermissionLevel(SessionModification modification, OnlineSessionPermissionLevel level)
        {
            var options = new SessionModificationSetPermissionLevelOptions()
            {
                PermissionLevel = level
            };

            Result result = modification.SetPermissionLevel(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationAllowJoinInProgress(SessionModification modification, bool allow)
        {
            var options = new SessionModificationSetJoinInProgressAllowedOptions()
            {
                AllowJoinInProgress = allow
            };
            Result result = modification.SetJoinInProgressAllowed(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationInvitesAllowed(SessionModification modification, bool allow)
        {
            var options = new SessionModificationSetInvitesAllowedOptions()
            {
                InvitesAllowed = allow,
            };
            Result result = modification.SetInvitesAllowed(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationAddSessionAttribute(SessionModification modification, SessionDetailsAttribute attribute)
        {
            var options = new SessionModificationAddAttributeOptions()
            {
                AdvertisementType = attribute.AdvertisementType,
                SessionAttribute = attribute.Data
            };
            Result result = modification.AddAttribute(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void SetModificationRemoveSessionAttribute(SessionModification modification, string key)
        {
            var options = new SessionModificationRemoveAttributeOptions()
            {
                Key = key
            };
            Result result = modification.RemoveAttribute(ref options);
            if (result != Result.Success)
            {
                return;
            }
        }
        static public void UpdateSession(SessionsInterface ISession, SessionModification modification, OnUpdateSessionCallback callback = null)
        {
            var options = new UpdateSessionOptions()
            {
                SessionModificationHandle = modification
            };
            ISession.UpdateSession(ref options, null, callback);
        }
        static public void SendInvite(SessionsInterface ISession, ProductUserId localPUID, string sessionName, ProductUserId targetUserId, Epic.OnlineServices.Sessions.OnSendInviteCallback callback = null)
        {
            var options = new Epic.OnlineServices.Sessions.SendInviteOptions()
            {
                SessionName = sessionName,
                LocalUserId = localPUID,
                TargetUserId = targetUserId
            };
            ISession.SendInvite(ref options, null, callback);
        }
        static public void RejectInvite(SessionsInterface ISession,ProductUserId localPUID, string inviteID, Epic.OnlineServices.Sessions.OnRejectInviteCallback callback = null)
        {
            var options = new Epic.OnlineServices.Sessions.RejectInviteOptions()
            {
                InviteId = inviteID,
                LocalUserId = localPUID,
            };
            ISession.RejectInvite(ref options, null, callback);
        }
        static public bool GetSessionSearch(SessionsInterface ISession, uint maxResult, out SessionSearch search)
        {
            var options = new CreateSessionSearchOptions() { MaxSearchResults = maxResult };
            Result result = ISession.CreateSessionSearch(ref options, out search);
            return result == Result.Success;
        }
        static public bool SetSearchParamUserID(SessionSearch search, ProductUserId userid)
        {
            var Options = new SessionSearchSetTargetUserIdOptions()
            {
                TargetUserId = userid
            };
            Result result = search.SetTargetUserId(ref Options);
            return result == Result.Success;
        }
        static public bool SetSearchParamAttribute(SessionSearch search, Epic.OnlineServices.Sessions.AttributeData attribute)
        {
            var paramOptions = new SessionSearchSetParameterOptions()
            {
                ComparisonOp = ComparisonOp.Equal,
                Parameter = new Epic.OnlineServices.Sessions.AttributeData()
                {
                    Key = attribute.Key,
                    Value = attribute.Value
                }
            };
            Result result = search.SetParameter(ref paramOptions);
            return result == Result.Success;
        }
        static public bool SetSearchParamSessionID(SessionSearch search, string sessionID)
        {
            var setLobbyOptions = new SessionSearchSetSessionIdOptions()
            {
                SessionId = sessionID
            };
            Result result = search.SetSessionId(ref setLobbyOptions);
            return result == Result.Success;
        }
        static public void FindSessionDetails(SessionSearch search, ProductUserId localPUID, Action<Dictionary<string, SessionDetails>, Result> onComplete)
        {
            var findSessions = new Dictionary<string, SessionDetails>();
            var sessionSearchFindOptions = new SessionSearchFindOptions()
            {
                LocalUserId = localPUID
            };
            search.Find(ref sessionSearchFindOptions, null, (ref SessionSearchFindCallbackInfo data) =>
            {
                if (data.ResultCode == Result.OperationWillRetry)
                {
                    Debug.Log("LobbySearch Find Result.OperationWillRetry");
                    return;
                }

                if (data.ResultCode == Result.Success)
                {
                    uint searchResultCount = 0;
                    {
                        var options = new SessionSearchGetSearchResultCountOptions();
                        searchResultCount = search.GetSearchResultCount(ref options);
                        Debug.Log("LobbySearch :  SearchResults Lobby objects = " + searchResultCount);
                    }

                    {
                        var options = new SessionSearchCopySearchResultByIndexOptions();
                        for (uint i = 0; i < searchResultCount; i++)
                        {
                            options.SessionIndex = i;
                            Result result = search.CopySearchResultByIndex(ref options, out SessionDetails outSessionDetails);
                            if (result == Result.Success)
                            {
                                string sessionID = GetSessionDetailsInfo(outSessionDetails).SessionId;
                                findSessions.Add(sessionID, outSessionDetails);
                            }
                        }
                    }
                }
                onComplete(findSessions, data.ResultCode);
            });
        }
    }
    public class P2PControl
    {
        static public ulong AddCBNotifyPeerConnectionRequest(P2PInterface IP2P,ProductUserId localPUID, SocketId? socketID = null, OnIncomingConnectionRequestCallback callback = null)
        {
            var options = new AddNotifyPeerConnectionRequestOptions()
            {
                LocalUserId = localPUID,
                SocketId = socketID
            };
            return IP2P.AddNotifyPeerConnectionRequest(ref options, null, callback);
        }
        static public ulong AddCBNotifyIncomingPacketQueueFull(P2PInterface IP2P, ProductUserId localPUID, SocketId socketID, OnIncomingPacketQueueFullCallback callback = null)
        {
            var options = new AddNotifyIncomingPacketQueueFullOptions();
            return IP2P.AddNotifyIncomingPacketQueueFull(ref options, null, callback);
        }
        static public ulong AddCBNotifyPeerConnectionInterrupted(P2PInterface IP2P, ProductUserId localPUID, SocketId? socketID = null, OnPeerConnectionInterruptedCallback callback = null)
        {
            var options = new AddNotifyPeerConnectionInterruptedOptions()
            {
                LocalUserId = localPUID,
                SocketId = socketID
            };
            return IP2P.AddNotifyPeerConnectionInterrupted(ref options, null, callback);
        }
        static public ulong AddCBNotifyPeerConnectionEstablished(P2PInterface IP2P, ProductUserId localPUID, SocketId? socketID = null, OnPeerConnectionEstablishedCallback callback = null)
        {
            var options = new AddNotifyPeerConnectionEstablishedOptions()
            {
                LocalUserId = localPUID,
                SocketId = socketID
            };
            return IP2P.AddNotifyPeerConnectionEstablished(ref options, null, callback);
        }
        static public ulong AddCBNotifyPeerConnectionClosed(P2PInterface IP2P, ProductUserId localPUID, SocketId? socketID = null, OnRemoteConnectionClosedCallback callback = null)
        {
            AddNotifyPeerConnectionClosedOptions options = new AddNotifyPeerConnectionClosedOptions()
            {
                LocalUserId = localPUID,
                SocketId = socketID
            };
            return IP2P.AddNotifyPeerConnectionClosed(ref options, null, callback);
        }
        static public bool AcceptConnection(P2PInterface IP2P, ProductUserId localPUID, ProductUserId productId, SocketId socketID)
        {
            var AcceptConnectionOptions = new AcceptConnectionOptions()
            {
                SocketId = socketID,
                LocalUserId = localPUID,
                RemoteUserId = productId,
            };
            Result result = IP2P.AcceptConnection(ref AcceptConnectionOptions);
            return result == Result.Success;
        }
        static public bool CloseConnection(P2PInterface IP2P, ProductUserId localPUID, ProductUserId productId, SocketId? socketID = null)
        {
            var Options = new CloseConnectionOptions()
            {
                LocalUserId = localPUID,
                RemoteUserId = productId,
                SocketId = socketID
            };
            Result result = IP2P.CloseConnection(ref Options);
            return result == Result.Success;
        }
        static public bool ClearPacketQueue(P2PInterface IP2P, ProductUserId localPUID, ProductUserId productId, SocketId socketID)
        {
            var options = new ClearPacketQueueOptions()
            {
                LocalUserId = localPUID,
                RemoteUserId = productId,
                SocketId = socketID
            };
            Result result = IP2P.ClearPacketQueue(ref options);
            return Result.Success == result;

        }
        static public void QueryNATType(P2PInterface IP2P, OnQueryNATTypeCompleteCallback completionDelegate)
        {
            var options = new QueryNATTypeOptions();
            IP2P.QueryNATType(ref options, null, completionDelegate);
        }
        static public bool GetNATType(P2PInterface IP2P, out NATType outType)
        {
            var options = new GetNATTypeOptions();
            Result result = IP2P.GetNATType(ref options, out outType);
            return result == Result.Success;
        }
        static public bool GetRelayControl(P2PInterface IP2P, out RelayControl outRelayControl)
        {
            var options = new GetRelayControlOptions();
            Result result = IP2P.GetRelayControl(ref options, out outRelayControl);
            return result == Result.Success;
        }
        static public bool GetPacketQueueInfo(P2PInterface IP2P, out PacketQueueInfo outPacketQueueInfo)
        {
            var options = new GetPacketQueueInfoOptions();
            Result result = IP2P.GetPacketQueueInfo(ref options, out outPacketQueueInfo);
            return result == Result.Success;
        }
        static public bool SetRelayControl(P2PInterface IP2P, RelayControl control)
        {
            var options = new SetRelayControlOptions()
            {
                RelayControl = control
            };
            Result result = IP2P.SetRelayControl(ref options);
            return Result.Success == result;
        }
        static public bool SetPortRange(P2PInterface IP2P, ushort port, ushort additionalTry)
        {
            var options = new SetPortRangeOptions()
            {
                Port = port,
                MaxAdditionalPortsToTry = additionalTry
            };
            Result result = IP2P.SetPortRange(ref options);
            return Result.Success == result;
        }
        static public bool SetPacketQueueSize(P2PInterface IP2P, ulong IncomingQueueMaxSizeBytes, ulong OutgoingQueueMaxSizeBytes)
        {
            var options = new SetPacketQueueSizeOptions()
            {
                IncomingPacketQueueMaxSizeBytes = IncomingQueueMaxSizeBytes,
                OutgoingPacketQueueMaxSizeBytes = OutgoingQueueMaxSizeBytes
            };
            Result result = IP2P.SetPacketQueueSize(ref options);
            return Result.Success == result;
        }
        static public bool GetNextReceivedPacketSize(P2PInterface IP2P, ProductUserId localPUID, out uint nextPacketSizeBytes, byte? channel = null)
        {
            nextPacketSizeBytes = 0;
            var options = new GetNextReceivedPacketSizeOptions
            {
                LocalUserId = localPUID,
                RequestedChannel = channel
            };
            Result result = IP2P.GetNextReceivedPacketSize(ref options, out nextPacketSizeBytes);
            return result == Result.Success;
        }
        static public bool ReceiveNextPacket(P2PInterface IP2P, ProductUserId localPUID, ref ProductUserId puid, ref SocketId socketID, uint nextPacketSizeBytes, out ArraySegment<byte>  dataSegment, out byte channel)
        {
            channel = 0;
            dataSegment = null;
            if (nextPacketSizeBytes == 0) return false;
            var packet = new byte[nextPacketSizeBytes];
            dataSegment = new ArraySegment<byte>(packet);
            var options = new ReceivePacketOptions()
            {
                LocalUserId = localPUID,
                MaxDataSizeBytes = P2PInterface.MaxPacketSize,
                RequestedChannel = null
            };
            Result result = IP2P.ReceivePacket(ref options, ref puid, ref socketID, out channel, dataSegment, out uint bytesWritten);
            return result == Result.Success;
        }
        static public bool IsValidSocketName(string name)
        {
            return (name != null)
                && (name.Length > 0)
                && (name.Length <= 32)
                && Regex.IsMatch(name, "^[a-zA-Z0-9]+$");
        }
        static public bool SendPacket(P2PInterface IP2P, SendPacketOptions options)
        {
            Result result = IP2P.SendPacket(ref options);
            return result == Result.Success;
        }
    }
}