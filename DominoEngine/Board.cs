﻿using System.Collections;

namespace DominoEngine;

public class Board<T>: IReadOnlyList<Move<T>> {
	readonly List<Move<T>> _moves = new(); 

	public IEnumerator<Move<T>> GetEnumerator() => _moves!.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

	public int Count => _moves!.Count;

	public Move<T> this[int index] {
		// El tablero indexado en -1 representa la cabeza de la salida
		get {
			if (index is -1) {
				var move = _moves[0];
				return new Move<T>(move.PlayerId, false, -1, move.Tail, move.Head);
			}
			else return _moves[index];
		}
	} 

	internal void Add(Move<T> item) => _moves!.Add(item);

    public override string ToString()
    {
        string result = "";
		foreach (var move in _moves!)
			result += $"{move.PlayerId}: {move.ToString()}\n";
		return result;
    }
}