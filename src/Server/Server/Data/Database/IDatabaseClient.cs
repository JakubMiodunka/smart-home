using SmartHome.Server.Data.Repositories;

namespace SmartHome.Server.Data.Database;

/// <summary>
/// Defines interactions with the database client containing all data aggregated by the system.
/// </summary>
/// <remarks>
/// This interface is primarily created to support repository classes whose capabilities 
/// may exceed pure SQL execution. Such classes shall interact with the database through this interface.
/// </remarks>
public interface IDatabaseClient : IStationsRepository;
