using System.Collections;

namespace Modelling;
public sealed class BallotBatchesCollection : IEnumerable<BallotBatch>
{
    private readonly IReadOnlyCollection<BallotBatch> _batches;

    public byte[] MaskMultiplier { get; private set; }

    public BallotBatchesCollection(IReadOnlyCollection<BallotBatch> batches, byte[] maskMultiplier)
    {
        _batches = batches;
        MaskMultiplier = maskMultiplier;
    }

    public IEnumerator<BallotBatch> GetEnumerator()
    {
        return _batches.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
