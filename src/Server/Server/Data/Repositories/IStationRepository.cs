using Server.Data.Models.Entities;
using System.Net.NetworkInformation;

namespace Server.Data.Repositories;

public interface IStationRepository
{
    Task<bool> IsStationExistAsync(PhysicalAddress macAddress);
    Task<StationEntity> CreateStationAsync(StationEntity station);
}
