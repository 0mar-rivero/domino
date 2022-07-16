using System.Collections;

namespace DominoEngine;

public abstract class Tournament<T> : IEnumerable<Game<T>>, IWinneable<T>
{
    protected Judge<T>? Judge;
    protected IEnumerable<Team<T>>? Teams;

    protected Tournament(Judge<T> judge, IEnumerable<Team<T>> teams) {
        Judge = judge;
        if (teams.Any(team => team.IsEmpty()))
            throw new Exception("Equipo sin jugadores");
        Teams = teams;
    }

    protected Tournament() { }

    /// <summary>
    /// Setea Juez y devuelve la propia instancia
    /// </summary>
    /// <param name="judge"></param>
    /// <returns></returns>
    public Tournament<T> SetJudge(Judge<T> judge) {
        Judge =  judge;
        return this;
    }

    /// <summary>
    /// Setea Teams y devuelve la propia instancia
    /// </summary>
    /// <param name="teams"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Tournament<T> SetTeams(IEnumerable<Team<T>> teams) {
        if (teams.Any(team => team.IsEmpty()))
            throw new Exception("Equipo sin jugadores");
        Teams = teams;
        return this;
    }

    public abstract IEnumerable<Game<T>> Games(IWinneable<T> winneable);

    public abstract IEnumerable<Team<T>> Winner();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public virtual IEnumerator<Game<T>> GetEnumerator() => Games(new Game<T>()).GetEnumerator();

    public abstract IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams);
}

public class AllVsAllTournament<T> : Tournament<T>
{
    private readonly Dictionary<Team<T>, int> _games = new();

    public AllVsAllTournament() { }

    private AllVsAllTournament(Judge<T> judge, IEnumerable<Team<T>> teams) : base(judge, teams) { }

    /// <summary>
    /// Enfrenta a cada par de equipos dos veces
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) {
        Teams!.Make(team => _games.Add(team, 0)); // Inicializar el contador de partidas de cada equipo
        foreach (var (i, team1) in Teams!.Enumerate())
            foreach (var (j,team2) in Teams!.Enumerate().Where(pair => pair.Item1 != i)) {
                var newWinneable = winneable.NewInstance(Judge!, new List<Team<T>>{team1, team2});
                foreach (var game in newWinneable.Games(new Game<T>())) 
                    yield return game; // Devuelve un Game por cada combinacion de equipos
                // Actualizar el contador de partidas ganadas de cada equipo
                newWinneable.Winner().Reverse().Enumerate().Make(pair => _games[pair.Item2] += pair.Item1); 
            }
    }

    /// <summary>
    /// Crear la nueva instancia del 
    /// </summary>
    /// <param name="judge"></param>
    /// <param name="teams"></param>
    /// <returns></returns>
    public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams)
        => new AllVsAllTournament<T>(judge, teams);

    /// <summary>
    /// Ranking ordenado por la cantidad de puntos acumulados
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Team<T>> Winner() => _games.Keys.OrderByDescending(team => _games[team])!;

    public override string ToString()
        => "Para cada combinacion de dos equipos, se crean dos enfrentamientos";
}

public class DirichletTournament<T> : Tournament<T>
{
    private readonly Dictionary<Team<T>, List<IWinneable<T>>> _games = new(); 
    private readonly int _numberOfWins;

    public DirichletTournament(int number) {
        _numberOfWins = number; 
    }

    private DirichletTournament(Judge<T> judge, IEnumerable<Team<T>> teams, int number) : base(judge, teams) {
        _numberOfWins = number;
    }

    /// <summary>
    /// Genera juego hasta que un equipo gane una cantidad especifica de veces
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) {
        while (EndCondition()) {
            var newWinneable = winneable.NewInstance(Judge!, Teams!); 
            foreach (var game in newWinneable.Games(new Game<T>()))
                yield return game; 
            var team = newWinneable.Winner().First();
            if (!_games.ContainsKey(team)) _games.Add(team, new List<IWinneable<T>>(){newWinneable});
            else _games[team].Add(newWinneable);
        }
    }

    private bool EndCondition() => _games.All(pair => pair.Value.Count() < _numberOfWins);

    public override IEnumerable<Team<T>> Winner() => _games.Keys.OrderByDescending(team => _games[team].Count)!;

    public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams) 
        => new DirichletTournament<T>(judge, teams, _numberOfWins); 

    public override string ToString()
        => "Mientras un equipo no gane n veces, se seguira jugando";
}

public class PlayOffTournament<T> : Tournament<T>
{
    private readonly List<IEnumerable<Team<T>>> _rankings = new();
    private IEnumerable<Team<T>>? _winners;

    public PlayOffTournament() { }

    private PlayOffTournament(Judge<T> judge, IEnumerable<Team<T>> teams) : base(judge, teams) {}

