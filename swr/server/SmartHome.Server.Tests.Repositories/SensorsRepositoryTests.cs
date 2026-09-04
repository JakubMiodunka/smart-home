using NUnit.Framework.Internal;
using SmartHome.Server.Repositories;

namespace SmartHome.Server.Tests.Repositories.TypeHandlers;

[Category("UnitTest")]
[TestOf(typeof(SensorsRepository))]
[Author("Jakub Miodunka")]
internal sealed class SensorsRepositoryTests
{
    #region Constructor
    [Test]
    public void InstantiationPossible()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;
        string connectionString = randomizer.GetString();   // Currently, constructor accepts any string as connection string.
        Action actionUnderTest = () => new SensorsRepository(connectionString);

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void InstantiationImpossibleUsingNullReferenceAsConnectionString()
    {
        Action actionUnderTest = () => new SensorsRepository(null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }
    #endregion
}
