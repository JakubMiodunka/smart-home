using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Managers.Factories;

/// <summary>
/// Factory for creating managers that control electrical switches.
/// </summary>
public interface IElectricalSwitchManagerFactory
{
    /// <summary>
    /// Creates manager for the electrical switch.
    /// </summary>
    /// <param name="switchEntity">
    /// Entity of electrical switch which shall be controlled by created manager.
    /// </param>
    /// <returns>
    /// An <see cref="ElectricalSwitchManager"/> instance that allows performing operations on the switch.
    /// </returns>
    IElectricalSwitchManager CreateFor(ElectricalSwitchEntity switchEntity);
}

/// <inheritdoc cref="IElectricalSwitchManagerFactory"/>
public sealed class ElectricalSwitchManagerFactory : IElectricalSwitchManagerFactory
{
    /// <inheritdoc cref="IElectricalSwitchManagerFactory"/>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public IElectricalSwitchManager CreateFor(ElectricalSwitchEntity switchEntity)
    {
        ArgumentNullException.ThrowIfNull(switchEntity, nameof(switchEntity));

        return new ElectricalSwitchManager(switchEntity);
    }
}
