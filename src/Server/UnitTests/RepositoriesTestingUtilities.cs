using Moq;
using SmartHome.Server.Data.Repositories;
using System.Net;
using System.Net.NetworkInformation;

namespace UnitTests;

internal static class RepositoriesTestingUtilities
{
    public static void AssertNoContentModifications(this Mock<IStationsRepository> repositoryMock)
    {
        repositoryMock.Verify(mock => mock
            .CreateStationAsync(
                It.IsAny<PhysicalAddress>(),
                It.IsAny<IPAddress?>(),
                It.IsAny<int?>(),
                It.IsAny<byte?>(),
                It.IsAny<DateTimeOffset>()),
            Times.Never);

        repositoryMock.Verify(mock => mock
            .UpdateStationAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<IPAddress?>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<bool>(),
                It.IsAny<byte?>(),
                It.IsAny<bool>(),
                It.IsAny<DateTimeOffset?>()),
            Times.Never);
    }


    public static void AssertNoContentModifications(this Mock<ISwitchesRepository> repositoryMock)
    {
        repositoryMock.Verify(mock =>
            mock.CreateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<byte>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);

        repositoryMock.Verify(mock =>
            mock.UpdateSwitchAsync(
                It.IsAny<long>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>(),
                It.IsAny<bool>(),
                It.IsAny<bool?>()),
            Times.Never);
    }
}
