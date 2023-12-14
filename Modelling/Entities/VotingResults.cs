using Modelling.Entities;

namespace Modelling.Models;
public sealed class VotingResults
{
    public SortedDictionary<int, CandidateVotingResults> CandidatesResults { get; } = [];

    public ICollection<VoterResults> VotersResults { get; } = new List<VoterResults>();
}
