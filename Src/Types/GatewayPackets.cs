namespace GatewayTypes
{
    public interface ILoginRequest
    {
        string LoginRequest { get; set; }
        string Ticket { get; set; }
        string ClientProtocol { get; set; }
        string ClientBuild { get; set; }
    }

    public interface ILoginReply
    {
        string LoggedIn { get; set; }
    }

    public interface ILogout
    {
        // empty...
    }

    public interface IForceDisconnect
    {
        // empty...
    }

    public interface IChannelIsRoutable
    {
        bool IsRoutable { get; set; }
    }

    public interface IConnectionIsNotRoutable
    {
        // empty...
    }

    public enum GatewayTypes
    {
        ILoginRequest,
        ILoginReply,
        ILogout,
        IForceDisconnect,
        IChannelIsRoutable,
        IConnectionIsNotRoutable,
    }
}
