# Domino PWA
Proyecto de Programación II. Facultad de Matemática y Computación. Universidad de La Habana. Curso 2022

> Omar Rivero Gómez-Wangüemert. C-211

> Alex Sierra Alcalá. C-211

Domino PWA es una variación del juego de mesa clásico del domino, que como sabemos se compone de una serie de jugadores que juegan en una secuencia de turnos dada, ganando aquel que se quede sin fichas, o en caso de que no se pueda jugar más, ganando  aquel cuya mano suma menos puntos. En nuestra implementación, intentamos abstraer este concepto de juego, manteniendo la idea de un juego de mesa con fichas de dos caras, pero con regla modificables, pudiéndose obtener alguna variante bien diferente del juego clásico. Entre las reglas modificables se encuentran:

* La forma en la que se generan las fichas del juego.

* La forma en la que se reparten las fichas a cada jugador.

* El orden del turno en que se realizan las jugadas.

* La forma en la que matchea una ficha con otra.

* La forma de puntuar las fichas y decicidir quien gana.

* Las condiciones de fin de juego.

Contamos también con un sistema de personalización de torneos que comentaremos mejor más adelante. Tenemos además la implementación de una interfaz gráfica para el juego, que permite la interacción con el usuario...

## Detalles de la implementación

Para nuestra implementación, dividimos nuestro código en dos carpertas: `DominoLogic` y `MyDominoPWA`. En `DominoLogic` se encuentran las clases que implementan el juego, y en `MyDominoPWA` se encuentran las clases que implementan la interfaz gráfica. `DominoLogic` tiene a su vez las classlibs `DominoEngine` y `Players`.

## DominoEngine

Aquí se encuentra toda la jerarquía de clases que nos permite armar la estructura sobre la cual descansa el juego. Es válido aclarar que, aunque solo implementamos una variante de domino donde las fichas tienen valores enteros en sus caras, la estructura está creada para correr sobre un juego totalmente genérico. Comenzemos explicando qué objetos usamos para nuestro juego:

### Token
Representa una ficha del juego. Es una tupla de `(T,T)` donde cada valor está asigando a una de las caras. Sobre este objeto están redefinidos los métodos `Equals()` y `GetHashCode()`, ya que la ficha `(Head,Tail)` es exactamente igual a la ficha `(Tail,Head)`. Las propiedades `Head` y `Tail` nos permiten obtener el valor de la cara correspondiente, con un único accesor `get` para que sean inmutables.

### Move
Representa una jugada. Es una tupla de `(int,bool,int,T,T)`. En la primera posición se encuentra el Id del player que realiza la jugada. En la segunda posición se encuentra un booleano, por default true, que indica si la jugada es o no un pase. En la tercera posición se encuentra el turno por cual se jugará la ficha, por default -2, considerando que la salida apunta al turno -1, de esta forma, un `Move` cuyo `Turn` sea n, se refiere a un movimiento que se desea hacer matcheando el `Head` de una ficha con el `Tail` de la ficha jugada en el turno n. En la cuarta posición se encuentra el `Head` de la ficha jugada, y en la quinta posición se encuentra el `Tail` de la ficha jugada.

### Board
Representa el tablero del juego. Es una lista de `Move`, donde indexar en -1 es equivalente a pregunatar por el `Head` de la ficha jugada en la salida.

### Hand
Representa las manos de los jugadores. Esta clase implementa la interfaz `ICollection<Token<T>>`, con un método `Clone()` que nos permite clonar una mano para evitar que nos cambien los valores originales al pasarlos por referencia.

### Player
Es una clase abstracta que representa en esencia a los jugadores. Tiene un método `Play()` que recibe un `IEnumerbale<Move<T>>` y unos cuantos delegados que le brindan al jugador toda la información que pudiera necesitar sobre el juego:

```c#
public abstract Move<T> Play(IEnumerable<Move<T>> possibleMoves, Func<int, IEnumerable<int>> passesInfo, List<Move<T>> board, 
		Func<int, int> inHand, Func<Move<T>, double> scorer, Func<int, int, bool> partner);
```
* `possibleMoves`: Es un `IEnumerable<Move<T>>` que contiene todas las jugadas que puede hacer el jugador.
* `passesInfo`: Es una función que recibe un `int`, el Id de un player, y devuelve un `IEnumerable<int>` que contiene los turnos en que el jugador con dicho Id se pasó.
* `board`: Es una lista de `Move<T>` que representa el tablero del juego.
* `inHand`: Es una función que recibe un `int`, el Id de un player, y devuelve el número de fichas que tiene en su mano.
* `scorer`: Es una función que recibe un `Move<T>` y devuelve un `double` que representa la puntuación de la jugada.
* `partner`: Es una función que recibe una tupla `(int,int)`, el Id de dos players, y devuelve un `bool` que indica si ambos jugadores están en el mismo equipo.

