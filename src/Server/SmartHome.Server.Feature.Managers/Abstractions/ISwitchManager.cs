namespace SmartHome.Server.Feature.Managers.Abstractions;

/// <summary>
/// Manages the state of a specific switch on a remote station.
/// </summary>
/// <remarks>
/// Does not update details of managed switch in any repository, that's the responsibility of the caller.
/// </remarks>
public interface ISwitchManager
{
    /// <summary>
    /// Attempts to change state of managed electrical switch.
    /// </summary>
    /// <param name="expectedState">
    /// Desired state of electrical switch - <see langword="true"/> if the circuit shall be closed 
    /// and current shall be flowing, <see langword="false"/> otherwise. 
    /// </param>
    /// <param name="cancellationToken">
    /// A token to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if operation was successful, <see langword="false"/>otherwise.
    /// </returns>
    Task<bool> TryChangeState(bool expectedState, CancellationToken cancellationToken);
}
