// See https://aka.ms/new-console-template for more information
using Algorithms.Common;
using Algorithms.RSA;
using Demo;

var rsaService = new RSAService();
var rsaKeysGenerator = new RSAKeysGenerator();
var randomProvider = new RandomProvider();
var objectToByteArrayTransformer = new ObjectToByteArrayTransformer();

var factory = new DemoDataFactory(rsaKeysGenerator);
var candidates = factory.CreateCandidates();
var voters = factory.CreateVoters();
var commission = factory.CreateCentralElectionCommission(candidates, voters);

var printer = new ModellingPrinter(rsaService, rsaKeysGenerator, randomProvider, objectToByteArrayTransformer);

printer.PrintUsualVoting(commission, factory.CreateVotersWithCandidateIds(voters.SkipLast(2).ToList()));
printer.PrintVotingWithIncorrectBallot(commission);
printer.PrintVotingWithDoubleBallotCase1(commission, voters[^1]);
printer.PrintVotingWithDoubleBallotCase2(commission, 2, voters[^2]);

printer.PrintVotingResults(commission);

printer.PrintVotingAfterCompletion(commission, randomProvider.NextItem(candidates).Id, randomProvider.NextItem(voters));
