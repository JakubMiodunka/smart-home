using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Services;
using SmartHome.Server.Services.Processors;
using System.Diagnostics.Metrics;

namespace SmartHome.UnitTests.Server.Services;

[Category("UnitTest")]
[TestOf(typeof(BackgroundProcessorService))]
[Author("Jakub Miodunka")]
public sealed class BackgroundProcessorServiceTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan serviceExecutionInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var timeProviderStub = new FakeTimeProvider();
        var serviceProcessorStub = new Mock<IBackgroundServiceProcessor>();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                serviceProcessorStub.Object,
                timeProviderStub,
                serviceExecutionInterval,
                loggerStub);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsServiceProcessor()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan serviceExecutionInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                null!,
                timeProviderStub,
                serviceExecutionInterval,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsTimeProvider()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan serviceExecutionInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var serviceProcessorStub = new Mock<IBackgroundServiceProcessor>();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                serviceProcessorStub.Object,
                null!,
                serviceExecutionInterval,
                loggerStub);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsLogger()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        TimeSpan serviceExecutionInterval = randomizer.NextTimeSpan(
            from: TimeSpan.FromMicroseconds(1));

        var timeProviderStub = new FakeTimeProvider();
        var serviceProcessorStub = new Mock<IBackgroundServiceProcessor>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                serviceProcessorStub.Object,
                timeProviderStub,
                serviceExecutionInterval,
                null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingZeroServiceExecutionInterval()
    {
        var serviceExecutionInterval = TimeSpan.Zero;

        var serviceProcessorStub = new Mock<IBackgroundServiceProcessor>();
        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                serviceProcessorStub.Object,
                timeProviderStub,
                serviceExecutionInterval,
                loggerStub);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNegativeServiceExecutionInterval()
    {
        var serviceExecutionInterval = TimeSpan.FromMicroseconds(-1);

        var timeProviderStub = new FakeTimeProvider();
        var serviceProcessorStub = new Mock<IBackgroundServiceProcessor>();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        TestDelegate actionUnderTest = () => new BackgroundProcessorService(
                serviceProcessorStub.Object,
                timeProviderStub,
                serviceExecutionInterval,
                loggerStub);

        Assert.Throws<ArgumentOutOfRangeException>(actionUnderTest);
    }
    #endregion

    #region Service execution
    // TODO: Continue here. Context available in Gemini.
    [TestCase(5, 290)]
    public async Task ServiceExecutionTriggeredInSpecifiedInterval(int serviceExecutionInterval, int length)
    {
        var serviceProcessorMock = new Mock<IBackgroundServiceProcessor>();

        int serviceExecutionCounter = 0;

        serviceProcessorMock.Setup(processor => processor
            .ProcessAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => serviceExecutionCounter++);

        var timeProviderStub = new FakeTimeProvider();
        var loggerStub = new FakeLogger<BackgroundProcessorService>();

        var service = new BackgroundProcessorService(
                serviceProcessorMock.Object,
                timeProviderStub,
                TimeSpan.FromMilliseconds(serviceExecutionInterval),
                loggerStub);

        using var serviceCancelationTokenSource = new CancellationTokenSource();
        await service.StartAsync(serviceCancelationTokenSource.Token);

        foreach (var _ in Enumerable.Range(0, length))
        {

        }

        await service.StopAsync(CancellationToken.None);

        Assert.That(serviceExecutionCounter, Is.EqualTo(serviceActivationTime / serviceExecutionInterval));
    }
    #endregion
}
