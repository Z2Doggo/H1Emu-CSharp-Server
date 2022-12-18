namespace LoginTypes
{
    public interface IGameServer
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
        bool AllowedAccess { get; set; }
    }
}