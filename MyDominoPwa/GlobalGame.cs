using DominoEngine;

namespace MyDominoPwa;

public class GlobalState<TDominoType> {
	public GameState<int>? GlobalGameState { get; set; }
	public Game<int> GlobalGame { get; set; } = new();
	public Tournament<int>? GLobalTournament { get; set; }
	public bool Visible { get; internal set; } = false;

	public IMatcher<TDominoType>? Matcher { get; set; }
	public IScorer<TDominoType>? Scorer { get; set; }
	public ITurner<TDominoType>? Turner { get; set; }
	public IDealer<TDominoType>? Dealer { get; set; }
	public IGenerator<TDominoType>? Generator { get; set; }
	public IFinisher<TDominoType>? Finisher {get; set; }

	public Dictionary<int, Team<int>> Teams { get; set; } = new();
	public Judge<int>? Judge { get; set; }
}