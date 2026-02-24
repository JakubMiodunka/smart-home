using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Testing;
using Microsoft.Extensions.Time.Testing;
using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data;
using SmartHome.Server.Data.Repositories;
using SmartHome.Server.Services;
using SmartHome.Server.Services.Processors;

namespace SmartHome.UnitTests.Server.Services;

[Category("UnitTest")]
[TestOf(typeof(HeartbeatMonitoringServiceProcessor))]
[Author("Jakub Miodunka")]
public sealed class HeartbeatMonitoringServiceProcessorTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
         // TODO: Continue here.
    }
    #endregion
}
