namespace Modelling.Models;
public sealed class CandidateVotingResults
{
    public Candidate Candidate { get; }

    public int Votes { get; set; }

    public CandidateVotingResults(Candidate candidate)
    {
        Candidate = candidate;
    }
}
