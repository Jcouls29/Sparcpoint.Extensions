using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sparcpoint.Extensions.Tests.ObjectMapper;

public class JsonSerializerDiscovery
{


    [Fact]
    public void BasicTypeSerialization()
    {
        Assert.Equal("102", _S((byte)102));
        Assert.Equal("102", _S((int)102));
        Assert.Equal("102", _S((uint)102));
        Assert.Equal("102", _S((ulong)102));
        Assert.Equal("102", _S((long)102));
        Assert.Equal("56.3", _S((float)56.30));
        Assert.Equal("56.3", _S((double)56.30));
        Assert.Equal("9865.3568", _S(9865.3568M));
        Assert.Equal("\"Justin Coulston\"", _S("Justin Coulston"));
        Assert.Equal("null", _S(null));
        Assert.Equal("\"2020-02-16T03:15:06\"", _S(new DateTime(2020, 2, 16, 3, 15, 6)));
        Assert.Equal("\"03:15:06\"", _S(new TimeSpan(3, 15, 6)));

        // Nullable
        Assert.Equal("102", _S((Nullable<byte>)102));
        Assert.Equal("102", _S((Nullable<int>)102));
        Assert.Equal("102", _S((Nullable<uint>)102));
        Assert.Equal("102", _S((Nullable<ulong>)102));
        Assert.Equal("102", _S((Nullable<long>)102));
        Assert.Equal("56.3", _S((Nullable<float>)56.30));
        Assert.Equal("56.3", _S((Nullable<double>)56.30));
        Assert.Equal("null", _S((Nullable<int>)null));
    }

    private string _S(object? obj)
        => JsonSerializer.Serialize(obj);
}
