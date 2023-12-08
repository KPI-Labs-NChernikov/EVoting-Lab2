namespace Modelling;
public sealed class SignedBallot
{
    public Ballot Ballot { get; }

    public byte[] Signature { get; }

    public SignedBallot(Ballot ballot, byte[] signature)
    {
        Ballot = ballot;
        Signature = signature;
    }
}
