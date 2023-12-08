namespace Modelling.Entities;
public sealed class VoterResults
{
    public Guid VoterId { get; }
    public int BallotId { get; }
    public int CandidateId { get; }

    public VoterResults(Guid voterId, int ballotId, int candidateId)
    {
        VoterId = voterId;
        BallotId = ballotId;
        CandidateId = candidateId;
    }
}
