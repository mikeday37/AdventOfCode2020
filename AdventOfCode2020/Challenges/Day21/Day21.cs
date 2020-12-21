using AdventOfCodeScaffolding;
using AdventOfCode2020.ThirdParty.RosettaCode;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace AdventOfCode2020.Challenges.Day21
{
	[Challenge(21, "Allergen Assessment")]
	public class Day21Challenge : ChallengeBase
	{
		public record FoodLabel
		{
			public int LineNumber { get; }
			public IReadOnlySet<string> Ingredients { get; }
			public IReadOnlySet<string> Allergens { get; }

			public FoodLabel(string line, int index)
			{
				var a = line.Split(" (contains ");
				Ingredients = a[0].Split(' ').ToHashSet();
				Allergens = a[1].TrimEnd(')').Split(", ").ToHashSet();
				LineNumber = index + 1;
			}

			public static IEnumerable<FoodLabel> ParseAll(string input)
			{
				return input.ToLines().WithIndex().Select((x, i) => new FoodLabel(x.item, x.index));
			}
		}

		public class Ingredient
		{
			public List<FoodLabel> Labels { get; }
			public string Name { get; set; }
			public Ingredient() => (Labels, PotentialAllergens) = (new(), new());
			public HashSet<string> PotentialAllergens {get;}
		}

		public class Allergen
		{
			public List<FoodLabel> Labels { get; }
			public string Name { get; set; }
			public Allergen() => Labels = new();
		}

		public override object Part1(string input)
		{
			Dictionary<string, Ingredient> ingredients = new();
			Dictionary<string, Allergen> allergens = new();
			Dictionary<int, FoodLabel> labels = new();

			// for each food label, build a dictionary for each ingredient and allergen -> list of labels it appears in
			foreach (var label in FoodLabel.ParseAll(input))
			{
				labels[label.LineNumber] = label;
				foreach (var ingredient in label.Ingredients)
				{
					if (!ingredients.ContainsKey(ingredient))
						ingredients[ingredient] = new Ingredient{Name = ingredient};
					ingredients[ingredient].Labels.Add(label);
				}
				foreach (var allergen in label.Allergens)
				{
					if (!allergens.ContainsKey(allergen))
						allergens[allergen] = new Allergen{Name = allergen};
					allergens[allergen].Labels.Add(label);
				}
			}

			// for each allergen
			foreach (var allergen in allergens.Values)
			{
				// get a list of all ingredient lists for foods known to have this allergen
				var listOfLists = allergen
					.Labels
					.Select(x => x.Ingredients.ToList())
					.ToList();

				// get the intersection of those lists
				var intersection = listOfLists
					.Skip(1)
					.Aggregate(
						new HashSet<string>(listOfLists[0]),
						(a, b) => { a.IntersectWith(b); return a; }
					);

				// mark all in the intersection as potentially the allergen
				foreach (var ingredient in intersection)
					ingredients[ingredient].PotentialAllergens.Add(allergen.Name);
			}

			// determine ingredients which can't contain any of the known allergens
			var safeIngredients = ingredients
				.Values
				.Where(x => 0 == x.PotentialAllergens.Count)
				.Select(x => x.Name)
				.ToHashSet();

			// return total count of appearances of any safe ingredient in any food label
			return labels
				.Values
				.Select(label => label
					.Ingredients
					.Count(i => safeIngredients.Contains(i))
				)
				.Sum();
		}

		public override object Part2(string input)
		{
			return -1;
		}

	}
}
