using System.Collections;

namespace DominoEngine;

public static class Utils
{
    /// <summary>
    ///  Devuelve una tupla con el indice y el elemento
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<(int index, TSource item)> Enumerate<TSource>(this IEnumerable<TSource> source) 
        => source.Select((t, i) => (index: i, item: t)); 
        
    /// <summary>
    /// Complemento de un IEnumerable con respecto a otro
    /// </summary>
    /// <param name="source"></param>
    /// <param name="another"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TSource> Complement<TSource>(this IEnumerable<TSource> source, IEnumerable<TSource> another)
        => source.Where(x => !another.Contains(x)); 

    /// <summary>
    /// Devuelve true si un IEnumerable no tiene elementos
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static bool IsEmpty<TSource>(this IEnumerable<TSource> source) => source.All(_ => false);

    public static void Make<TSource>(this IEnumerable<TSource> source, Action<TSource> function) {
        foreach (var item in source)
            function(item); // Ejecuta la funcion sobre cada elemento
    }

    /// <summary>
    /// Dados dos IEnumerables con los mismos elementos, construye un tercero reorganizandolos, de forma tal que se prioricen los elementos que entre ambos IEnumerables tengan menor indice
    /// </summary>
    /// <param name="source"></param>
    /// <param name="other"></param>
    /// <param name="value"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TSource> Average<TSource>(this IEnumerable<TSource> source, 
        IEnumerable<TSource> other, double value) where TSource : notnull  {
            Dictionary<TSource, double> record = new(); 
            source.Enumerate().Make(item => record.Add(item.item, item.index)); 
            other.Select((x,i) => (i * value, x)).Make(item => record[item.x] += item.Item1); 
            return record.Keys.OrderBy(key => record[key]);
        }

    /// <summary>
    /// Dado un IEnumerable, devuele un ciclo infinito de los mismos elementos
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TSource> Infinity<TSource>(this IEnumerable<TSource> source) {
        var enumerator = new InfiniteEnumerator<TSource>(source);
        while (enumerator.MoveNext())
            yield return enumerator.Current;
    }
    
    /// <summary>
    /// Dado un IEnumerable de IEnumerables, devuelve otro tomando uno a uno cada elemento de cada IEnumerable
    /// </summary>
    /// <param name="source"></param>
    /// <typeparam name="TSource"></typeparam>
    /// <returns></returns>
    public static IEnumerable<TSource> OneByOne<TSource>(this IEnumerable<IEnumerable<TSource>> source) {
        var enumerator = new OneByOneEnumerator<TSource>(source);
        while(enumerator.MoveNext())
            yield return enumerator.Current;
    }
}

internal class OneByOneEnumerator<T> : IEnumerator<T>
{
    private int _current = -1;
    private readonly List<IEnumerator<T>> _list = new();
    public OneByOneEnumerator(IEnumerable<IEnumerable<T>> enumerable) {
        // Tomar el mcm entre cantidad de elementos para que quede bien formateado
        var max = Mcm(enumerable.Select(x => x.Count()));                       
        enumerable.Make(x => _list.Add(x.Infinity().Take(max).GetEnumerator()));
    }

    public T Current => _list[_current].Current;

    object IEnumerator.Current => Current!;

    public void Dispose() {}

    // No dara false hasta que no se complete el ciclo y vuelva al inicio
    public bool MoveNext() {
        _current  = (_current+1) % _list.Count();
        return _list[_current].MoveNext();
    }

    public void Reset() => _current = -1;

    private int Mcm(IEnumerable<int> items) {
        var mcm = 0;
        for (var i = 1; i < items.Count(); i++) {
            mcm = OneByOneEnumerator<T>.Mcm(items.ElementAt(i), items.ElementAt(i-1));
        }
        return mcm;
    }

    private static int Mcm(int a, int b) => (a * b) / Mcd(a, b);

    private static int Mcd(int a, int b) {
        if (a is 0 || b is 0) return 0;
        return (a % b is 0)? b : Mcd(b, a % b);
    }
}

internal class InfiniteEnumerator<T> : IEnumerator<T>
{
	private readonly IEnumerable<T> _enumerable;
	private int _current = -1;

    public InfiniteEnumerator(IEnumerable<T> enumerable) {
		_enumerable = enumerable;
    }

    public T Current => _enumerable.ElementAt(_current);

    object IEnumerator.Current => Current!;

    public void Dispose() {}

    public bool MoveNext() {
		_current = (_current+1) % _enumerable.Count();
		return true;
	}

    public void Reset() => _current = -1;
}
