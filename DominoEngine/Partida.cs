namespace DominoEngine;

public class Partida<T> {
	private readonly Board<T> _board = new(); 
	private readonly IEnumerable<Team<T>> _teams; // Los equipos que participan en la partida
	private readonly Dictionary<int, IEnumerable<int>> _validsTurns = new(); 

	public Partida(IEnumerable<Team<T>> teams) {
		_teams = teams;
	}

	/// <summary>
	/// Añade movimientos al tablero
	/// </summary>
	/// <param name="move"></param>
	internal void AddMove(Move<T> move) => _board.Add(move); 

	/// <summary>
	/// Remueve fichas de las manos de los jugadores
	/// </summary>
	/// <param name="player"></param>
	/// <param name="token"></param>
	/// <returns></returns>
	internal bool RemoveFromHand(Player<T> player, Token<T> token) =>
		Hands.ContainsKey(player) && Hands[player].Remove(token); 

	/// <summary>
	/// Actualiza los turnos validos de la partida, guarda el registro de jugadas validas por turno
	/// </summary>
	/// <param name="validsTurns"></param>
	internal void AddValidsTurns(IEnumerable<int> validsTurns) => _validsTurns.Add(_validsTurns.Count, validsTurns); 

	/// <summary>
	/// Para un turno devuelve las jugadas validas en ese momento
	/// </summary>
	/// <param name="turn"></param>
	/// <returns></returns>
	internal IEnumerable<int> PassesInfo(int turn) => _validsTurns[turn]; 

	/// <summary>
	/// Devuelve una copia de la mano del player
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	internal IEnumerable<Token<T>> Hand(Player<T> player) => Hands[player].Clone(); 

	/// <summary>
	/// Devuelve el id del jugador
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	internal static int PlayerId(Player<T> player) => player.PlayerId; 

	/// <summary>
	/// Para un player, devuelve cuantas fichas tiene en las manoaq
	/// </summary>
	/// <param name="hash"></param>
	/// <returns></returns>
	internal int InHand(int hash) {
		if (Players().Where(x => x.PlayerId == hash).IsEmpty()) return -1; 
		return Hands[Players().FirstOrDefault(x => x.PlayerId == hash)!].Count; 
	} 

	/// <summary>
	/// Devuelve true si dos players estan en el mismo equipo
	/// </summary>
	/// <param name="pId1"></param>
	/// <param name="pId2"></param>
	/// <returns></returns>
	internal bool Partnership(int pId1, int pId2) => TeamOf(pId1).Equals(TeamOf(pId2)); 

	/// <summary>
	/// Devuelve el equipo de un player teniendo su instancia
	/// </summary>
	/// <param name="player"></param>
	/// <returns></returns>
	internal Team<T> TeamOf(Player<T> player) => _teams.FirstOrDefault(x => x!.Contains(player), default)!; 

	/// <summary>
	/// Devuelve el equipo de un player teniendo su iD
	/// </summary>
	/// <param name="playerId"></param>
	/// <returns></returns>
	internal Team<T> TeamOf(int playerId) => TeamOf(Players().FirstOrDefault(x => x.PlayerId == playerId)!); 

	/// <summary>
	/// Devuelve el tablero de la partida
	/// </summary>
	internal Board<T> Board => _board; 

	/// <summary>
	/// Guarda las manos en el diccionario de manos 
	/// </summary>
	/// <param name="player"></param>
	/// <param name="hand"></param>
	internal void SetHand(Player<T> player, Hand<T> hand) => Hands.Add(player, hand.Clone()); 

	/// <summary>
	/// Devuelve todos los players involucrados en la partida
	/// </summary>
	/// <returns></returns>
	internal IEnumerable<Player<T>> Players() => _teams.SelectMany(team => team); 

	/// <summary>
	/// Devuelve los teams involucrados en la partida
	/// </summary>
	/// <returns></returns>
	internal IEnumerable<Team<T>> Teams() => _teams; 

	internal Dictionary<Player<T>, Hand<T>> Hands { get; } = new(); 
}