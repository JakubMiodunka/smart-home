using Server.Data.Models.Descriptors.Api;

namespace Server.Data.Models.Requests;

public sealed record RegisterStationRequest(
    StationApiDescriptor StationApiDescriptor,
    ElectricalSwitchApiDescriptor[] ElectricalSwitchDescriptors)
{
    // Empty array insteead of null. TODO: Maybe refactor it a little later.
    public ElectricalSwitchApiDescriptor[] ElectricalSwitchDescriptors { get; init; } =
        ElectricalSwitchDescriptors ?? Array.Empty<ElectricalSwitchApiDescriptor>();
}