    /// <summary>
    /// Crea un torneo de eliminatoria
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) {
        if (Teams!.Count() is 2) {
            var newWinneable = winneable.NewInstance(Judge!, Teams!);
            foreach (var game in newWinneable.Games(new Game<T>()))
                yield return game;
            _winners = newWinneable.Winner();
            yield break; // Si solo hay dos equipos, no hay mas partidas por jugar
        }
        // El número de equipos en el torneo es el doble del número de equipos en la fase de eliminatoria
        var teamsNumber = (Teams!.Count() + 1) / 2; 
        var count = 0; 
        while (count <= teamsNumber) {
            if (Teams!.Skip(count).Count() is 1) {
                _rankings.Add(Teams!.Skip(count).Take(1));
                break;
            }
            // Seleccionar los equipos que participaran en la fase de eliminatoria
            var newWinneable = winneable.NewInstance(Judge!, Teams!.Skip(count).Take(teamsNumber)); 
            foreach (var game in newWinneable.Games(new Game<T>()))
                yield return game;
            _rankings.Add(newWinneable.Winner()); // Guardar el ranking de los equipos en la fase de eliminatoria
            count += teamsNumber; // Incrementar el contador de equipos en la fase de eliminatoria
        }
        // Seleccionar los equipos que participaran en la siguiente fase
        var nextWinsel = NewInstance(Judge!, _rankings.SelectMany(teams => teams.Take((teams.Count() + 1) / 2)))!;
        foreach (var game in nextWinsel.Games(winneable))
            yield return game;
        _winners = nextWinsel.Winner(); // Guardar los equipos ganadores de la fase
    }

    public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams)
        => new PlayOffTournament<T>(judge, teams);

    /// <summary>
    /// Gana el player que gane el ultimo enfrentamiento
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Team<T>> Winner() => _winners!;

    public override string ToString()
        => "Enfrenta a la mitad de los equipos en un juego, a la otra mitad en otro, a los ganadores los enfrenta";
}

public class NGamesTournament<T> : Tournament<T>
{
    private readonly Dictionary<Team<T>, int> _winners = new();
    private readonly int _numberOfGames;

    public NGamesTournament(int numberOfGames) {
        _numberOfGames = numberOfGames;
    }

    private NGamesTournament(Judge<T> judge, IEnumerable<Team<T>> teams, int numberOfGames) : base(judge, teams) {
        _numberOfGames = numberOfGames;
    }

    /// <summary>
    /// Genera n juegos consecutivos
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) {
        for (var i = 0; i < _numberOfGames; i++) {
            var newWinneable = winneable.NewInstance(Judge!, Teams!);
            foreach (var game in newWinneable.Games(new Game<T>()))
                yield return game;    
            var winner = newWinneable.Winner().First();
            if (!_winners.ContainsKey(winner)) _winners.Add(winner, 0);
            _winners[winner]++;
        }
    }

    public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams)
        => new NGamesTournament<T>(judge, teams, _numberOfGames);

    /// <summary>
    /// Gana el que mas juegos gane
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Team<T>> Winner()
        => _winners.OrderByDescending(pair => pair.Value).Select(pair => pair.Key);

    public override string ToString()
        => "Se crearan n enfrentamientos con todos los equipos";
}

public static class TournamentExtensors
{
    /// <summary>
    /// Compone dos Tournament
    /// </summary>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static Tournament<TSource> Compose<TSource>(this Tournament<TSource> source, Tournament<TSource> other)
        => new TournamentComposition<TSource>(source, other);
}

internal class TournamentComposition<T> : Tournament<T>
{
    private readonly Tournament<T> _tournament1;
    private readonly Tournament<T> _tournament2;

    public TournamentComposition(Tournament<T> t1, Tournament<T> t2) {
        _tournament1 = t1;
        _tournament2 = t2;
    }

    private TournamentComposition(Judge<T> judge, IEnumerable<Team<T>> teams, 
        Tournament<T> t1, Tournament<T> t2) : base(judge, teams) {
            _tournament1 = t1;
            _tournament2 = t2;
    }

    public override IEnumerator<Game<T>> GetEnumerator() => Games(_tournament2).GetEnumerator();

    /// <summary>
    /// Genera torneos internos de la misma forma en que el externo genera games
    /// </summary>
    /// <param name="winneable"></param>
    /// <returns></returns>
    public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) 
        => _tournament1.SetJudge(Judge!).SetTeams(Teams!).Games(winneable); 

    public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams)
        => new TournamentComposition<T>(judge, teams, (Tournament<T>)(_tournament1.NewInstance(judge!, teams!)), _tournament2);


    /// <summary>
    /// El ganador de un torneo compuesto es el ganador del torneo externo
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Team<T>> Winner() => _tournament1.Winner(); 

    public override string ToString()
        => $@"Composicion:
    {_tournament1.ToString()!.Replace("\n","\n\t")}
    {_tournament2.ToString()!.Replace("\n","\n\t")}";
}