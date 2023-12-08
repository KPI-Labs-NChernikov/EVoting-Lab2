namespace Algorithms.Abstractions;
public interface IObjectToByteArrayTransformer
{
    byte[] Transform(object obj);

    T? ReverseTransform<T>(byte[] data); 
}
