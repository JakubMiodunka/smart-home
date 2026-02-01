using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Managers.Factories;

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
    /// <returns>
    /// An <see cref="SwitchManager"/> instance that allows performing operations on the switch.
    /// </returns>
    ISwitchManager CreateFor(SwitchEntity switchEntity);
}

/// <inheritdoc cref="ISwitchManagerFactory"/>
public sealed class SwitchManagerFactory : ISwitchManagerFactory
{
    /// <inheritdoc cref="ISwitchManagerFactory"/>
    public ISwitchManager CreateFor(SwitchEntity switchEntity) =>
        new SwitchManager(switchEntity);
}
