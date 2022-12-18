namespace LoginUdp11Types
{
    public interface ILoginRequest
    {
        string SessionId { get; set; }
        string SystemFingerPrint { get; set; }
        int LangaugeSetLocale { get; set; }
        int ThirdPartyAuthTicket { get; set; }
        int ThirdPartyUserId { get; set; }
        int ThirdPartyId { get; set; }

    }

    public interface ILoginReply
    {
        bool LoggedIn { get; set; }
        int Status { get; set; }
        int ResultCode { get; set; }
        bool IsGameMember { get; set; }
        bool IsGameInternal { get; set; }
        string CharacterNameSpace { get; set; }
        dynamic[] AccountFeatures { get; set; }
        dynamic ApplicationPayload { get; set; }
        dynamic[] ErrorDetails { get; set; }
        string IpCountryCode { get; set; }
    }

    public interface ILoguout
    {
        // empty...        
    }

    public interface IForceDisconnect
    {
        int Reason { get; set; }
    }

    public interface ICharacterCreateRequest
    {
        int ServerId { get; set; }
        int Unknown { get; set; }
        dynamic Payload { get; set; }
    }

    public interface ICharacterCreateReply
    {
        int Status { get; set; }
        string CharacterId { get; set; }
    }

    public interface ICharacterLoginRequest
    {
        string CharacterId { get; set; }
        int ServerId { get; set; }
        int Status { get; set; }
        dynamic Payload { get; set; }
    }

    public interface ICharacterLoginReply
    {
        string UnknownQword1 { get; set; }
        int UnknownDword1 { get; set; }
        int UnknownDword2 { get; set; }
        int Status { get; set; }
        dynamic ApplicationData { get; set; }
    }

    public interface ICharacterDeleteRequest
    {
        string CharacterId { get; set; }
    }

    public interface ICharacterDeleteReply
    {
        string CharacterId { get; set; }
        int Status { get; set; }
        string Payload { get; set; }
    }

    public interface ICharacterSelectInfoRequest
    {
        // empty...
    }

    public interface ICharacterSelectInfoReply
    {
        int Status { get; set; }
        bool CanBypassServerLock { get; set; }
        dynamic[] Characters { get; set; }
    }

    public interface IServerListRequest
    {
        // empty...
    }

    public interface IServerListReply
    {
        dynamic[] Servers { get; set; }
    }

    public interface IServerUpdate
    {
        int ServerId { get; set; }
        int ServerState { get; set; }
        bool Locked { get; set; }
        string Name { get; set; }
        int NameId { get; set; }
        string Description { get; set; }
        int DescriptionId { get; set; }
        int ReqFeatureId { get; set; }
        string ServerInfo { get; set; }
        int PopulationLevel { get; set; }
        string PopulationData { get; set; }
        string AccessExpression { get; set; }
        bool AllowedAccess { get; set; }
    }

    public interface ITunnelAppPacketClientToServer
    {
        // empty...
    }

    public interface ITunnelAppPacketServerToClient
    {
        // empty...
    }

    public enum LoginUdp11Types
    {
        ILoginRequest,
        ILoginReply,
        ILogout,
        IForceDisconnect,
        ICharacterCreateRequest,
        ICharacterCreateReply,
        ICharacterLoginRequest,
        ICharacterLoginReply,
        ICharacterDeleteRequest,
        ICharacterDeleteReply,
        ICharacterSelectInfoRequest,
        ICharacterSelectInfoReply,
        IServerListRequest,
        IServerListReply,
        IServerUpdate,
        ITunnelAppPacketClientToServer,
        ITunnelAppPacketServerToClient,
    }
}