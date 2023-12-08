namespace Algorithms.Abstractions;
public interface IRandomProvider
{
    T NextItem<T>(IEnumerable<T> items);
}