### Team
Es una lista de `Player` que representa a los jugadores que pertenecen a un mismo equipo. La idea de esta clase surge de la posibilidad del juego en equipo,
y no hacer diferencias a jugar en equipo de a uno, es decir, en solitario.

### Partida
Es una clase que representa una partida. Tiene una lista de `Team` que representan a los equipos que participan en la partida, 
las `Hand` de los players que participan en ella, y en general toda la información que se pudiera necesitar para jugar. 
Solo necesita en su constructor las lista de los jugadores que iteractuan. La mayoría de los delegados que recibe el `Player` son métodos de esta clase.

### Reglas
Definimos el comportamiento de cada una de nuestras reglas mediante el uso de `Interfaces`. Acá una breve explicación de cómo funcionan dichas interfaces:

* `IGenerator<T>`: Define la forma en la que se generan las fichas de domino. Pensamos que sería interesante esta interfaz que cuya función es la de encapsular la generación de fichas de un tipo que potencialmente no conocemos. Solo implementamos algunas variantes de `IGenerator<int>`. Las clases que implementan esta interfaz tienen un método `Generate()` que devuelve de forma lazy un `IEnumerable<Token<T>>` infinito, basándose en la generación de fichas de forma triangular. Ejemplo:
```c#
IEnumerable<Token<int>> IGenerator<int>.Generate() {
    for (var i = 0; ; i++)
        for (var j = 0; j <= i; j++)
            yield return new Token<int>(i, j);
}
```
* `IDealer<T>`: Define la forma en la que se reparten las fichas de domino. Las clases que implementan esta interfaz tienen un método `Deal` que recibe el `IEnumerable<Token<T>>` deuvuelto por el `IGenerator`, y con esos tokens crea un `Dictionary<Player<T>, Hand<T>>` que representa las manos de los jugadores.
* `ITurner<T>`: Define el orden en que jugarán los players. Las clase que implementen esta interfaz tienen un método `Players()` que devuelve un `IEnumerable<Player<T>>` con el orden que se seguirá en la partida. Estos ienumerables son infinitos y cumplen que no hay dos players del mismo equipo que jueguen de forma consecutiva. Esto fue resuelto con el empleo de los métodos extensores `Enumerable.OneByOne()` y `Enumerable.Infinity()` implementados en la clase estática `Utils`, los cuales hacen uso de dos `IEnumerator<T>` implementados en esa misma clase.
* `IFinisher<T>`: Determina las condiciones de finalización de la partida. Las clases que implementen esta interfaz tienen un método `GameOver()` que devuelve un `bool` que indica si la partida ya terminó o no. 
* `IScorer<T>`: Determina la puntuación de cada movimiento en cada momento de una partida, así como el valor individual de cada ficha, no necesariamente igual al valor del movimiento que la contiene. Las clases que implementen esta interfaz tienen un método `Winner()` que dada una partida, devuelve un `IEnumerable<Player<T>>`, un ranking de equipos ordenados según su clasificación en el juego.
* `IMatcher<T>`: Esta interfaz encapsula la funcionalidad de permitir o no ubicar una ficha por alguno de los turnos del tablero. Es básicamente un filtro de jugadas válidas. Algunas de las variantes que tenemos implementadas, nos permiten matchear por donde lo haría el domino clásico (por los dos extremos), o como se haría en un juego de longana, sin especificar que el `Tail` de la ficha puesta y el `Head` de la ficha por poner sean necesariamente iguales, es más, podrían matchear bajo cualquier criterio, como veremos luego. Un ejemplo de cómo funciona el `SideMatcher<T>`, que matchea por los turnos como se haría en un juego de domino clásico:
```c#
// Actuliza los turnos validos de la forma clasica
private void ValidsTurn(Partida<T> partida) {
    foreach (var (turn, move) in partida.Board.Enumerate().
	// Revisa solo movimientos que no sean pases
	Where((pair => !pair.item.Check && 
	// Cuyo valor del turno sea mayor al guardado en la actualización anterior
    pair.index > _validsTurns[partida].MaxBy(value => value) && pair.index >= 1))) { 
        _validsTurns[partida].Remove(move.Turn);
        _validsTurns[partida].Add(turn);
    }
}
```

