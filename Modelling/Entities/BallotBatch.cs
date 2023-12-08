namespace Modelling;
public sealed class BallotBatch
{
    public IReadOnlyCollection<byte[]> MaskedBallots { get; }

    public BallotBatch(IReadOnlyCollection<byte[]> maskedBallots)
    {
        MaskedBallots = maskedBallots;
    }
}
