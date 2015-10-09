using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using HTS.SmartPath.PathFragments;

namespace HTS.SmartPath
{
	public static class PathUtilities
	{
		// usage: IEnumerableExt.FromSingleItem(someObject);
		internal static IEnumerable<T> EnumerableForSingleItem<T>(T item)
		{
			yield return item;
		}

		internal static IEnumerable<Tuple<T1, T2>> ZipExhaustive<T1, T2>(IEnumerable<T1> first, IEnumerable<T2> second)
		{
			using (var firstEnumerator = first.GetEnumerator())
			using (var secondEnumerator = second.GetEnumerator())
			{
				bool firstMoveWorked = firstEnumerator.MoveNext();
				bool secondMoveWorked = secondEnumerator.MoveNext();

				while (firstMoveWorked || secondMoveWorked)
				{
					var firstItem = firstMoveWorked ? firstEnumerator.Current : default(T1);
					var secondItem = secondMoveWorked ? secondEnumerator.Current : default(T2);
					yield return new Tuple<T1, T2>(firstItem, secondItem);
					firstMoveWorked = firstEnumerator.MoveNext();
					secondMoveWorked = secondEnumerator.MoveNext();
				}
			}
		}

		internal static string EnsureEndsWithBackslash(string path)
		{
			return path.EndsWith(WindowsPathDetails.DirectorySeparator.ToString()) ? path : (path + WindowsPathDetails.DirectorySeparator);
		}

		internal static IEnumerable<PathFragment> GetPathFragments(Match match, bool treatFileMatchAsDirectory)
		{
			if (match.Groups["root"].Success)
				yield return new RootFragment(match.Groups["root"].Value);
			foreach (Capture folder in match.Groups["folders"].Captures)
				yield return new DirectoryFragment(folder.Value.TrimEnd('\\'));
			if (match.Groups["file"].Success)
				if (treatFileMatchAsDirectory)
					yield return new DirectoryFragment(match.Groups["file"].Value);
				else
					yield return new FileFragment(match.Groups["file"].Value);
		}

		public static bool IsEntirePathDescendingOnly(IEnumerable<PathFragment> pathFragments)
		{
			if (pathFragments == null || !pathFragments.Any())
				return true;
			return !pathFragments.Any(fragment => fragment.Equals(DirectoryFragment.UpOneDirectory));
		}

		public static bool IsEntirePathDescendingOnly(IFragmentProvider path)
		{
			return IsEntirePathDescendingOnly(path.PathFragments);
		}
	}
}
