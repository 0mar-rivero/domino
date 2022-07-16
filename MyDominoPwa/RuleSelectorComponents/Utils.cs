namespace MyDominoPwa.RuleSelectorComponents;

public static class Utils {
	public static string Space(this string item) =>
		string.Join("", item.Select((character, i) => Normalizer(character, i)).SelectMany(t => t));

	private static IEnumerable<char> Normalizer(char character, int index) =>
		index is 0 ? Enumerable.Repeat(char.ToUpper(character), 1) :
		char.IsUpper(character) ? Enumerable.Repeat(' ', 1).Append(char.ToLower(character)) :
		Enumerable.Repeat(character, 1);
}