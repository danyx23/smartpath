using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HTS.SmartPath;
using HTS.SmartPath.PathFragments;
using NUnit.Framework;

namespace SmartPathTests
{
	internal static class TestUtilities
	{
		public static void AssertCollectionEqual<T>(IEnumerable<T> first, IEnumerable<T> second)
		{
			CollectionAssert.AreEqual(first, second);
		}

		public static RelativeDirectory GetParent(RelativeDirectory dir)
		{
			if (dir.PathFragments.Count() <= 1)
				return RelativeDirectory.Empty;
			var parentFragments = dir.PathFragments.Take(dir.PathFragments.Count() - 1);
			return RelativeDirectory.FromPathFragments(parentFragments);
		}

		public static RelativeDirectory GetParent(RelativeFilename dir)
		{
			if (dir.PathFragments.Count() <= 1)
				return RelativeDirectory.Empty;
			var parentFragments = dir.PathFragments.Take(dir.PathFragments.Count() - 1);
			return RelativeDirectory.FromPathFragments(parentFragments);
		}

		public static IEnumerable<TAccumulator> Scan<TEnumerable, TAccumulator>(this IEnumerable<TEnumerable> input, Func<TAccumulator, TEnumerable, TAccumulator> next, TAccumulator state)
		{
			yield return state;
			foreach (var item in input)
			{
				state = next(state, item);
				yield return state;
			}
		}
	}
}