#### Extensibilidad de las reglas
Aún inconformes con las implementaciones de las clases que se comportan como reglas del juego, decidimos dar un paso más en la búsqueda de su extensibilidad y variedad. Para algunas de las reglas creamos clases estáticas que agrupan un conjunto de métodos extensores sobre la interfaz, que, básicamente, son operadores unarios o binarios que nos permiten, a partir de las funcionalidades ya implementadas, crear nuevas instancias de reglas personalizadas, potencialmente diferentes a las originales. Veamos exactamente como se manifiestan con el ejemplo de la clase `MatcherExtensors`:
```c#
public static class MatcherExtensors{
    public static IMatcher<TSource> Intersect<TSource>(this IMatcher<TSource> matcher1, IMatcher<TSource> matcher2)
        => new IntersectMatcher<TSource>(matcher1, matcher2);

    public static IMatcher<TSource> Join<TSource>(this IMatcher<TSource> matcher1, IMatcher<TSource> matcher2)
        => new JoinMatcher<TSource>(matcher1, matcher2);

	public static IMatcher<TSource> Inverse<TSource>(this IMatcher<TSource> matcher)
        => new InverseMatcher<TSource>(matcher);
}
```
* `IntersectMatcher<T>`: Crea un `IMatcher<T>` a partir de dos matchers `m1` y `m2`, siendo el resultante una instancia nueva que solo valida movimientos que son confirmados a la vez por `m1` y `m2`, o sea una intersección entre los ienumerables. Por ejemplo, el código:
```c#
new SideMatcher<T>().Interect(new EqualMatcher<T>());
``` 
Crea una instancia que únicamente valida movimientos que apunten a los dos turnos del domino clásico y además, el `Tail` de la cola de la ficha puesta sea necesariamente igual al `Head` de las ficha por poner. Y el código:
```c#
new LonganaMatcher<int>().Intersect(new TeamTokenInvalidMatcher<int>()).Intersect(new RelativesPrimesMatcher());
```
Crea un `IMatcher<int>` que valida movimientos como se haría jugando longana, pero que no permita que una ficha sea ubicada en un turno por el cual jugó un compañero de equipo, y además, solo si las respectivas sumas de las fichas a matchear son primos relativos.
* `JoinMatcher<T>`: Crea un `IMatcher<T>` a partir de dos matchers `m1` y `m2`, siendo el resultante una instancia nueva que solo valida movimientos que son confirmados por `m1` o `m2`. Por ejemplo, el código:
```c#
new EvenOddMatcher().Join(new EqualMatcher<int>().Intersect(new SideMatcher<int>()));
```
Crea un `IMatcher<int>` que valida el matcheo de movimientos cuya paridad coincida con la del turno, o cumpla con la condición de matcheo del juego de domino clásico.
* `InverseMatcher<T>`: Crea un `IMatcher<T>` a partir de un matcher `m`, siendo el resultante una instancia nueva que solo valida movimientos que no son confirmados por `m`. Por ejemplo, el código:
```c#
new EvenOddMatcher().Inverse().Intersect(new RelativesPrimesMatcher().Inverse());
```
Crea un `IMatcher<int>` que valide movimientos si la paridad de la ficha a poner es diferente a la del turno, y además, si las respectivas sumas de las ficha a matchear no son es primos relativos.

