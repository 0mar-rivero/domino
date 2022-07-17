namespace DominoEngine;

public class ClassicScorer : IScorer<int>
{
    /// <summary>
    /// Los valores de los movimientos son los clasicos
    /// </summary>
    /// <param name="partida"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    public double Scorer(Partida<int> partida, Move<int> move) => TokenScorer(move.Token);

    public double TokenScorer(Token<int> token) => token.Head + token.Tail;

    /// <summary>
    /// Gana el que se pega o tiene la data mas baja
    /// </summary>
    /// <param name="partida"></param>
    /// <returns></returns>
    public IEnumerable<Team<int>> Winner(Partida<int> partida) {
        foreach (var player in partida.Players().Where(x => partida.Hands[x].IsEmpty())) {
            var winners = new List<Team<int>>(){partida.TeamOf(player)};
            return winners.Concat(partida.Teams().Complement(winners));
        }
        return partida.Teams().OrderBy(team => team.Sum(player => partida.Hand(player).
                Sum(TokenScorer)));
    }

    public override string ToString() 
        => "Las fichas valen la suma de sus datas, gana el equipo con menor data";
}

public class ModFiveScorer : IScorer<int>
{
    /// <summary>
    /// Solo devuelve puntuacion si la suma es divisible por 5
    /// </summary>
    /// <param name="partida"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    public double Scorer(Partida<int> partida, Move<int> move) {
        if ((TokenScorer(partida.Board[move.Turn].Token) + TokenScorer(move.Token) % 5 is 0))
            return TokenScorer(partida.Board[move.Turn].Token) + TokenScorer(move.Token);
        else return 0;
    }

    public double TokenScorer(Token<int> token) => token.Head + token.Tail;

    /// <summary>
    /// Devuelve los equipos rankeados por la suma de la puntuacion de sus jugadores
    /// </summary>
    /// <param name="partida"></param>
    /// <returns></returns>
    public IEnumerable<Team<int>> Winner(Partida<int> partida)
        => partida.Teams().OrderBy(team => team.Sum(player => partida.Board.
            Where(move => move.PlayerId == Partida<int>.PlayerId(player) && !move.Check).Sum(move => Scorer(partida, move))));

    public override string ToString()
        => "Si la ficha suma un multiplo de 5 por donde va a jugar, ese es el valor del movimiento.\nGana el que alcance mayor puntuacion";
}

public class TurnDividesBoardScorer : IScorer<int>
{
    private readonly Dictionary<Partida<int>, List<(int turn, int score)>> _scores = new();

    public double Scorer(Partida<int> partida, Move<int> move) {
        if (!_scores.ContainsKey(partida))
            _scores.Add(partida, new List<(int turn, int score)>(){(0, 0)});
        if (_scores[partida].Count is 1)
            return TokenScorer(move.Token);
        else {
            Update(partida);
            if (_scores[partida].Last().score + TokenScorer(move.Token) % (_scores[partida].Last().score + 1) is 0)
                return TokenScorer(move.Token);
            else return 0;
        }
    }

    private void Update(Partida<int> partida)
        => partida.Board.Enumerate().Where(pair => !pair.Item2.Check && pair.Item1 > _scores[partida].Count).
            Make(pair => _scores[partida].Add((pair.Item1, _scores[partida].Last().score + (int)TokenScorer(pair.Item2.Token))));


    public double TokenScorer(Token<int> token) => token.Head + token.Tail;

    public IEnumerable<Team<int>> Winner(Partida<int> partida)
        => partida.Teams().OrderBy(team => team.Sum(player => partida.Board.Enumerate().
            Where(pair => !pair.Item2.Check && pair.Item2.PlayerId == Partida<int>.PlayerId(player)).
            Sum(pair => _scores[partida].First(x => x.turn == pair.Item1).score)));

    public override string ToString()
        => "Si la suma del tablero es divisible entre el numero de turnos, la ficha vale su valor, gana el de mayor puntuacion";
}

public static class ScorerExtensors
{
    /// <summary>
    /// Invierte un IScorer
    /// </summary>
    /// <param name="scorer"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IScorer<TSource> Inverse<TSource>(this IScorer<TSource> scorer)
        => new InverseScorer<TSource>(scorer);
}

internal class InverseScorer<T> : IScorer<T>
{
    private readonly IScorer<T> _scorer;

    public InverseScorer(IScorer<T> scorer) {
        _scorer = scorer;
    }

    /// <summary>
    /// Las fichas valen lo inverso del IScorer original
    /// </summary>
    /// <param name="partida"></param>
    /// <param name="move"></param>
    /// <returns></returns>
    public double Scorer(Partida<T> partida, Move<T> move)
        => _scorer.Scorer(partida, move) is 0 ? int.MaxValue : 1 / (_scorer.Scorer(partida, move));


    public double TokenScorer(Token<T> token)
        => _scorer.TokenScorer(token) is 0 ? int.MaxValue : 1 / (_scorer.TokenScorer(token));

    /// <summary>
    /// Gana el que hubiera perdido en el IScorer original
    /// </summary>
    /// <param name="partida"></param>
    /// <returns></returns>
    public IEnumerable<Team<T>> Winner(Partida<T> partida) => _scorer.Winner(partida).Reverse();

    public override string ToString()
        => $@"Union:
    {_scorer.ToString()!.Replace("\n","\n\t")}";
}
