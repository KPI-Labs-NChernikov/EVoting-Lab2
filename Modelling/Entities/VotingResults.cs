using Modelling.Entities;

namespace Modelling.Models;
public sealed class VotingResults
{
    public SortedDictionary<int, CandidateVotingResults> CandidatesResults { get; } = new ();

    public ICollection<VoterResults> VotersResults { get; } = new List<VoterResults>();
}
