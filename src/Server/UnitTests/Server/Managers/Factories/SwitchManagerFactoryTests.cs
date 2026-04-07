using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers;
using SmartHome.Server.Managers.Factories;
using SmartHome.UnitTests;
using UnitTests;

namespace SmartHome.UnitTests.Server.Managers.Factories;

[Category("UnitTest")]
[TestOf(typeof(SwitchManagerFactory))]
[Author("Jakub Miodunka")]
public sealed class SwitchManagerFactoryTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerFactoryStub.Object);

        Assert.DoesNotThrow(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpClientFactory()
    {
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            null!,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsStationsRepository()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            null!,
            switchesRepositoryMock.Object,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            null!,
            loggerFactoryStub.Object);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLoggerFactory()
    {
        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        TestDelegate actionUnderTest = () => new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }
    #endregion

    #region Manager creation
    [Test]
    public void ManagerCreationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerFactoryStub.Object);

        SwitchEntity switchEntity = randomizer.NextSwitchEntity();
        ISwitchManager switchManager = factoryUnderTest.CreateFor(switchEntity);

        Assert.That(switchManager, Is.Not.Null);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }

    [Test]
    public void ManagerCreationImpossibleUsingNullReferenceAsSwitchEntity()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        var httpClientFactoryStub = new Mock<IHttpClientFactory>();
        var stationsRepositoryMock = new Mock<IStationsRepository>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerFactoryStub = new Mock<ILoggerFactory>();

        loggerFactoryStub.Setup(factory => factory
            .CreateLogger(It.IsAny<string>()))
            .Returns((string categoryName) => new FakeLogger(new FakeLogCollector(), categoryName));

        var factoryUnderTest = new SwitchManagerFactory(
            httpClientFactoryStub.Object,
            stationsRepositoryMock.Object,
            switchesRepositoryMock.Object,
            loggerFactoryStub.Object);

        TestDelegate actionUnderTest = () => factoryUnderTest.CreateFor(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);

        stationsRepositoryMock.AssertNoContentModifications();
        switchesRepositoryMock.AssertNoContentModifications();
    }
    #endregion
}
