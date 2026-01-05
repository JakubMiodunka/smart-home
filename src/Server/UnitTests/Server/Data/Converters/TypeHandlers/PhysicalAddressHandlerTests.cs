using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Converters.TypeHandlers;
using System.Data;
using System.Net.NetworkInformation;
using UnitTests;

namespace SmartHome.UnitTests.Server.Data.Converters.TypeHandlers;

[Category("UnitTest")]
[TestOf(typeof(PhysicalAddressHandler))]
[Author("Jakub Miodunka")]
public class PhysicalAddressHandlerTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        TestDelegate actionUnderTest = () => new PhysicalAddressHandler();

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void SettingParameterImpossibleUsingNullReferenceAsParameter()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();

        var handlerUnderTest = new PhysicalAddressHandler();
        TestDelegate actionUnderTest = () => handlerUnderTest.SetValue(null!, macAddress);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void ParameterTypeSetCorrectly()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new PhysicalAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, macAddress);

        dbDataParameterMock.VerifySet(mock => mock.DbType = DbType.AnsiStringFixedLength);
    }

    [Test]
    public void ParameterSizeSetCorrectly()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new PhysicalAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, macAddress);

        dbDataParameterMock.VerifySet(mock => mock.Size = 12);
    }

    [Test]
    public void ParameterValueSetCorrectlyIfMacAddressIsNotNullReference()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new PhysicalAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, macAddress);

        dbDataParameterMock.VerifySet(mock => mock.Value = macAddress.ToString());
    }

    [Test]
    public void ParameterValueSetCorrectlyIfMacAddressIsNullReference()
    {
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new PhysicalAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, null);

        dbDataParameterMock.VerifySet(mock => mock.Value = DBNull.Value);
    }

    [Test]
    public void ResultOfParsingNullReferenceIsNullReference()
    {
        var handlerUnderTest = new PhysicalAddressHandler();
        PhysicalAddress? parsedMacAddress = handlerUnderTest.Parse(null);

        Assert.That(parsedMacAddress, Is.Null);
    }

    [Test]
    public void ResultOfParsingNonStringReferenceIsNullReference()
    {
        var handlerUnderTest = new PhysicalAddressHandler();
        PhysicalAddress? parsedMacAddress = handlerUnderTest.Parse(new object());

        Assert.That(parsedMacAddress, Is.Null);
    }

    [Test]
    public void ResultOfParsingValidMacAddressStringIsCorrespondingPhysicalsAddressObject()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress inputMacAddress = randomizer.NextMacAddress();

        var handlerUnderTest = new PhysicalAddressHandler();
        PhysicalAddress? parsedMacAddress = handlerUnderTest.Parse(inputMacAddress.ToString());

        Assert.That(parsedMacAddress, Is.EqualTo(inputMacAddress));
    }

    [Test]
    public void ParsingImpossibleUsingInvalidMacAddressString()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        string invalidMacAddressString = randomizer.GetString();
        while (PhysicalAddress.TryParse(invalidMacAddressString, out _))
        {
            invalidMacAddressString = randomizer.GetString();
        }

        var handlerUnderTest = new PhysicalAddressHandler();
        TestDelegate actionUnderTest = () => handlerUnderTest.Parse(invalidMacAddressString);

        Assert.Throws<FormatException>(actionUnderTest);
    }
    #endregion
}
