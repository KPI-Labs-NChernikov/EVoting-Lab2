using Algorithms.Abstractions;
using System.Text.Json;
using System.Text;

namespace Algorithms.Common;
public sealed class ObjectToByteArrayTransformer : IObjectToByteArrayTransformer
{
    public byte[] Transform(object obj)
    {
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(obj));
    }

    public T? ReverseTransform<T>(byte[] data)
    {
        return JsonSerializer.Deserialize<T>(Encoding.UTF8.GetString(data));
    }
}
