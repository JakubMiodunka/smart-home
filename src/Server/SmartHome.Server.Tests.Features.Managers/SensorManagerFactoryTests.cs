using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Api.Clients.Abstractions;
using SmartHome.Server.Features.Managers;
using SmartHome.Server.Features.Managers.Abstractions;
using SmartHome.Server.Repositories.Entities;
using SmartHome.Server.Tests.Utilities;

namespace SmartHome.Server.Tests.Features.Managers;

[Category("UnitTest")]
[TestOf(typeof(SensorManagerFactory))]
[Author("Jakub Miodunka")]
internal sealed class SensorManagerFactoryTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        Action actionUnderTest = () => new SensorManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationClientsFactory()
    {
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        Action actionUnderTest = () => new SensorManagerFactory(
            null!,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLoggerFactory()
    {
        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        Action actionUnderTest = () => new SensorManagerFactory(
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

        var factoryUnderTest = new SensorManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();
        StationEntity parentStation = randomizer.NextOnlineStationEntity() with { Id = sensorEntity.StationId };
        ISensorManager sensorManager = factoryUnderTest.CreateFor(sensorEntity, parentStation);

        Assert.That(sensorManager, Is.Not.Null);
        Assert.That(sensorManager, Is.InstanceOf<SensorManager>());
    }

    [Test]
    public void ManagerCreationImpossibleUsingNullReferenceAsSensorEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SensorManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        StationEntity stationEntity = randomizer.NextOnlineStationEntity();

        ISensorManager? sensorManager = null;
        Action actionUnderTest = () => sensorManager = factoryUnderTest.CreateFor(null!, stationEntity);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        Assert.That(sensorManager, Is.Null);
    }

    [Test]
    public void ManagerCreationImpossibleUsingNullReferenceAsParentStationEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SensorManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        ISensorManager? sensorManager = null;
        Action actionUnderTest = () => sensorManager = factoryUnderTest.CreateFor(sensorEntity, null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        Assert.That(sensorManager, Is.Null);
    }

    [Test]
    public void ManagerCreationImpossibleWhenStationEntityIsNotSensorParentStation()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var stationApiClientsFactoryStub = new Mock<IStationApiClientFactory>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SensorManagerFactory(
            stationApiClientsFactoryStub.Object,
            loggerFactoryStub.Object);

        SensorEntity sensorEntity = randomizer.NextSensorEntity();

        StationEntity parentStation = randomizer.NextOnlineStationEntity();
        while (parentStation.Id == sensorEntity.StationId)
        {
            parentStation = randomizer.NextOnlineStationEntity();
        };

        ISensorManager? sensorManager = null;
        Action actionUnderTest = () => sensorManager = factoryUnderTest.CreateFor(sensorEntity, parentStation);

        Assert.Throws<ArgumentException>(actionUnderTest);
        Assert.That(sensorManager, Is.Null);
    }
    #endregion
}
