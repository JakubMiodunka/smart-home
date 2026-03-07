using SmartHome.Server.Data.Models.Entities;

namespace SmartHome.Server.Managers;

/// <summary>
/// Manager, which is able to control an electrical switch.
/// </summary>
public interface ISwitchManager
{
    /// <summary>
    /// Changes state of managed electrical switch.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    public bool TryChangeState(bool expectedState);
}

/// <summary>
/// Provides logic to communicate with and control an electrical switch via its associated station.
/// </summary>
public sealed class SwitchManager : ISwitchManager
{
    #region Properties
    public SwitchEntity ManagedSwitch { get; init; }
    #endregion

    #region Instantiation
    /// <summary>
    /// Creates new instance of <see cref="SwitchManager"/>.
    /// </summary>
    /// <param name="managedSwitch">
    /// Entity of switch which shall be controlled by the manager.
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// Thrown, when at least one non-nullable argument is a <see langword="null"/> reference.
    /// </exception>
    public SwitchManager(SwitchEntity managedSwitch)
    {
        ArgumentNullException.ThrowIfNull(managedSwitch);

        ManagedSwitch = managedSwitch;
    }
    #endregion

    #region Interacitons
    /// <summary>
    /// Sends a command to station associated with managed electrical switch to change its state.
    /// </summary>
    /// <remarks>
    /// TODO: Implement the actual communication with the station.
    /// </remarks>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing; <see langword="false"/> otherwise. 
    /// </param>
    /// <see langword="true"/> if the command was successfully delivered and acknowledged by the station, 
    /// <see langword="false"/>otherwise.
    /// </returns>
    public bool TryChangeState(bool expectedState) => true; // TODO: Implement
    #endregion
}
