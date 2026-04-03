using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Controllers.Clients;
using SmartHome.Server.Data.Models.Entities;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Managers.Factories;
using SmartHome.UnitTests;
using System.Net;

namespace UnitTests.Server.Controllers.Clients;

// TODO: Add rest of required test cases.
[Category("UnitTest")]
[TestOf(typeof(SwitchesController))]
[Author("Jakub Miodunka")]
public sealed class SwitchesControllerTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsHttpContextAccessor()
    {
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            null!,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchesRepository()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            null!,
            switchManagerFactoryStub.Object,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsSwitchManagerFactory()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var loggerStub = new FakeLogger<SwitchesController>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            null!,
            loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertThatNoDataModifications();
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        var httpContextAccessorStub = new Mock<IHttpContextAccessor>();
        var switchesRepositoryMock = new Mock<ISwitchesRepository>();
        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();

        TestDelegate actionUnderTest = () => new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
        switchesRepositoryMock.AssertThatNoDataModifications();
    }
    #endregion

    #region Switch retrival
    [TestCase(2)]
    public async Task GettingAllSwitchesPossible(int switchesInRepository)
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress clientIpAddress = randomizer.NextIpAddress();
        Mock<IHttpContextAccessor> httpContextAccessorStub = 
            TestDataGenerator.CreateHttpContextAccessorFake(clientIpAddress);

        SwitchEntity[] repositoryContent = Enumerable.Range(1, switchesInRepository)
            .Select(id => randomizer.NextSwitchEntity() with { Id = id })
            .ToArray();

        var switchesRepositoryMock = new Mock<ISwitchesRepository>();

        switchesRepositoryMock.Setup(mock => mock
            .GetMultipleSwitchesAsync())
            .ReturnsAsync(repositoryContent);

        var switchManagerFactoryStub = new Mock<ISwitchManagerFactory>();
        var loggerMock = new FakeLogger<SwitchesController>();

        var controllerUnderTest = new SwitchesController(
            httpContextAccessorStub.Object,
            switchesRepositoryMock.Object,
            switchManagerFactoryStub.Object,
            loggerMock);

        IActionResult response = await controllerUnderTest.GetSwitches();

        // TODO: Add extension to repsponse assertion to handle collections and use it here.

        switchesRepositoryMock.AssertThatNoDataModifications();

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));

    }
    #endregion

    #region Switch update
    #endregion
}
