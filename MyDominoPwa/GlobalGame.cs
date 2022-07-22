using DominoEngine;

namespace MyDominoPwa;

public class GlobalState<TDominoType> {
	public GameState<int>? State { get; set; }
	public Game<int> Game1 { get; set; } = new();

	public IEnumerator<GameState<int>?>? GameEnumerator { get; set; }
	
	public IEnumerator<Game<int>>? Enumerator { get; set; }
	public Tournament<int>? Tournament { get; set; }
	public bool Started { get; internal set; }
	public bool Created { get; internal set; }

	public bool Over { get; internal set; }
	public IMatcher<TDominoType>? Matcher { get; set; }
	public IScorer<TDominoType>? Scorer { get; set; }
	public ITurner<TDominoType>? Turner { get; set; }
	public IDealer<TDominoType>? Dealer { get; set; }
	public IGenerator<TDominoType>? Generator { get; set; }
	public IFinisher<TDominoType>? Finisher {get; set; }

	public Dictionary<int, Team<int>> Teams { get; set; } = new();
	public Judge<int>? Judge { get; set; }

	public void Reset() {
		Created = false;
		Over = false;
		Started = false;
	}
}