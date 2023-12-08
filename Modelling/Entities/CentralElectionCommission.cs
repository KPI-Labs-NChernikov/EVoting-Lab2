using Algorithms.Abstractions;
using FluentResults;
using Modelling.Entities;
using Modelling.Models;
using System.Security.Cryptography;

namespace Modelling;
public sealed class CentralElectionCommission
{
    private readonly Dictionary<int, Candidate> _candidates = new();
    private readonly Dictionary<Guid, Voter> _voters = new();

    public VotingResults VotingResults { get; }

    private readonly Dictionary<Guid, VoterStatus> _votersStatuses = new();

    public RSAParameters PublicKey { get; }
    private readonly RSAParameters _privateKey;

    public bool IsVotingCompleted { get; private set; }

    public CentralElectionCommission(IEnumerable<Candidate> candidates, IEnumerable<Voter> voters, IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> keysGenerator)
    {
        foreach (var candidate in candidates)
        {
            _candidates.Add(candidate.Id, candidate);
        }

        foreach (var voter in voters)
        {
            _voters.Add(voter.Id, voter);
            _votersStatuses.Add(voter.Id, VoterStatus.NotAttended);
        }

        (PublicKey, _privateKey) = keysGenerator.GenerateKeys();
        VotingResults = new();
    }

    public Result<IEnumerable<byte[]>> AcceptBatches()
    {

    }

    public Result<int> AcceptVote()
    {

    }

    public void CompleteVoting()
    {
        IsVotingCompleted = true;
    }
}
