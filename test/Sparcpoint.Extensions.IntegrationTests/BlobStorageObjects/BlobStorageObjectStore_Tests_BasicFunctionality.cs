using Azure.Data.Tables;
using Microsoft.Extensions.DependencyInjection;


namespace Sparcpoint.Extensions.IntegrationTests;

[Collection("Blob Storage")]
public class BlobStorageObjectStore_Tests_BasicFunctionality
{
    private readonly BlobStorageFixture _Fixture;

    public BlobStorageObjectStore_Tests_BasicFunctionality(BlobStorageFixture fixture)
    {
        _Fixture = fixture;
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("Justin Coulston")]
    [InlineData("/My name is /*&%^$#")]
    public void Can_encode_and_decode_blob_tag_values(string? value)
    {
        var encoded = value.EncodeBlobTagValue();
        var decoded = value.DecodeBlobTagValue();

        Assert.Equal(value, decoded);
    }

    [Fact]
    public async Task Can_save_and_retrieve_all_field_types()
    {
        var service = _Fixture.Provider.GetRequiredService<IObjectStore<AllPropertyTypesObject>>();

        var id = ScopePath.Parse("/objects/samples/sample_123456789");
        var expected = new AllPropertyTypesObject
        {
            Id = id,
            Name = "SAMPE 123456798"
        };
        await service.UpsertAsync(new[] { expected });
        var actual = await service.FindAsync(id);

        Assert.NotNull(actual);
        Assert.Equal(expected.ByteValue, actual.ByteValue);
        Assert.Equal(expected.ShortValue, actual.ShortValue);
        Assert.Equal(expected.IntValue, actual.IntValue);
        Assert.Equal(expected.LongValue, actual.LongValue);
        Assert.Equal(expected.FloatValue, actual.FloatValue);
        Assert.Equal(expected.DoubleValue, actual.DoubleValue);
        Assert.Equal(expected.DecimalValue, actual.DecimalValue);
        Assert.Equal(expected.DateTimeValue, actual.DateTimeValue);
        Assert.Equal(expected.DateOnlyValue, actual.DateOnlyValue);
        Assert.Equal(expected.TimeOnlyValue, actual.TimeOnlyValue);
        Assert.Equal(expected.TimeSpanValue, actual.TimeSpanValue);
        Assert.Equal(expected.StringValue, actual.StringValue);

        Assert.Equal(expected.ByteValueNullable, actual.ByteValueNullable);
        Assert.Equal(expected.ShortValueNullable, actual.ShortValueNullable);
        Assert.Equal(expected.IntValueNullable, actual.IntValueNullable);
        Assert.Equal(expected.LongValueNullable, actual.LongValueNullable);
        Assert.Equal(expected.FloatValueNullable, actual.FloatValueNullable);
        Assert.Equal(expected.DoubleValueNullable, actual.DoubleValueNullable);
        Assert.Equal(expected.DecimalValueNullable, actual.DecimalValueNullable);
        Assert.Equal(expected.DateTimeValueNullable, actual.DateTimeValueNullable);
        Assert.Equal(expected.DateOnlyValueNullable, actual.DateOnlyValueNullable);
        Assert.Equal(expected.TimeOnlyValueNullable, actual.TimeOnlyValueNullable);
        Assert.Equal(expected.TimeSpanValueNullable, actual.TimeSpanValueNullable);
        Assert.Equal(expected.StringValueNullable, actual.StringValueNullable);

        Assert.Equal(expected.IntArray, actual.IntArray);
        Assert.Equal(expected.StringArray, actual.StringArray);
        Assert.Equal(expected.ComplexObject, actual.ComplexObject);
        Assert.Equal(expected.ComplexObjectArray, actual.ComplexObjectArray);

        Assert.Equal(expected.IntArrayNullable, actual.IntArrayNullable);
        Assert.Equal(expected.StringArrayNullable, actual.StringArrayNullable);
        Assert.Equal(expected.IntArrayNullableInner, actual.IntArrayNullableInner);
        Assert.Equal(expected.StringArrayNullableInner, actual.StringArrayNullableInner);

        // Delete
        await service.DeleteAsync(new[] { id });
        actual = await service.FindAsync(id);
        Assert.Null(actual);
    }
}

internal record AllPropertyTypesObject : SparcpointObject
{
    public bool BoolValue { get; set; } = true;
    public byte ByteValue { get; set; } = 1;
    public short ShortValue { get; set; } = 2;
    public int IntValue { get; set; } = 3;
    public long LongValue { get; set; } = 4;
    public float FloatValue { get; set; } = 5;
    public double DoubleValue { get; set; } = 6;
    public decimal DecimalValue { get; set; } = 7;
    public DateTime DateTimeValue { get; set; } = new DateTime(2010, 2, 15, 13, 10, 11);
    public DateOnly DateOnlyValue { get; set; } = new DateOnly(2011, 3, 16);
    public TimeOnly TimeOnlyValue { get; set; } = new TimeOnly(14, 11, 12);
    public TimeSpan TimeSpanValue { get; set; } = new TimeSpan(15, 12, 13);
    public string StringValue { get; set; } = "A String Value!";

    public bool? BoolValueNullable { get; set; } = true;
    public byte? ByteValueNullable { get; set; } = 8;
    public short? ShortValueNullable { get; set; } = 9;
    public int? IntValueNullable { get; set; } = 10;
    public long? LongValueNullable { get; set; } = 11;
    public float? FloatValueNullable { get; set; } = 12;
    public double? DoubleValueNullable { get; set; } = 13;
    public decimal? DecimalValueNullable { get; set; } = 14;
    public DateTime? DateTimeValueNullable { get; set; } = new DateTime(2009, 2, 15, 13, 10, 11);
    public DateOnly? DateOnlyValueNullable { get; set; } = new DateOnly(2005, 3, 16);
    public TimeOnly? TimeOnlyValueNullable { get; set; } = new TimeOnly(4, 11, 12);
    public TimeSpan? TimeSpanValueNullable { get; set; } = new TimeSpan(5, 12, 13);
    public string? StringValueNullable { get; set; } = "A String Value 2!";

    public int? IntValueNullableWithNull { get; set; } = null;

    public int[] IntArray { get; set; } = [3, 4, 5];
    public string[] StringArray { get; set; } = ["A", "C", "E"];

    public int[]? IntArrayNullable { get; set; } = [3, 4, 5];
    public string[]? StringArrayNullable { get; set; } = ["A", "C", "E"];

    public int?[] IntArrayNullableInner { get; set; } = [3, 4, 5];
    public string?[] StringArrayNullableInner { get; set; } = ["A", "C", "E"];

    public ComplexObject ComplexObject { get; set; } = new();
    public ComplexObject[] ComplexObjectArray { get; set; } = [new ComplexObject(), new ComplexObject()];
    public ComplexObject? ComplexObjectNullable { get; set; } = null;
}

internal record ComplexObject
{
    public string ValueA = "JAC";
    public string City = "CityName";
    public string ValueB = "B";
    public int IntValue = 789;
}
