using Moq;
using NUnit.Framework.Internal;
using SmartHome.Server.Data.Converters.TypeHandlers;
using System.Data;
using System.Net;
using UnitTests;

namespace SmartHome.UnitTests.Server.Data.Converters.TypeHandlers;

[Category("UnitTest")]
[TestOf(typeof(IPAddressHandler))]
[Author("Jakub Miodunka")]
public class IPAddressHandlerTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        TestDelegate actionUnderTest = () => new IPAddressHandler();

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void SettingParameterImpossibleUsingNullReferenceAsParameter()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress ipAddress = randomizer.NextIpAddress();

        var handlerUnderTest = new IPAddressHandler();
        TestDelegate actionUnderTest = () => handlerUnderTest.SetValue(null!, ipAddress);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void ParameterTypeSetCorrectly()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress ipAddress = randomizer.NextIpAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new IPAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, ipAddress);

        dbDataParameterMock.VerifySet(mock => mock.DbType = DbType.AnsiString);
    }

    [Test]
    public void ParameterSizeSetCorrectly()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress ipAddress = randomizer.NextIpAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new IPAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, ipAddress);

        dbDataParameterMock.VerifySet(mock => mock.Size = 39);
    }

    [Test]
    public void ParameterValueSetCorrectlyIfIpAddressIsNotNullReference()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress ipAddress = randomizer.NextIpAddress();
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new IPAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, ipAddress);

        dbDataParameterMock.VerifySet(mock => mock.Value = ipAddress.ToString());
    }

    [Test]
    public void ParameterValueSetCorrectlyIfIpAddressIsNullReference()
    {
        var dbDataParameterMock = new Mock<IDbDataParameter>();

        var handlerUnderTest = new IPAddressHandler();
        handlerUnderTest.SetValue(dbDataParameterMock.Object, null);

        dbDataParameterMock.VerifySet(mock => mock.Value = DBNull.Value);
    }

    [Test]
    public void ResultOfParsingNullReferenceIsNullReference()
    {
        var handlerUnderTest = new IPAddressHandler();
        IPAddress? parsedIpAddress = handlerUnderTest.Parse(null);

        Assert.That(parsedIpAddress, Is.Null);
    }

    [Test]
    public void ResultOfParsingNonStringReferenceIsNullReference()
    {
        var handlerUnderTest = new IPAddressHandler();
        IPAddress? parsedIpAddress = handlerUnderTest.Parse(new object());

        Assert.That(parsedIpAddress, Is.Null);
    }

    [Test]
    public void ResultOfParsingValidIpAddressStringIsCorrespondingIpAddressObject()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        IPAddress inputIpAddress = randomizer.NextIpAddress();

        var handlerUnderTest = new IPAddressHandler();
        IPAddress? parsedIpAddress = handlerUnderTest.Parse(inputIpAddress.ToString());

        Assert.That(parsedIpAddress, Is.EqualTo(inputIpAddress));
    }

    [Test]
    public void ParsingImpossibleUsingInvalidIpAddressString()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;
        
        string invalidIpAddressString = randomizer.GetString();
        while (IPAddress.TryParse(invalidIpAddressString, out _))
        {
            invalidIpAddressString = randomizer.GetString();
        }

        var handlerUnderTest = new IPAddressHandler();
        TestDelegate actionUnderTest = () => handlerUnderTest.Parse(invalidIpAddressString);

        Assert.Throws<FormatException>(actionUnderTest);
    }
    #endregion
}
