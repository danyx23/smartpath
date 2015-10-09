using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HTS.SmartPath.PathFragments;

namespace HTS.SmartPath
{
	/// <summary>
	///		Represents an absolute filename path. Valid paths are
	///		windows NTFS paths starting with a drive letter (C:\test\) or SMB Shares
	///		(\\Server\share\somedir). 
	/// </summary>
	public struct AbsoluteFilename : IEquatable<AbsoluteFilename>, IFragmentProvider
	{
		/// <summary>
		///		Default value for this type that represents no filename.
		/// </summary>
		public static AbsoluteFilename Empty = new AbsoluteFilename();
		private readonly string m_AbsolutePath;
		private readonly PathFragment[] m_PathFragments;

		/// <summary>
		///		The <see cref="System.IO.FileInfo"/> for this file. Lazily initialized.
		/// </summary>
		public FileInfo FileInfo
		{
			get
			{
				return new System.IO.FileInfo(m_AbsolutePath);
			}
		}

		/// <summary>
		///		Filename including the file extension or an empty string if this is an Empty value.
		/// </summary>
		public string FilenameWithExtension { get { return IsEmpty ? "" : m_PathFragments.Last().Fragment; } }

		/// <summary>
		///		The parent <see cref="AbsoluteDirectory"/>.
		/// </summary>
		public AbsoluteDirectory AbsoluteParent
		{
			get
			{
				if (IsEmpty)
					return AbsoluteDirectory.Empty;

				var indexOfParentSeparator = AbsolutePath.LastIndexOf(WindowsPathDetails.DirectorySeparator, AbsolutePath.Length - 2, AbsolutePath.Length - 1);

				return new AbsoluteDirectory(m_AbsolutePath.Substring(0, indexOfParentSeparator));
			}
		}

		/// <summary>
		///		The absolute path as a string or an empty string if this element is an Emtpy value.
		/// </summary>
		public string AbsolutePath { get { return m_AbsolutePath ?? ""; } }

		/// <summary>
		///		The file extions of this file or the Empty FileExtension value if the file does not have an extension.
		/// </summary>
		public FileExtension Extension
		{
			get
			{
				var extensionMatch = WindowsPathDetails.FileExtensionRegex.Match(AbsolutePath);

				if (!extensionMatch.Success)
					return FileExtension.Empty;
				return new FileExtension(extensionMatch.Groups["Extension"].Value);
			}
		}

		/// <summary>
		///		Indicates if the path is Empty
		/// </summary>
		public bool IsEmpty { get { return string.IsNullOrWhiteSpace(m_AbsolutePath); } }

		/// <summary>
		///		Flag that indicates if this path is valid. At the moment this simply means if it is not empty,
		///		as all invalid characters etc would already be rejected by the contsturctor.
		/// </summary>
		public bool IsValid { get { return !IsEmpty; } }

		/// <summary>
		///		The filename without the file extension (and without the dot of course).
		/// </summary>
		public string FileNameWithoutExtension
		{
			get
			{
				if (IsEmpty)
					return "";

				var filenameWithExtension = FilenameWithExtension;
                var lastIndexOfDot = filenameWithExtension.LastIndexOf(".");
				if (lastIndexOfDot >= 0)
					return filenameWithExtension.Substring(0, lastIndexOfDot);
				return filenameWithExtension;
			}
		}

		/// <summary>
		///	Enumerates the fragments of this path. This will yield nothing for the empty path,
		/// and otherwise one RootFragment for the root and then one DirectoryFragment for every directory and finally a FileFragment
		/// </summary>
		public IEnumerable<PathFragment> PathFragments { get { return m_PathFragments ?? Enumerable.Empty<PathFragment>(); } }

		internal AbsoluteFilename(string absolutePath)
		{
			var match = WindowsPathDetails.AbsolutePathRegex.Match(absolutePath);
			if (!match.Success)
				throw new PathInvalidException("Path was invalid: " + absolutePath);

			if (string.IsNullOrWhiteSpace(match.Groups["file"].Value))
				throw new PathInvalidException("No filename contained in path " + absolutePath);

			m_AbsolutePath = absolutePath;
			m_PathFragments = PathUtilities.GetPathFragments(match, false).ToArray();
		}

		internal AbsoluteFilename(AbsoluteDirectory parent, RelativeFilename relativePath)
		{
			m_AbsolutePath = parent.AbsolutePath + relativePath.FullPath;
			m_PathFragments = parent.PathFragments.Concat(relativePath.PathFragments).ToArray();
		}

		/// <summary>
		///		Returns an AbsoluteFilename representing the given path or the Empty value if either
		///		the string is empty or null or if it contained illegal characters (=did not match
		///		the FullPathRegex). Doesn't care if the file actually exists or not. 
		/// </summary>
		/// <param name="absolutePath">The path to create the AbsoluteFilename from.</param>
		/// <param name="throwExceptionForInvalidPath">Flag to indicate if an exception should be thrown (or null returned) if the given path was invalid.</param>
		/// <returns></returns>
		public static AbsoluteFilename FromAbsolutePath(string path, bool throwExceptionForInvalidPath = false)
		{
			if (!throwExceptionForInvalidPath && string.IsNullOrWhiteSpace(path))
				return Empty;

			try
			{
				return new AbsoluteFilename(path);
			}
			catch (PathInvalidException)
			{
				if (throwExceptionForInvalidPath)
					throw;

				return Empty;
			}
			
		}

