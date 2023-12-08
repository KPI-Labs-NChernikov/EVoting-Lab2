using Modelling.Entities;

namespace Modelling.Models;
public sealed class VotingResults
{
    public ICollection<CandidateVotingResults> CandidatesResults { get; } = new List<CandidateVotingResults>();

    public ICollection<VoterResults> VoterResults { get; } = new List<VoterResults>();
}
