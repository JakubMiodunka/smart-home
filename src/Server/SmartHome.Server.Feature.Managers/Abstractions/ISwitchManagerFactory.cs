using SmartHome.Server.Repositories.Entities;

namespace SmartHome.Server.Feature.Managers.Abstractions;

/// <summary>
/// Factory for creating managers that control electrical switches.
/// </summary>
public interface ISwitchManagerFactory
{
    /// <summary>
    /// Creates manager for the electrical switch.
    /// </summary>
    /// <param name="switchEntity">
    /// Entity of switch which shall be controlled by created manager.
    /// </param>
    /// <param name="parentStation">
    /// Entity of station, that controls the specified switch.
    /// </param>
    /// <returns>
    /// An <see cref="ISwitchManager"/> instance that allows performing operations on the switch.
    /// </returns>
    ISwitchManager CreateFor(SwitchEntity switchEntity, StationEntity parentStation);
}
