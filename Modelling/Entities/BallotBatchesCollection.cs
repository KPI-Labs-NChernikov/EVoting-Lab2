namespace Modelling;
public sealed class BallotBatchesCollection
{
    public IReadOnlyCollection<BallotBatch> Batches { get; private set; }

    public byte[] MaskMultiplier { get; private set; }

    public BallotBatchesCollection(IReadOnlyCollection<BallotBatch> batches, byte[] maskMultiplier)
    {
        Batches = batches;
        MaskMultiplier = maskMultiplier;
    }
}
