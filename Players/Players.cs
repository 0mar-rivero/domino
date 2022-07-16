using DominoEngine;

namespace Players;


public class RandomPlayer<T> : Player<T>
{
    public RandomPlayer(string name) : base(name) {}

    public override Move<T> Play(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner)
                => moves.ElementAt(Random.Next(moves.Count()));

    public override string ToString()
        => Name + " (random)";
}

public class Botagorda<T> : CriterionPlayer<T>
{
    public Botagorda(string name) : base(name) {}

    public Botagorda(int playerId) : base(playerId) {}

    public override IEnumerable<Move<T>> PreferenceCriterion(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner)
                => moves.OrderByDescending(scorer);

    public override string ToString()
        => Name + " (botagorda)";
}

public class DisablerPlayer<T> : CriterionPlayer<T>
{
    private readonly Botagorda<T> _aux;
    public DisablerPlayer(string name) : base(name) {
        _aux = new Botagorda<T>(PlayerId);
    }

    public DisablerPlayer(int playerId) : base(playerId) {
        _aux = new Botagorda<T>(playerId);
    }

    public override IEnumerable<Move<T>> PreferenceCriterion(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner) 
                => moves.OrderBy(move => DestroyRival(move.Head!, board, MakeRivalsTeams(board, partner), passesInfo)).
                Average(moves.OrderByDescending(move => RivalPasses(move.Tail!, board, MakeRivalsTeams(board, partner), passesInfo)),1).
                Average(moves.OrderBy(move => PreferRivalsTokens(move, board, partner)),1).
                Average(_aux.PreferenceCriterion(moves, passesInfo, board, inHand, scorer ,partner),0.5);

    /// <summary>
    /// Arma los equipos rivales
    /// </summary>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private IEnumerable<int> MakeRivalsTeams(List<Move<T>> board, Func<int, int, bool> partner)
        => board.Select(move => move.PlayerId).Where(pId => !partner(pId, PlayerId)).ToHashSet();

    /// <summary>
    /// Prioriza jugar por fichas del jugadores rivales
    /// </summary>
    /// <param name="move"></param>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private int PreferRivalsTokens(Move<T> move, List<Move<T>> board, Func<int, int, bool> partner) {
        if (board.IsEmpty()) return 0;
        else return (move.Check)? 1 : (partner(PlayerId, board[(move.Turn is -1)? 0 : move.Turn].PlayerId))? 0 : 1;
    }

    /// <summary>
    /// Calcula a cuantos jugadores podria pasar en este tunro
    /// </summary>
    /// <param name="head"></param>
    /// <param name="board"></param>
    /// <param name="rivalId"></param>
    /// <param name="passesInfo"></param>
    /// <returns></returns>
    private static int DestroyRival(T head, List<Move<T>> board, IEnumerable<int> rivalId, Func<int, IEnumerable<int>> passesInfo) 
        => rivalId.Count(player => board.Where(move => move.Check && move.PlayerId == player).Enumerate().
            Select(pair => passesInfo(pair.Item1)).
                Any(turns => turns.Any(turn => turn is -1 ? Equals(board[0].Head,head) : Equals(board[turn].Tail,head))));

    /// <summary>
    /// Evita jugar por fichas por las cuales los players rivales estan pasados
    /// </summary>
    /// <param name="tail"></param>
    /// <param name="board"></param>
    /// <param name="rivalId"></param>
    /// <param name="passesInfo"></param>
    /// <returns></returns>
    private static int RivalPasses(T tail, List<Move<T>> board, IEnumerable<int> rivalId, Func<int, IEnumerable<int>> passesInfo) 
        => rivalId.Count(player => board.Where(move => move.Check && move.PlayerId == player).Enumerate().
            Select(pair => passesInfo(pair.Item1)).
                Any(turns => turns.Any(turn => turn is -1 ? Equals(board[0].Head,tail) : Equals(board[turn].Tail,tail))));

    public override string ToString()
        => Name + " (disabler)";
}

public class SupportPlayer<T> : CriterionPlayer<T>
{
    private readonly Botagorda<T> _aux;

    public SupportPlayer(string name) : base(name) {
        _aux = new Botagorda<T>(PlayerId);
    }

    public SupportPlayer(int playerId) : base(playerId) {
        _aux = new Botagorda<T>(playerId);
    }

    public override IEnumerable<Move<T>> PreferenceCriterion(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner)
                => moves.OrderBy(move => HelpTeam(move.Head!, board, MakeTeam(board, partner), passesInfo)).
                Average(moves.OrderBy(move => AvoidFriendPasses(move.Tail!, board, MakeTeam(board, partner), passesInfo)),1).
                Average(moves.OrderBy(move => AvoidFriendsTokens(move, board, partner)),1).
                Average(_aux.PreferenceCriterion(moves, passesInfo, board, inHand, scorer ,partner), 0.5);

    /// <summary>
    /// Encontrar Id de compañeros de equipo
    /// </summary>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private IEnumerable<int> MakeTeam(List<Move<T>> board, Func<int, int, bool> partner)
        => board.Select(move => move.PlayerId).Where(pId => pId != PlayerId
            && partner(pId, PlayerId)).ToHashSet();

