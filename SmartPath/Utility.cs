using System;
using System.Collections.Generic;
using System.Linq;

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

		/// <summary>
		///		Enumerates all Ancestors from the root until and including this item.
		/// </summary>
		internal static IEnumerable<AbsoluteDirectory> GetAncestorsAndSelf(AbsoluteDirectory dir)
		{
			return GetAncestors(dir).Concat(EnumerableForSingleItem(dir));
		}

		/// <summary>
		///		Enumerates all Ancestors from the root until but excluding this item.
		/// </summary>
		internal static IEnumerable<AbsoluteDirectory> GetAncestors(AbsoluteDirectory dir)
		{
			var parent = dir.AbsoluteParent;
			if (parent.IsEmpty)
				return Enumerable.Empty<AbsoluteDirectory>();

			var parentAncestors = GetAncestors(parent);

			var parentAsEnumerable = EnumerableForSingleItem(parent);

			return parentAncestors.Concat(parentAsEnumerable);

		}

		/// <summary>
		///		Enumerates all Ancestors from the root until and including this item.
		/// </summary>
		internal static IEnumerable<RelativeDirectory> GetAncestorsAndSelf(RelativeDirectory dir)
		{
			var self = Enumerable.Empty<RelativeDirectory>();
			if (!dir.IsEmpty)
				self = EnumerableForSingleItem(dir);
			return GetAncestors(dir).Concat(self);
		}

		/// <summary>
		///		Enumerates all Ancestors from the root until but excluding this item.
		/// </summary>
		internal static IEnumerable<RelativeDirectory> GetAncestors(RelativeDirectory dir)
		{
			var parent = dir.Parent;
			if (parent.IsEmpty)
				return Enumerable.Empty<RelativeDirectory>();

			var parentAncestors = GetAncestors(parent);

			var parentAsEnumerable = EnumerableForSingleItem(parent);

			return parentAncestors.Concat(parentAsEnumerable);

		}

		internal static IEnumerable<string> DirectoriesInString(string relativePath, bool addFileMatchAsDirectory=true)
		{
			var match = WindowsPathDetails.RelativePathRegex.Match(relativePath);
			if (!match.Success || match.Groups["root"].Success)
				throw new PathInvalidException("The give path was not just a relative folder: " + relativePath);

			IEnumerable<string> folders = Enumerable.Empty<string>();

			if (match.Groups["folders"].Success)
			{
				folders = match.Groups["folders"].Value.Split(WindowsPathDetails.DirectorySeparator)
												 .Where(item => !String.IsNullOrWhiteSpace(item))
												 .Select(item => item.Trim());
				
			}
			if (match.Groups["file"].Success && addFileMatchAsDirectory)
			{
				folders = folders.Concat(EnumerableForSingleItem(match.Groups["file"].Value.Trim()));
			}
			return folders;
		}

		internal static RelativeDirectory GetRelativeDirectoryParentForDirectory(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return RelativeDirectory.Empty;

			var directories = DirectoriesInString(path, true).ToList();
			if (directories.Count <= 1)
				return RelativeDirectory.Empty;

			return new RelativeDirectory(String.Join(WindowsPathDetails.DirectorySeparator.ToString(), directories.Take(directories.Count - 1)));
		}

		internal static RelativeDirectory GetRelativeDirectoryParentForFile(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
				return RelativeDirectory.Empty;

			var directories = DirectoriesInString(path, false).ToList();
			if (directories.Count == 0)
				return RelativeDirectory.Empty;

			return new RelativeDirectory(String.Join(WindowsPathDetails.DirectorySeparator.ToString(), directories));
		}

		public static bool IsEntirePathIsDescendingOnly(RelativeDirectory directory)
		{
			if (directory.IsEmpty)
				return true;
			return !StringComparer.InvariantCultureIgnoreCase.Equals(directory.DirectoryName, RelativeDirectory.UpOneDirectory.DirectoryName)
					&& (directory.Parent.IsEmpty 
						|| IsEntirePathIsDescendingOnly(directory.Parent));
		}
	}
}
