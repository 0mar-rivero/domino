using DominoEngine;
using Players;

namespace MyDominoPwa;

public class Init {
	public Dictionary<int, (string name, IMatcher<int> matchers)> MatchersDic { get; private set; } = new();
	public Dictionary<int, (string name, IDealer<int> dearlers)> DealersDic { get; private set; } = new();
	public Dictionary<int, (string name, IFinisher<int> finisher)> FinishersDic { get; private set; } = new();
	public Dictionary<int, (string name, IGenerator<int> player)> GeneratorDic { get; private set; } = new();
	public Dictionary<int, (string name, IScorer<int> dealer)> ScorerDic { get; private set; } = new();
	public Dictionary<int, (string name, ITurner<int> turner)> TurnerDic { get; private set; } = new();
	public Dictionary<string, Player<int>> PlayerTypesDic { get; private set; } = new();

	public Dictionary<int, (string name, Tournament<int>)> BaseTournamentsDic { get; private set; } = new();

	public Init() {
		SetMatchers();
		SetDealers();
		SetFinishers();
		SetGenerators();
		SetScorers();
		SetTurners();
		SetPlayers();
		SetTournaments();
	}

	private void SetMatchers() {
		var sideMatcher = new SideMatcher<int>();
		var equalMatcher = new EqualMatcher<int>();
		var classicMatcher = sideMatcher.Intersect(equalMatcher);
		var longanaSideMatcher = new LonganaMatcher<int>();
		var longanaMatcher = longanaSideMatcher.Intersect(equalMatcher);
		var relativePrimesMatcher = new RelativesPrimesMatcher();
		var evenOddMatcher = new EvenOddMatcher();
		var teamTokenMatcher = new TeamTokenInvalidMatcher<int>();
		MatchersDic.Add(sideMatcher.GetHashCode(), ("Side Matcher", sideMatcher));
		MatchersDic.Add(equalMatcher.GetHashCode(), ("Equal Matcher", equalMatcher));
		MatchersDic.Add(classicMatcher.GetHashCode(), ("Classic Matcher", classicMatcher));
		MatchersDic.Add(longanaSideMatcher.GetHashCode(), ("Longana Side Matcher", longanaSideMatcher));
		MatchersDic.Add(longanaMatcher.GetHashCode(), ("Longana Matcher", longanaMatcher));
		MatchersDic.Add(relativePrimesMatcher.GetHashCode(), ("Relative Primes Matcher", relativePrimesMatcher));
		MatchersDic.Add(evenOddMatcher.GetHashCode(), ("Even Odd Matcher", evenOddMatcher));
		MatchersDic.Add(teamTokenMatcher.GetHashCode(), ("Team Token Invalid Matcher", teamTokenMatcher));
	}

	private void SetDealers() {
		var classicDealer = new ClassicDealer<int>(55, 10);
		var evenDealer = new EvenDealer(55, 10);
		var oddDealer = new OddDealer(55, 10);
		DealersDic.Add(classicDealer.GetHashCode(), ("Classic Dealer (55) (10)", classicDealer));
		DealersDic.Add(evenDealer.GetHashCode(), ("Even Dealer (55) (10)", evenDealer));
		DealersDic.Add(oddDealer.GetHashCode(), ("Odd Dealer (55) (10)", oddDealer));
	}

	private void SetFinishers() {
		var emptyHandFinisher = new EmptyHandFinisher<int>();
		var allCheckCheckFinisher = new AllCheckFinisher<int>();
		var classicFinisher = emptyHandFinisher.Join(allCheckCheckFinisher);
		var turnCountFinisher = new TurnCountFinisher<int>(20);
		var passCountFinisher = new PassesCountFinisher<int>(10);
		FinishersDic.Add(emptyHandFinisher.GetHashCode(), ("Empty Hand Finisher", emptyHandFinisher));
		FinishersDic.Add(allCheckCheckFinisher.GetHashCode(), ("All Check Finisher", allCheckCheckFinisher));
		FinishersDic.Add(classicFinisher.GetHashCode(), ("Classic Finisher", classicFinisher));
		FinishersDic.Add(turnCountFinisher.GetHashCode(), ("Turn Count Finisher (20)", turnCountFinisher));
		FinishersDic.Add(passCountFinisher.GetHashCode(), ("Passes Count Finisher (10)", passCountFinisher));
	}

	private void SetGenerators() {
		var classicGenerator = new ClassicGenerator();
		var noDoubleGenerator = new NoDoubleGenerator();
		var sumPrimeGenerator = new SumPrimeGenerator();
		var fibonacciGenerator = new FiboGenerator()
;		GeneratorDic.Add(classicGenerator.GetHashCode(), ("Classic Generator", classicGenerator));
		GeneratorDic.Add(noDoubleGenerator.GetHashCode(), ("No Double Generator", noDoubleGenerator));
		GeneratorDic.Add(sumPrimeGenerator.GetHashCode(), ("Sum Prime Generator", sumPrimeGenerator));
		GeneratorDic.Add(fibonacciGenerator.GetHashCode(), ("Fibonacci Generator", fibonacciGenerator));
		
	}

	private void SetScorers() {
		var classicScorer = new ClassicScorer();
		var mod5Scorer = new ModFiveScorer();
		var turnDevidesBoardScorer = new TurnDividesBoardScorer();
		ScorerDic.Add(classicScorer.GetHashCode(), ("Classic Scorer", classicScorer));
		ScorerDic.Add(mod5Scorer.GetHashCode(), ("Mod 5 Scorer", mod5Scorer));
		ScorerDic.Add(turnDevidesBoardScorer.GetHashCode(), ("Turn Divides Board Scorer", turnDevidesBoardScorer));
	}

	private void SetTurners() {
		var classicTurner = new ClassicTurner<int>();
		var nPassesReverseTurner = new NPassesReverseTurner<int>(5);
		var randomTurner = new RandomTurner<int>();
		TurnerDic.Add(classicTurner.GetHashCode(), ("Classic Turner", classicTurner));
		TurnerDic.Add(nPassesReverseTurner.GetHashCode(), ("NPasses Reverse Turner (5)", nPassesReverseTurner));
		TurnerDic.Add(randomTurner.GetHashCode(), ("Random Turner", randomTurner));
	}

	private void SetPlayers() {
		PlayerTypesDic.Add("Botagorda", new Botagorda<int>("BaseBotagorda"));
		PlayerTypesDic.Add("Smart Player", new SmartPlayer<int>("Smart"));
		PlayerTypesDic.Add("Random Player", new RandomPlayer<int>("Random"));
		PlayerTypesDic.Add("Carrier", new CarrierPlayer<int>("Carrier"));
		PlayerTypesDic.Add("Disabler", new DisablerPlayer<int>("Disabler"));
		PlayerTypesDic.Add("Support", new SupportPlayer<int>("Support"));
	}

	private void SetTournaments() {
		Tournament<int> tournament = new NGamesTournament<int>(1);
		BaseTournamentsDic.Add(tournament.GetHashCode(), ("NGames Tournament (1)", tournament));
		tournament = new AllVsAllTournament<int>();
		BaseTournamentsDic.Add(tournament.GetHashCode(), ("All Vs All Tournament", tournament));
		tournament = new DirichletTournament<int>(2);
		BaseTournamentsDic.Add(tournament.GetHashCode(), ("Dirichlet Tournament (2)", tournament));
		tournament = new PlayOffTournament<int>();
		BaseTournamentsDic.Add(tournament.GetHashCode(), ("Play Off Tournament", tournament));
	}
}
