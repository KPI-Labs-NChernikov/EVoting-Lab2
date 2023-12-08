using Modelling.Models;

namespace Modelling.Extensions;
public static class LinqExtensions
{
    public static IOrderedEnumerable<CandidateVotingResults> OrderByVotes(this IEnumerable<CandidateVotingResults> candidatesVotingResults)
        => candidatesVotingResults.OrderByDescending(c => c.Votes);
}
