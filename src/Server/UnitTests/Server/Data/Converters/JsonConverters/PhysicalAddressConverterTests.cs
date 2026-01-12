using NUnit.Framework.Internal;
using SmartHome.Server.Data.Converters.JsonConverters;
using System.Net.NetworkInformation;
using System.Text.Json;

namespace SmartHome.UnitTests.Server.Data.Converters.JsonConverters;

[Category("UnitTest")]
[TestOf(typeof(PhysicalAddressConverter))]
[Author("Jakub Miodunka")]
public sealed class PhysicalAddressConverterTests
{
    #region Test cases
    [Test]
    public void InstantiationPossible()
    {
        TestDelegate actionUnderTest = () => new PhysicalAddressConverter();

        Assert.DoesNotThrow(actionUnderTest);
    }

    [Test]
    public void ReadingImpossibleUsingNullReferenceAsTypeToConvert()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();
        var jsonSerializerOptions = new JsonSerializerOptions();

        var converterUnderTest = new PhysicalAddressConverter();

        TestDelegate actionUnderTest = () =>
        {
            Utf8JsonReader jsonReader = TestDataGenerator.CreateJsonReader(macAddress.ToString());
            converterUnderTest.Read(ref jsonReader, null!, jsonSerializerOptions);
        };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void ReadingImpossibleUsingNullReferenceAsOptions()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress macAddress = randomizer.NextMacAddress();

        var converterUnderTest = new PhysicalAddressConverter();

        TestDelegate actionUnderTest = () =>
        {
            Utf8JsonReader jsonReader = TestDataGenerator.CreateJsonReader(macAddress.ToString());
            converterUnderTest.Read(ref jsonReader, macAddress.GetType(), null!);
        };

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void WritingImpossibleUsingNullReferenceAsValue()
    {
        using var stream = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(stream);
        var jsonSerializerOptions = new JsonSerializerOptions();

        var converterUnderTest = new PhysicalAddressConverter();

        TestDelegate actionUnderTest = () => converterUnderTest.Write(jsonWriter, null!, jsonSerializerOptions);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void WritingImpossibleUsingNullReferenceAsOptions()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        using var stream = new MemoryStream();
        using var jsonWriter = new Utf8JsonWriter(stream);
        var macAddress = randomizer.NextMacAddress();

        var converterUnderTest = new PhysicalAddressConverter();

        TestDelegate actionUnderTest = () => converterUnderTest.Write(jsonWriter, macAddress, null!);

        Assert.Throws<ArgumentNullException>(actionUnderTest);
    }

    [Test]
    public void ResultOfDeserializationOfJsonNullValueIsNullReference()
    {
        const string JsonNullValue = "null";

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());

        PhysicalAddress? deserializedMacAddress = 
            JsonSerializer.Deserialize<PhysicalAddress>(JsonNullValue, jsonSerializerOptions);

        Assert.That(deserializedMacAddress, Is.Null);
    }

    [Test]
    public void DeserializationImpossibleUsingInvalidMacAddressString()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        string invalidMacAddressString = randomizer.GetString();
        while (PhysicalAddress.TryParse(invalidMacAddressString, out _))
        {
            invalidMacAddressString = randomizer.GetString();
        }

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());

        TestDelegate actionUnderTest = () =>
            JsonSerializer.Deserialize<PhysicalAddress>(invalidMacAddressString, jsonSerializerOptions);

        Assert.Throws<JsonException>(actionUnderTest);
    }

    [Test]
    public void SerializationIsTransparent()
    {
        Randomizer randomizer = TestContext.CurrentContext.Random;

        PhysicalAddress originalMacAddress = randomizer.NextMacAddress();

        var jsonSerializerOptions = new JsonSerializerOptions();
        jsonSerializerOptions.Converters.Add(new PhysicalAddressConverter());

        string jsonString =
            JsonSerializer.Serialize(originalMacAddress, jsonSerializerOptions);

        PhysicalAddress? deserializedMacAddress =
            JsonSerializer.Deserialize<PhysicalAddress>(jsonString, jsonSerializerOptions);

        Assert.That(deserializedMacAddress, Is.EqualTo(originalMacAddress));
    }
    #endregion
}
