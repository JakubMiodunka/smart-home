using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Managers;

/// <summary>
/// Manager, which is able to control an electrical switch.
/// </summary>
public interface IElectricalSwitchManager
{
    /// <summary>
    /// Changes state of managed electrical switch.
    /// </summary>
    /// <param name="shallBeClosed">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    public bool TryChangeState(bool shallBeClosed);
}

/// <summary>
/// Provides logic to communicate with and control an electrical switch via its associated station.
/// </summary>
public sealed class ElectricalSwitchManager : IElectricalSwitchManager
{
    #region Properties
    public ElectricalSwitchEntity ManagedSwitch { get; init; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new manager instance.
    /// </summary>
    /// <param name="managedElectricalSwitch">
    /// Entity of electrical switch which shall be controlled by the manager.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable reference-type argument is a <see langword="null"/> reference.
    /// </exception>
    public ElectricalSwitchManager(ElectricalSwitchEntity managedElectricalSwitch)
    {
        ArgumentNullException.ThrowIfNull(managedElectricalSwitch);

        ManagedSwitch = managedElectricalSwitch;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Sends a command to station associated with managed electrical switch to change its state.
    /// </summary>
    /// <remarks>
    /// TODO: Implement the actual communication with the station.
    /// </remarks>
    /// <param name="shallBeClosed">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if the command was successfully delivered and acknowledged by the station, 
    /// <see langword="false"/>otherwise.
    /// </returns>
    public bool TryChangeState(bool shallBeClosed)
    {
        return true;
    }
    #endregion
}