    /// <summary>
    /// Calcula cantidad de compañeros pasados por un valor especifico
    /// </summary>
    /// <param name="head"></param>
    /// <param name="board"></param>
    /// <param name="teamId"></param>
    /// <param name="passesInfo"></param>
    /// <returns></returns>
    private int HelpTeam(T head, List<Move<T>> board, IEnumerable<int> teamId, Func<int, IEnumerable<int>> passesInfo) 
        => teamId.Count(player => board.Where(move => move.Check && move.PlayerId == player).Enumerate().
            Select(pair => passesInfo(pair.Item1)).
                Any(turns => turns.Any(turn => turn is -1 ? Equals(board[0].Head,head) : Equals(board[turn].Tail,head))));

    /// <summary>
    /// Evitar jugar por fichas puestas por compañeros de equipo
    /// </summary>
    /// <param name="move"></param>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private int AvoidFriendsTokens(Move<T> move, List<Move<T>> board, Func<int, int, bool> partner) {
        if (board.IsEmpty()) return 0;
        else return (move.Check)? 1 : (partner(PlayerId, board[(move.Turn is -1)? 0 : move.Turn].PlayerId))? 1 : 0;
    }  

    /// <summary>
    /// Contar la cantidad de compañeros que podrian pasarse con un valor especifico
    /// </summary>
    /// <param name="tail"></param>
    /// <param name="board"></param>
    /// <param name="teamId"></param>
    /// <param name="passesInfo"></param>
    /// <returns></returns>
    private int AvoidFriendPasses(T tail, List<Move<T>> board, IEnumerable<int> teamId, Func<int, IEnumerable<int>> passesInfo) 
        => teamId.Count(player => board.Where(move => move.Check && move.PlayerId == player).Enumerate().
            Select(pair => passesInfo(pair.Item1)).
                Any(turns => turns.Any(turn => turn is -1 ? Equals(board[0].Head,tail) : Equals(board[turn].Tail,tail))));

    public override string ToString()
        => Name + " (support)";
}

public class CarrierPlayer<T> : CriterionPlayer<T> where T : notnull
{
    private readonly Botagorda<T> _aux;
    public CarrierPlayer(string name) : base(name) {
        _aux = new Botagorda<T>(PlayerId);
    }

    public CarrierPlayer(int playerId) : base(playerId) {
        _aux = new Botagorda<T>(playerId);
    }

    public override IEnumerable<Move<T>> PreferenceCriterion(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner) {
                var data = Data();
                var newMoves = moves.OrderByDescending(move => Math.Min(data[move.Tail!], data[move.Head!]));
                return newMoves.Average(_aux.PreferenceCriterion(moves, passesInfo, board, inHand, scorer, partner), 0.5);
            }

    /// <summary>
    /// Para un valor T cualquiera, devuelva cuantas ocurrencias tiene en la mano del player
    /// </summary>
    /// <returns></returns>
    private Dictionary<T, int> Data() {
        Dictionary<T, int> data = new();
        foreach (var (head, tail) in Hand!) {
            if (!data.ContainsKey(head)) data.Add(head, 1);
            else data[head]++;
            if (!data.ContainsKey(tail)) data.Add(tail, 1);
            else data[head]++;
        }
        return data;
    }

    public override string ToString()
        => Name + " (carrier)";
}

public class SmartPlayer<T> : CriterionPlayer<T> where T : notnull
{
    private readonly CarrierPlayer<T> _carrier;
    private readonly SupportPlayer<T> _support;
    private readonly DisablerPlayer<T> _disabler;
    
    public SmartPlayer(string name) : base(name) { 
        _carrier = new CarrierPlayer<T>(PlayerId);
        _support = new SupportPlayer<T>(PlayerId);
        _disabler = new DisablerPlayer<T>(PlayerId);
    }

    public override IEnumerable<Move<T>> PreferenceCriterion(IEnumerable<Move<T>> moves, 
        Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, Func<int, int> inHand, 
            Func<Move<T>, double> scorer, Func<int, int, bool> partner) {
                var first = ((CriterionPlayer<T>)_carrier.SetHand(Hand!))
                    .PreferenceCriterion(moves, passesInfo, board, inHand, scorer, partner);
                var second = ((CriterionPlayer<T>)_support.SetHand(Hand!)).
                    PreferenceCriterion(moves, passesInfo, board, inHand, scorer, partner);
                var third = ((CriterionPlayer<T>)_disabler.SetHand(Hand!)).
                    PreferenceCriterion(moves, passesInfo, board, inHand, scorer, partner);

                if (MakeRivalsTeams(board, partner).IsEmpty() || MakeTeam(board, partner).IsEmpty())
                    return first;

                if (MakeRivalsTeams(board, partner).Select(inHand).Min() > 4) {
                    if (MakeTeam(board, partner).Select(inHand).Min() < inHand(PlayerId))
                        (first, second) = (second, first);
                }
                else (first, third) = (third, first);

                return first.Average(second,0.5).Average(third,0.5);
            }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private IEnumerable<int> MakeTeam(List<Move<T>> board, Func<int, int, bool> partner)
        => board.Select(move => move.PlayerId).Where(pId => pId != PlayerId
            && partner(pId, PlayerId)).ToHashSet();

    /// <summary>
    /// Devuelve un IEnumerable con los hashCode de las players rivales
    /// </summary>
    /// <param name="board"></param>
    /// <param name="partner"></param>
    /// <returns></returns>
    private IEnumerable<int> MakeRivalsTeams(List<Move<T>> board, Func<int, int, bool> partner)
        => board.Select(move => move.PlayerId).Where(pId => !partner(pId, PlayerId)).ToHashSet();

    public override string ToString()
        => Name + " (smart)";
}