### Judge
Esta clase representa al juez que tendrá control sobre las acciones que se realizan en el juego. En su constructor recibe un conjunto 6 instancias, una por cada tipo de regla de las explicadas anteriormente, que serán las reglas por las que estará regido el juego. El juez tiene un método `Start()` que comienza con la repartición de las manos a los jugadores, haciendo uso de las instancias del `IGenerator<T>` y el `IDealer<T>`, luego se llama al método `Play()` que devuelve un `IEnumerable<Player<T>>`, en el mismo orden en que los va devolviendo el `ITurner<T>`, a medida que los players vayan efectuando sus jugadas. Explicaremos cuál sería la secuencia de pasos lógicos durante un turno de una partida de cualquier juego de domino. Primero, efectuar la salida, para eso se llama al método booleano `Salir()` que solo devolverá `true` una vez se haya jugado una salida validada por el `IMatcher<T>`, lo cual es conveniente ya que le deja la responsabilidad al matcher de decidir qué salidas son correctas para él. Una vez salida puesta en mesa, el juez comprueba si se cumplió la condición de finalización del `IFinisher<T>`. Luego se llama al método `GenMoves()` que genera todos los movimientos posibles de la siguiente forma: por cada ficha `t` en la mano del player actual, y por cada turno `i` del tablero, incluyendo el -1, genera un movimiento con `t.Head` que apunta a `i` y otro con `t.Tail` que apunta a `i`, incluyendo además un pase. Estos movimientos son pasados por el filtro del `IMatcher<T>`, de forma tal que el juez le da al jugador un `IEnumerable<Move<T>>` solo con jugadas válidas. El player juega y el juez comprueba que el movimiento devuelto se encuentra efectivamente entre los disponibles, en caso contrario toma por default el primero de los movimientos disponibles. Elimina la ficha jugada de la mano del player, y actualiza la información del tablero. Finalmente se devuelve al player que acaba de jugar. Tiene un método `Winner()` en el cual recibe del `IScorer<T>` un `IEnumerbale<Team<T>>` del ranking de los equipos dado un momento específico de la partida.
```c#
public IEnumerable<Player<T>> Play(Partida<T> partida) {
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
		// Se actualiza la informacion del tablero
		partida.AddValidsTurns(_matcher.ValidsTurns(partida, partida.PlayerId(player))); 
		if (!move!.Check) partida.RemoveFromHand(player, move.Token!); // Si no es un pase, se quita de la mano
		yield return player; // Se devuelve al player
	}
}
```

### GameState
Esta clase representa el estado actual del juego. Es una tupla de `(List<Move<T>>, Dictionary<Player<T>, Hand<T>>, int, Player<T>)` con toda la información necesaria para poder expectar cada instante de un juego:
* Lista de movimientos jugados sobre el tablero.
* Las manos actuales de cada uno de los jugadores que participan.
* El turno actual.
* El player que acaba de relizar el último movimiento.

### Game
La clase `Game` no es más que un `IEnumerable<GameState<T>>` de todos los estados del juego desde que se inicia hasta que se termina. Recorrerlo nos permitiría espectar turno por turno cada jugada.

### Tournament
La clase abstracta `Tournament<T>` representa un torneo de domino. Expliquemos cómo funciona esta abstracción. 
```c#
public interface IWinneable<T> {
    public IEnumerable<Team<T>> Winner();
    public IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams);
    public IEnumerable<Game<T>> Games(IWinneable<T> winneable);
}
```
Encapsulamos en la interfaz `IWinneable<T>` el comportamiento de objetos tales que, después de generar un `IEnumerable<Game<T>>`, son capaces de devolver un `IEnumerable<Team<T>>` ordenado bajo algún criterio. Las clases que implementan esta interfaz están obligadas además a implementar el método `NewInstance()` que devuelve una nueva instancia de la misma clase, con un `Judge` y un `IEnumerable<Team<T>>` que recibe el método. Las clases `Game<T>` y `Tournament<T>` implementan esta interfaz. Cada torneo particular hereda de la clase abstracta `Tournament<T>` y define su propia forma de generar su `IEnumerable<Game<T>>`. Pero surgió la idea de que cada torneo pudiera, de la misma forma en que genera juegos, generar otros tipos de torneos, es decir, de alguna forma lograr la composición entre torneos para obtener nuevas instancias que se comportan totalmente diferente a las originales. Para ello nos creamos un método extensor de `Tournament<T>` el cual llama a una nueva instancia de la clase `TournamentComposition<T>`. Expliquemos como resolvimos esto. El método `Games()` recibe como parámetro una instancia de alguna clase que implementa la interfaz `IWinneable<T>`, digamos una varible de nombre `winneable`, y por tanto, sabe replicarse, entonces la idea de generar juegos de la misma forma en la que se generan torneos se puede solucionar instanciando el `IWinneable<T>` recibido, pasándole los valores de `Judge<T>` y `IEnumerable<Team<T>>` que usará y luego recorriendo `winneable.Games(new Game<T>())` para ir devolviendo uno a uno en el torneo externo, los games generados por el torneo interno. En definitiva, la composición de torneos es básicamente una función recursiva y asociativa que tiene como base la generación explícita de las instancias de `Game<T>`. La clase `TournamentComposition<T>` tiene un comportamiento tal vez algo bizzaro en los siguientes métodos:
```c#
public override IEnumerator<Game<T>> GetEnumerator() => Games(_tournament2).GetEnumerator();
```
Como vemos se redefine `GetEnumerator<T>` para que llame al método `Games()` pero cambiando el parámetro por defecto a `_tournament2` para generar ahora torneos de esta nueva instancia.
```c#
public override IEnumerable<Game<T>> Games(IWinneable<T> winneable) 
    => _tournament1.SetJudge(Judge!).SetTeams(Teams!).Games(winneable); 
```
Se le setean los valores necesarios a `_tournament1` para que pueda generar las nuevas instancias de la forma en que lo hace su implementación de `Game()`.
```c#
public override IWinneable<T> NewInstance(Judge<T> judge, IEnumerable<Team<T>> teams)
    => new TournamentComposition<T>(judge, teams, (Tournament<T>)(_tournament1.NewInstance(judge!, teams!)), _tournament2);
```
Para crear una instancia nueva de un torneo compuesto es necesario también crear una instancia nueva del torneo externo ya que este guarda información nada relevante que entorpecerán el correcto funcionamiento del torneo compuesto.

