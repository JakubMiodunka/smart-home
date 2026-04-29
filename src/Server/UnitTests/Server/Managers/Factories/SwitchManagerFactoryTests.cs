using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.ApiClients.StationApi;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Managers;
using SmartHome.Server.Managers.Factories;
using SmartHome.UnitTests;

namespace SmartHome.SmartHome.UnitTests.Server.Managers.Factories;

[Category("UnitTest")]
[TestOf(typeof(SwitchManagerFactory))]
[Author("Jakub Miodunka")]
public sealed class SwitchManagerFactoryTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationClientsFactory()
    {
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            null!,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLoggerFactory()
    {
        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            stationApiClientsFactoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion

    #region Manager creation
    [Test]
    public void ManagerCreationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SwitchManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        StationEntity parentStation = randomizer.NextStationEntity() with { Id = switchEntity.StationId };
        ISwitchManager switchManager = factoryUnderTest.CreateFor(switchEntity, parentStation);

        Assert.That(switchManager, Is.Not.Null);
        Assert.That(switchManager, Is.InstanceOf<SwitchManager>());
    }
    #endregion
}
