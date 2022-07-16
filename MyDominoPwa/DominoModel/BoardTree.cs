using DominoEngine;

namespace MyDominoPwa.DominoModel;

public class DualBoardTree<T> {
	public readonly BoardTree<T> LeftChild;
	public readonly BoardTree<T> RightChild;

	public DualBoardTree(GameState<T> gameState) {
		LeftChild = new BoardTree<T>(gameState.Board[0].Tail, gameState.Board[0].Head, -1,-1);
		RightChild = new BoardTree<T>(gameState.Board[0].Head, gameState.Board[0].Tail, 0,0);
		foreach (var (move,index) in gameState.Board.Select((move,i)=>(move,i)).Skip(1).Where(x=>!x.move.Check)) {
			var tree = new BoardTree<T>(move.Head, move.Tail, index, move.Turn);
			_ = LeftChild.Add(tree) || RightChild.Add(tree);
		}
	}
}

public class BoardTree<T> {
	public readonly List<BoardTree<T>> Children;
	public readonly T? Head;
	public readonly T? Tail;
	public readonly int Turn;
	private int _lastTurn;

	public BoardTree(T? head, T? tail, int turn, int lastTurn) {
		Children = new List<BoardTree<T>>();
		Head = head;
		Tail = tail;
		_lastTurn = lastTurn;
		Turn = turn;
	}

	internal bool Add(BoardTree<T> move) {
		if (move._lastTurn < Turn || move._lastTurn > _lastTurn) return false;
		if (move._lastTurn == Turn) {
			Children.Add(move);
			move._lastTurn = move.Turn;
			_lastTurn = move.Turn;
			return true;
		}

		if (!Children.Any(t => t.Add(move))) return false;
		_lastTurn = move.Turn;
		return true;
	}
}