## Players
La `classlib` que contiene la implementación específica de cada uno de los jugadores fue creada a parte debido a un tema de protección de nuestro código, para que nunguna implementación de `Player` pudiera modificar valores importantes sobre los datos de la partida, ya que la gran mayoría de los métodos en `DominoEngine` son `internal`, solo visibles desde el mismo `.dll`. Hablemos un poco sobre nuestras implementaciones de players:

* `RandomPlayer<T>`: Como el nombre lo indica, selecciona un movivmiento random cualquiera entre los movimientos válidos y lo devuelve.
### CriterionPlayer
Definimos como jugadores con criterio a aquellos que de alguna forma organizan los movimientos disponibles cada vez que van a jugar, dejando en la primera posición aquel movimiento que prefiere jugar.
### Average Extensor
Este método extensor toma dos `IEnumerables<T>` con los mismos elementos pero en diferente orden, y un valor `double`. La idea es devolver un `IEnumerable<T>` organizado bajo el criterio de que primero aparecerá el valor de mayor relevancia teniendo en cuenta la suma de los índices respectivos del elemento en cada ienumerable, tomando los índices del segundo ienumerable, como su valor multiplicado por el `double` recibido. Este métodos lo usan los jugadores para elegir un mejor movimiento entre varios criterios de preferencia.
* `BotagordaPlayer<T>`: El clásico jugador botagorda que juega minimizando los puntos en su mano. Perfectamente adaptable a las diferentes formas de calcular el valor de un movimiento.
* `CarrierPlayer<T>`: Este es un jugador egoísta, su principal objetivo es jugar para maximizar su data, es decir, tener siempre en la mano la mayor variedad de fichas posibles, para tener menos posiblidades de pasarse. Luego de organizar los movimientos bajo este criterio, llama al método `Average()` usando un `IEnumerable<Move<T>>` organizado con un criterio botagorda al 50%, es decir con un valor de `double` de 0.5.
* `SupportPlayer<T>`: Este es un jugador que sirve de apoyo a sus compañeros de equipo. 
Ordena los movimientos bajo 3 criterios:

1-) A cuántos de sus compañeros podría pasar al dejar un `Tail` de ficha específico por un turno, prioriza minimizar este valor.

2-) Jugar por fichas por las cuales algunos de sus compañeros de equipo estén pasados. Intenta maximizar ese valor.

3-) Evitar jugar por movimientos hechos por compañeros de equipo. Intenta minimzar este valor.

Luego de ordenar estos valores, se hace un `Average()` entre todos ellos, cada uno con una relevancia del 100%, por último una comparación más con un criterio de botagorda al 50%.
* `DesablerPlayer<T>`: Este es un jugador cuyo único objetivo es intentar pasar a los jugadores de los equipos rivales. Ordena los movimientos bajo 3 criterios:

1-) A cuántos jugadores rivales podría pasar al dejar un `Tail` de ficha específico por un turno, prioriza miximizar este valor.

2-) No jugar por fichas por las cuales algunos de los jugadores rivales estén pasados. Intenta minimizar ese valor.

3-) PRiorizar jugar por movimientos hechos por jugadores rivales. Intenta minimzar este valor.

Luego de ordenar estos valores, se hace un `Average()` entre todos ellos, cada uno con una relevancia del 100%, por último una comparación más con un criterio de botagorda al 50%.

* `SmartPlayer<T>`: Este jugador tiene cierta heurística para jugar. Tiene disponible la forma de organizar jugadas del los players `CarrierPlayer<T>`, `SupportPlayer<T>`, `DesablerPlayer<T>`. El player hace un pequeño análisis de la sitación de juego y hace un `Average` entre los tres IEnumerable, y juega en corresponecia con el análisis hecho.