		public static AbsoluteFilename FromPathFragments(IEnumerable<PathFragment> fragments, bool throwExceptionForInvalidPaths = false)
		{
			AbsoluteFilename returnVal = Empty;

			if (fragments.Any())
			{
				if (!(fragments.First() is RootFragment))
				{
					if (throwExceptionForInvalidPaths)
						throw new PathInvalidException("AbsoluteFilename must start with a root");
				}
				else if (!(fragments.Last() is FileFragment))
				{
					if (throwExceptionForInvalidPaths)
						throw new PathInvalidException("AbsoluteFilename must end with a file");
				}
				else if (fragments.Count() > 2 && !fragments.Skip(1).Take(fragments.Count() - 2).All(fragment => fragment is DirectoryFragment))
				{
					if (throwExceptionForInvalidPaths)
						throw new PathInvalidException("AbsoluteFilename must only contain directories in between the root and filename");
				}
				else
					returnVal = new AbsoluteFilename(string.Join("", fragments.Select(fragment => fragment.ConcatenableFragment)));
			}
			return returnVal;
		}

		/// <summary>
		///		Returns a new AbsoluteFilename with the given file extension. If the file extension is invalid, 
		///		the unchanged instance is returned.
		/// </summary>
		/// <param name="extension">Extension to change the file to.</param>
		/// <returns></returns>
		public AbsoluteFilename WithChangedExtension(FileExtension extension)
		{
			if (extension.IsValid)
			{
				return AbsoluteParent.CreateFilename(FileNameWithoutExtension + extension.AsStringWithDot);
			}
			else
				return this;
		}

		public bool IsBelow(AbsoluteDirectory suspectedDirectoryAbove)
		{
			if (this.IsEmpty)
				return false;

			if (suspectedDirectoryAbove.IsEmpty)
				return false;

			RelativeFilename relativeFilename;
			if (TryGetRelativePath(suspectedDirectoryAbove, out relativeFilename))
				return PathUtilities.IsEntirePathDescendingOnly(relativeFilename);
			return false;
		}

		/// <summary>
		///		Checks if a file with this name actually exists in the filesystem.
		/// </summary>
		/// <returns></returns>
		public bool Exists()
		{
			return File.Exists(AbsolutePath);
		}

		/// <summary>
		///		Deletes the file if it exists.
		/// </summary>
		/// <exception cref="System.IO.IOExeption"></exception>
		/// <exception cref="System.UnauthorizedExcpetion"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		public void DeleteFilesystemItemIfExists()
		{
			if (Exists())
				File.Delete(AbsolutePath);
		}

		/// <summary>
		///		Returns just the filename (without any of the directories) as a <see cref="RelativeFilename"/>, or the
		///		RelativeFilename.Empty value if this instance is the empty value.
		/// </summary>
		/// <returns></returns>
		public RelativeFilename AsRelativeFilename()
		{
			if (IsEmpty)
				return RelativeFilename.Empty;

			return new RelativeFilename(FilenameWithExtension);
		}

		/// <summary>
		///		Returns, as an out parameter, the <see cref="RelativeFilename"/> relative to the given base directory. If this filename and the 
		///		given baseDirectory do not share a common root, false is returned.
		/// </summary>
		/// <param name="baseDirectory"></param>
		/// <returns>False if the this file does not share the root directory with the given baseDirectory.</returns>
		public bool TryGetRelativePath(AbsoluteDirectory baseDirectory, out RelativeFilename relativeFilename)
		{
			relativeFilename = RelativeFilename.Empty;

			RelativeDirectory relativeDir;
			bool dirsShareRoot = AbsoluteParent.TryGetRelativePath(baseDirectory, out relativeDir);
			if (!dirsShareRoot)
				return false;
			if (relativeDir.IsEmpty)
				relativeFilename = new RelativeFilename(FilenameWithExtension);
			else
				relativeFilename = new RelativeFilename(relativeDir, FilenameWithExtension);
			
			return true;
		}

		/// <summary>
		///		Returns the <see cref="RelativeFilename"/> relative to the given base directory. If this filename and the 
		///		given baseDirectory do not share a common root, null is returned.
		/// </summary>
		/// <param name="baseDirectory"></param>
		/// <returns></returns>
		public RelativeFilename? GetRelativePathOrNull(AbsoluteDirectory baseDirectory)
		{
			RelativeFilename result;
			if (TryGetRelativePath(baseDirectory, out result))
				return result;
			else
				return null;
		}

		/// <summary>
		/// Indicates whether this instance and a specified object are equal.
		/// </summary>
		/// <returns>
		/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
		/// </returns>
		/// <param name="obj">Another object to compare to. </param><filterpriority>2</filterpriority>
		public override bool Equals(object obj)
		{
			if (!(obj is AbsoluteFilename))
				return false;

			return Equals((AbsoluteFilename) obj);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return m_AbsolutePath.ToLower().GetHashCode();
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(AbsoluteFilename other)
		{
			if (IsEmpty ^ other.IsEmpty) // it either is empty but not the other
				return false;

			if (IsEmpty && other.IsEmpty)
				return true;

			return StringComparer.InvariantCultureIgnoreCase.Equals(m_AbsolutePath, other.m_AbsolutePath);
		}

		/// <summary>
		/// Returns the absolute path as a string.
		/// </summary>
		/// <returns>
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			return m_AbsolutePath;
		}

		public static bool operator ==(AbsoluteFilename one, AbsoluteFilename two)
		{
			return one.Equals(two);
		}

		public static bool operator !=(AbsoluteFilename one, AbsoluteFilename two)
		{
			return !(one == two);
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(AbsoluteFilename one, AbsoluteFilename? two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(AbsoluteFilename one, AbsoluteFilename? two)
		{
			return true;
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(AbsoluteFilename? one, AbsoluteFilename two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(AbsoluteFilename? one, AbsoluteFilename two)
		{
			return true;
		}
	}
}
