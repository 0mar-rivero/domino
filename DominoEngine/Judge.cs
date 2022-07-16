namespace DominoEngine;

public class Judge<T> {
	private readonly IGenerator<T> _generator; 
	private readonly IDealer<T> _dealer; 
	private readonly ITurner<T> _turner; 
	private readonly IMatcher<T> _matcher; 
	private readonly IScorer<T> _scorer; 
	private readonly IFinisher<T> _finisher; 

	public Judge(IGenerator<T> generator, IDealer<T> dealer, ITurner<T> turner, IMatcher<T> matcher, IScorer<T> scorer,
		IFinisher<T> finisher) {
		_generator = generator; 
		_dealer = dealer; 
		_turner = turner; 
		_matcher = matcher; 
		_scorer = scorer; 
		_finisher = finisher; 
	}

	/// <summary>
	/// Inicializa el juego, repartiendole las fichas a los jugadores
	/// </summary>
	/// <param name="partida"></param>
    public void Start(Partida<T> partida) {
		// Inicializa la partida, le reparte las manos a los players
		foreach (var (player, hand) in _dealer.Deal(partida, _generator.Generate()))
			partida.SetHand(player.SetHand(hand), hand);
	}

	/// <summary>
	/// Devuelve los players a medida que se desarrolla el juego
	/// </summary>
	/// <param name="partida"></param>
	/// <returns></returns>
	internal IEnumerable<Player<T>> Play(Partida<T> partida) {
		// Mientras no se pueda salir no entra al foreach
		foreach (var (i, player) in _turner.Players(partida).Enumerate().SkipWhile(pair => !Salir(partida, pair.item))) {
			if (i is 0) {
				// Si es el primer turno, devuelve al player y pasa a la siguiente iteracion
				yield return player; 
				continue; 
			}
			if (_finisher.GameOver(partida!)) // Si se activa la condicion de finalizacion, termino el juego
				yield break; 

			var validMoves = GenValidMoves(partida, player).ToHashSet(); // Se generan las jugadas validas
			var move = player.Play(validMoves, partida.PassesInfo,partida.Board.ToList(), partida.InHand,
				move => _scorer.Scorer(partida!, move), partida.Partnership); // El player juega
			if (!validMoves.Contains(move)) move = validMoves.FirstOrDefault(); // Si no es valido, se selecciona jugada valida
			partida.AddMove(move!); // Se agrega la jugada a la partida
			partida.AddValidsTurns(_matcher.ValidsTurns(partida, Partida<T>.PlayerId(player))); // Se agrega la jugada a la lista de jugadas validas
			if (!move!.Check) partida.RemoveFromHand(player, move.Token!); // Si no es un pase, se quita de la mano
			yield return player; // Se devuelve al player
		}
	}

	/// <summary>
	/// Recibe un jugador y trata de salir con el
	/// </summary>
	/// <param name="partida"></param>
	/// <param name="player"></param>
	/// <returns></returns>
	private bool Salir(Partida<T> partida, Player<T> player) {
		var validMoves = GenSalidas(partida, player).ToHashSet(); // Se generan las salidas validas
		if (validMoves.IsEmpty()) return false; // Si no hay salidas validas, devuelve true
		var move = player.Play(validMoves, partida.PassesInfo,partida!.Board.ToList(), partida.InHand, 
			x => _scorer.Scorer(partida, x), partida.Partnership); // El player juega
		if (!validMoves.Contains(move)) move = validMoves.FirstOrDefault(); // Si no es valido, se selecciona jugada valida
		if (!move!.Check) partida!.RemoveFromHand(player, move.Token!); // Si no es un pase, se quita de la mano
		partida!.AddMove(move!); // Se agrega la jugada a la partida
		return true; 
	}

	/// <summary>
	/// Devuelve todas las jugadas posibles que se pueden hacerlo sobre el tablero
	/// </summary>
	/// <param name="partida"></param>
	/// <param name="player"></param>
	/// <returns></returns>
	private static IEnumerable<Move<T>> GenMoves(Partida<T> partida, Player<T> player) {
		var playerId = Partida<T>.PlayerId(player); // Se obtiene el id del player
		yield return new Move<T>(playerId); // Se devuelve un pase
		foreach (var (head, tail) in partida.Hand(player)) {
			// Se devuelven las jugadas que apuntan a la salida
			yield return new Move<T>(playerId, false, -1, head, tail); 
			yield return new Move<T>(playerId, false, -1, tail, head); 
			foreach (var (i, move) in partida.Board.Enumerate().Where(t => !t.Item2.Check)) {
				// Se devuelven las jugadas que apuntan al resto del tablero
				yield return new Move<T>(playerId, false, i, head, tail);
				yield return new Move<T>(playerId, false, i, tail, head); 
			}
		}
	}

	/// <summary>
	/// Devuelve el ienumerable de movimientos validsos flitrado por el matcher
	/// </summary>
	/// <param name="partida"></param>
	/// <param name="player"></param>
	/// <returns></returns>
	private IEnumerable<Move<T>> GenValidMoves(Partida<T> partida, Player<T> player) =>
		_matcher.CanMatch(partida!, GenMoves(partida, player), _scorer.TokenScorer); // Se devuelven las jugadas validas

	/// <summary>
	/// Genera salidas validas, con el turno apuntando a -1
	/// </summary>
	/// <param name="partida"></param>
	/// <param name="player"></param>
	/// <returns></returns>
	private IEnumerable<Move<T>> GenSalidas(Partida<T> partida, Player<T> player) {
		var id = Partida<T>.PlayerId(player); // Se obtiene el id del player
		foreach (var move in _matcher.CanMatch(partida, partida.Hand(player).
				Select(x => new Move<T>(id, false, -1, x.Head, x.Tail)), _scorer.TokenScorer))
			yield return move; // Se devuelven las salidas validas
	}

	/// <summary>
	/// Devuelve el ganador del juego segun la el scorer en un momento determinado del juego
	/// </summary>
	/// <param name="partida"></param>
	/// <returns></returns>
	internal IEnumerable<Team<T>> Winner(Partida<T> partida) => _scorer.Winner(partida); // Se devuelve el ganador
}


public class ClassicJudge : Judge<int> {
    public ClassicJudge() : base(new ClassicGenerator(), new ClassicDealer<int>(55, 10), 
		new ClassicTurner<int>(), new SideMatcher<int>().Intersect(new EqualMatcher<int>()), 
		new ClassicScorer(), new TurnCountFinisher<int>(5)) { }
}
