namespace DominoEngine;

/// <summary>
/// Interfaz que encapsula la funcionalidad de devolver un IEnumerable<Token<T>> inifinto
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IGenerator<T> {
    public IEnumerable<Token<T>> Generate();
}

/// <summary>
/// Interfaz que encapsula la funcionalidad de repartir las fichas a los jugadores
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IDealer<T> {
    public Dictionary<Player<T>, Hand<T>> Deal(Partida<T> partida, IEnumerable<Token<T>> tokens);
}

/// <summary>
/// // Interfaz que encapsula la funcionalidade de matchear dos fichas em algun momento del juego
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IMatcher<T> {
    // Filtro de jugadas validas
    public IEnumerable<Move<T>> CanMatch(Partida<T> partida, IEnumerable<Move<T>> enumerable,
            Func<Token<T>, double> tokenScorer);

    // Devuelve los turnos validos dados cierta partida
    public IEnumerable<int> ValidsTurns(Partida<T> partida, int player);
}

/// <summary>
/// Interfaz que se encarga de iterar por los jugadores y devolverlos en algun orden
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ITurner<T> {
    public IEnumerable<Player<T>> Players(Partida<T> partida);
}

/// <summary>
/// Interfaz que contiene las condiciones de finalizacion de una partida
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IFinisher<T> {
    public bool GameOver(Partida<T> partida);
}

/// <summary>
/// Define la forma en la que se puntua una partida, y quien es el ganador
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IScorer<T> {
    // Dada una partida, puntua un movimiento
    public double Scorer(Partida<T> partida, Move<T> move);

    // Puntua un token
    public double TokenScorer(Token<T> token);

    // Rankea a los equipos despues de finalizar la partida
    public IEnumerable<Team<T>> Winner(Partida<T> partida);
}

/// <summary>
///  Esta interfaz define el concepto de un objeto que luego de una secuencia de pasos, puede rankear una lista de Teams
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IWinneable<T>
{
    // Devuelve un IEnumerable de Teams rankeados
    public IEnumerable<Team<T>> Winner();

    // Dada una instancia de un objeto, este metodo devuelve una nueva instancia
    public IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams);

    public IEnumerable<Game<T>> Games(IWinneable<T> winneable);
}