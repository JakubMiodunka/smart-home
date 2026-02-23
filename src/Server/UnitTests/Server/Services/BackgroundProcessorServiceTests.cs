using Microsoft.Extensions.Logging;
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
    /// <remarks>
    /// This test is sensitive to timing and CPU scheduling.
    /// Running it in isolation prevents non - deterministic failures caused by resource contention.
    /// </remarks>
    [NonParallelizable]
    [TestCase(1, 100)]
    public async Task ServiceExecutionTriggeredInSpecifiedInterval(
        long serviceExecutionInterval,  // Given in milliseconds.
        int numberOfIntervals)
    {
        var serviceProcessorMock = new Mock<IBackgroundServiceProcessor>();

        int serviceExecutionCounter = 0;

        serviceProcessorMock.Setup(processor => processor
            .ProcessAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Callback(() => Interlocked.Increment(ref serviceExecutionCounter));

        TimeSpan executionInterval = TimeSpan.FromMilliseconds(serviceExecutionInterval);

        var timeProviderStub = new FakeTimeProvider();
        var loggerMock = new FakeLogger<BackgroundProcessorService>();

        var service = new BackgroundProcessorService(
                serviceProcessorMock.Object,
                timeProviderStub,
                executionInterval,
                loggerMock);

        // Ensure time advancement exceeds the threshold to trigger PeriodicTimer ticks.
        TimeSpan simulationStep = executionInterval.Add(TimeSpan.FromMicroseconds(1));

        await service.StartAsync(CancellationToken.None);

        /* 
         * Switching context to allow the task scheduler to transition service
         * execution to the BackgroundService.ExecuteAsync loop.
         */
        await Task.Delay(10);

        for (int currentInterval = 0; currentInterval < numberOfIntervals; currentInterval++)
        {
            /* 
             * There is a need to force a context switch here to allow the background task to process
             * the pending timer tick. It might be counterintuitive, but it is important to do it BEFORE
             * advancing the time ensure the service loop has returned to its 'waiting' state (WaitForNextTickAsync).
             * If we advance the time while the previous tick is still being processed, 
             * the timer may miss the signal, leading to non-deterministic failures.
             */
            await Task.Delay(10);
            timeProviderStub.Advance(simulationStep);
        }

        await service.StopAsync(CancellationToken.None);

        /*
         * Decided to use NUnit's polling assertion on counter value instead of immediate Mock.Verify
         * to account for asynchronous execution latency and ensure the background loop 
         * has fully committed all increments before the assertion fails.
         */ 
        Assert.That(() => serviceExecutionCounter, Is.EqualTo(numberOfIntervals).After(1000, 10));
        
        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.None.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }

    /// <remarks>
    /// This test is sensitive to timing and CPU scheduling.
    /// Running it in isolation prevents non - deterministic failures caused by resource contention.
    /// Refer to <see cref="ServiceExecutionTriggeredInSpecifiedInterval"/> for detailed insights
    /// into the underlying test execution flow and timing synchronization.
    /// </remarks>
    [NonParallelizable]
    [TestCase(1, 100)]
    public async Task ExecutionLoopContinuesWhenProcessorThrowsException(
        long serviceExecutionInterval,  // Given in milliseconds.
        int numberOfIntervals)
    {
        var serviceProcessorMock = new Mock<IBackgroundServiceProcessor>();

        int serviceExecutionCounter = 0;

        serviceProcessorMock.Setup(processor => processor
            .ProcessAsync(It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                Interlocked.Increment(ref serviceExecutionCounter);

                if (serviceExecutionCounter == 2)
                {
                    // Simulated processor failure.
                    throw new Exception();
                }

                await Task.CompletedTask;
            });

        TimeSpan executionInterval = TimeSpan.FromMilliseconds(serviceExecutionInterval);

        var timeProviderStub = new FakeTimeProvider();
        var loggerMock = new FakeLogger<BackgroundProcessorService>();

        var service = new BackgroundProcessorService(
                serviceProcessorMock.Object,
                timeProviderStub,
                executionInterval,
                loggerMock);

        TimeSpan simulationStep = executionInterval.Add(TimeSpan.FromMicroseconds(1));

        await service.StartAsync(CancellationToken.None);

        await Task.Delay(10);

        for (int currentInterval = 0; currentInterval < numberOfIntervals; currentInterval++)
        {
            await Task.Delay(10);
            timeProviderStub.Advance(simulationStep);
        }

        await service.StopAsync(CancellationToken.None);

        Assert.That(() => serviceExecutionCounter, Is.EqualTo(numberOfIntervals).After(1000, 10));

        IReadOnlyList<FakeLogRecord> logMessages = loggerMock.Collector.GetSnapshot();
        Assert.That(logMessages, Is.Not.Empty);
        Assert.That(logMessages, Has.Some.Matches<FakeLogRecord>(record => LogLevel.Information < record.Level));
    }
    #endregion
}
