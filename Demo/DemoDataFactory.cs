using Algorithms.Abstractions;
using Modelling;
using System.Security.Cryptography;

namespace Demo;
public sealed class DemoDataFactory
{
    private readonly IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> _rsaKeysGenerator;

    public DemoDataFactory(IMaskedAsymmetricAlgorithmKeysGenerator<RSAParameters> rsaKeysGenerator)
    {
        _rsaKeysGenerator = rsaKeysGenerator;
    }

    public IReadOnlyList<Candidate> CreateCandidates()
    {
        return new List<Candidate>
        {
            new Candidate(1, "Ishaan Allison"),
            new Candidate(2, "Oliver Mendez"),
            new Candidate(3, "Naomi Winter"),
        };
    }

    public IReadOnlyList<Voter> CreateVoters()
    {
        return new List<Voter>
        {
            new Voter(Guid.NewGuid(), true), 
            new Voter(Guid.NewGuid(), false),      // Not capable.
            new Voter(Guid.NewGuid(), true),
            new Voter(Guid.NewGuid(), true),
            new Voter(Guid.NewGuid(), true),
            new Voter(Guid.NewGuid(), true),
            new Voter(Guid.NewGuid(), true),

            new Voter(Guid.NewGuid(), true),
            new Voter(Guid.NewGuid(), true)
        };
    }

    public CentralElectionCommission CreateCentralElectionCommission(IReadOnlyList<Candidate> candidates, IReadOnlyList<Voter> voters)
    {
        return new CentralElectionCommission(candidates, voters, _rsaKeysGenerator);
    }

    public Dictionary<Voter, int> CreateVotersWithCandidateIds(IReadOnlyList<Voter> voters)
    {
        var dictionary = new Dictionary<Voter, int>();
        for (var i = 0; i < voters.Count; i++)
        {
            var candidateId = (i % 7 + 1) switch
            {
                1 => 1,
                2 => 1,

                3 => 2,
                4 => 1,
                5 => 3,
                6 => 3,
                7 => 3,

                _ => throw new InvalidOperationException("Negative and zero voters' ids are not supported in this method.")
            };
            dictionary.Add(voters[i], candidateId);
        }
        return dictionary;
    }
}
