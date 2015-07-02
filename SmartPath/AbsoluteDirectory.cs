using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text.RegularExpressions;

namespace HTS.SmartPath
{
	/// <summary>
	///		Represents an absolute filesystem directory path. Valid paths are
	///		windows NTFS paths starting with a drive letter (C:\test\) or SMB Shares
	///		(\\Server\share\somedir). Whenever an AbsoluteDirectory is converted
	///		back into a string, a backslash character will be appended to distinguish
	///		it from filenames
	/// </summary>
	public struct AbsoluteDirectory : IEquatable<AbsoluteDirectory>
	{
		/// <summary>
		///		Default value for this type that represents no directory.
		/// </summary>
		public static AbsoluteDirectory Empty = new AbsoluteDirectory();

		private readonly string m_AbsolutePath;
		private readonly string m_DirectoryNameOnly;

		/// <summary>
		///		Gets a <see cref="System.IO.DirectoryInfo"/>.
		/// </summary>
		public DirectoryInfo DirectoryInfo
		{
			get
			{
				return new DirectoryInfo(AbsolutePath);
			}
		}

		/// <summary>
		///		Gets the parent directory or Empty if this is a root directory.
		/// </summary>
		public AbsoluteDirectory AbsoluteParent
		{
			get
			{
				if (!IsRoot)
				{
					var indexOfParentSeparator = AbsolutePath.LastIndexOf(WindowsPathDetails.DirectorySeparator, AbsolutePath.Length - 2, AbsolutePath.Length - 1);
					
					return new AbsoluteDirectory(AbsolutePath.Substring(0, indexOfParentSeparator));
				}
				else
					return Empty;
			}
		}

		/// <summary>
		///		This absolute path as a string or the empty string if this is the Empty path.
		/// </summary>
		public string AbsolutePath { get { return m_AbsolutePath ?? ""; } }

		/// <summary>
		///		The name of this directory, without any parent directories or sepration characters. Empty string if this is the Empty path.
		/// </summary>
		public string DirectoryName { get { return m_DirectoryNameOnly ?? ""; } }

		/// <summary>
		///		Tells if this directory is a root directory
		/// </summary>
		public bool IsRoot { get { return string.IsNullOrEmpty(m_DirectoryNameOnly); } }

		/// <summary>
		///		Indicates if the path is Empty
		/// </summary>
		public bool IsEmpty
		{
			get { return string.IsNullOrEmpty(m_AbsolutePath); }
		}

		/// <summary>
		///		Flag that indicates if this path is valid. At the moment this simply means if it is not empty,
		///		as all invalid characters etc would already be rejected by the contsturctor.
		/// </summary>
		public bool IsValid { get { return !IsEmpty; } }

		/// <summary>
		///		Initializing Contstructor. Takes a string and creates an absolute directory from it.
		///		Attention: throws an exception if the path is invalid.		
		/// </summary>
		/// <param name="absolutePath">The path to turn into an AbsoluteDirectory. Has to be rooted.</param>
		/// <exception cref="PathInvalidException">Thrown when the given path string is empty or does not conform to our path regex</exception>
		internal AbsoluteDirectory(string absolutePath)
		{
			if (string.IsNullOrEmpty(absolutePath))
				throw new PathInvalidException("Given path was empty");

			absolutePath = absolutePath.Trim();

			var match = WindowsPathDetails.AbsolutePathRegex.Match(absolutePath);
			if (!match.Success)
				throw new PathInvalidException("Path was invalid: " + absolutePath);

			m_AbsolutePath = PathUtilities.EnsureEndsWithBackslash(absolutePath);

			m_DirectoryNameOnly = ExtractDirectoryName(match);
		}

		internal AbsoluteDirectory(AbsoluteDirectory parent, string relativePath)
		{
			if (parent.IsEmpty)
				throw new ArgumentException("Parent can not be empty!");

			relativePath = relativePath.Trim();

			m_AbsolutePath = parent.AbsolutePath + PathUtilities.EnsureEndsWithBackslash(relativePath);
			m_DirectoryNameOnly = relativePath;
		}

		/// <summary>
		///		Returns an AbsoluteDirectory representing the given path or the Empty element if either
		///		the string is empty or null or if it contained illegal characters (=did not match
		///		the Regex). Doesn't care if the directory actually exists or not.
		/// </summary>
		/// <param name="absolutePath">The path to create from. Should not be null and should be a rooted path.</param>
		/// <param name="throwExceptionOnInvalidPath">Flag to indicate if an exception should be thrown if the path was invalid.</param>
		/// <returns>An AbsoluteDirectory representing the given path or the Empty element.</returns>
		/// <exception cref="PathInvalidException">Thrown if the parameter throwExceptionOnInvalidPath is true.</exception>
		public static AbsoluteDirectory FromAbsolutePath(string absolutePath, bool throwExceptionOnInvalidPath = false)
		{
			if (!throwExceptionOnInvalidPath && string.IsNullOrWhiteSpace(absolutePath))
				return Empty;
			try
			{
				return new AbsoluteDirectory(absolutePath);
			}
			catch (PathInvalidException)
			{
				if (throwExceptionOnInvalidPath)
					throw;

				return Empty;
			}
		}

		/// <summary>
		///		Returns an AbsoluteDirectory representing the given path if it is an absolute path,
		///		or, if the path is relative, returns the relative path appended to the given workingdirectory.
		/// </summary>
		/// <param name="path">The path to create from. Should not be null.</param>
		/// <param name="workingDirectory">The working directory to use as a base if path is a relative directory.</param>
		/// <returns>A AbsoluteDirectory representing the given path or Empty if the path was invalid.</returns>
		public static AbsoluteDirectory FromAbsoluteOrRelativePath(string path, AbsoluteDirectory workingDirectory)
		{
			var absolutePath = FromAbsolutePath(path);

			if (!absolutePath.IsEmpty)
				return absolutePath;

			var relativeDirectory = RelativeDirectory.FromPathString(path);

			if (relativeDirectory.IsValid)
				return workingDirectory.CreateDirectoryPath(relativeDirectory);
			return Empty;
		}

		/// <summary>
		///		Checks if the directory actually exists on the filesystem.
		/// </summary>
		/// <returns>True if the directory exists.</returns>
		public bool Exists()
		{
			return Directory.Exists(AbsolutePath);
		}

		/// <summary>
		///		Creates an absolutefilename under this directory. 
		/// </summary>
		/// <param name="relativeFilePath">The name of the file to create an AbsoluteFilename path for under this directory path. 
		///	The filename can have optional directories preceding it and separated by \.  An Exception is thrown if this parameter is null
		/// or invalid.
		/// </param>
		/// <returns>The new AbsoluteFilename.</returns>
		/// <exception cref="ArgumentNullExcpetion"></exception>
		/// <exception cref="PathInvalidException"></exception>
		public AbsoluteFilename CreateFilename(string relativeFilePath)
		{
			return CreateFilename(RelativeFilename.FromPathString(relativeFilePath, throwExceptionForInvalidPaths: true));
		}

		/// <summary>
		///		Creates an AbsoluteFilename under this directory. 
		/// </summary>
		/// <param name="filename">The name of the file to create an AbsoluteFilename path for under this directory path. 
		///	The filename can have optional directories preceding it and separated by \. An Exception is thrown if this parameter is Empty.
		/// </param>
		/// <returns>The new AbsoluteFilename.</returns>
		/// <exception cref="ArgumentNullExcpetion"></exception>
		/// <exception cref="PathInvalidException"></exception>
		public AbsoluteFilename CreateFilename(RelativeFilename filename)
		{
			return CombineWithRelativePath(filename);
		}

		/// <summary>
		///		Creates an AbsoluteDirectory under this directory. 
		/// </summary>
		/// <param name="directoryname">The name of the directory to create an AbsoluteDirectory path for under this directory path. 
		///	The directory can have optional directories preceding it and separated by \.  An Exception is thrown if this parameter is null or empty
		/// or invalid.
		/// </param>
		/// <returns>The new AbsoluteDirectory.</returns>
		/// <exception cref="PathInvalidException"></exception>
		public AbsoluteDirectory CreateDirectoryPath(string directoryname)
		{
			return CreateDirectoryPath(RelativeDirectory.FromPathString(directoryname));
		}

		/// <summary>
		///		Creates an AbsoluteDirectory under this directory. 
		/// </summary>
		/// <param name="directoryname">The name of the directory to create an AbsoluteDirectory path for under this directory path. 
		///	The directory can have optional directories preceding it and separated by \.  An Exception is thrown if this parameter is Empty.
		/// </param>
		/// <returns>The new AbsoluteDirectory.</returns>
		/// <exception cref="PathInvalidException"></exception>
		public AbsoluteDirectory CreateDirectoryPath(RelativeDirectory directoryname)
		{
			return CombineWithRelativePath(directoryname);
		}

		/// <summary>
		///		Tries to create a filename under the current directory, returns false if the filename was invalid.
		/// </summary>
		/// <param name="filename">The name of the file to create an AbsoluteFilename path for under this directory path. 
		///	The filename can have optional directories preceding it and separated by \ </param>
		/// <param name="result">The new AbsoluteFilename.</param>
		/// <returns>True if successful</returns>
		public bool TryCreateFilename(string filename, out AbsoluteFilename result)
		{
			result = AbsoluteFilename.Empty;

			if (string.IsNullOrWhiteSpace(filename))
				return false;

			var asRelativeFilename = RelativeFilename.FromPathString(filename);
			
			return TryCreateFilename(asRelativeFilename, out result);
		}

		/// <summary>
		///		Tries to create an AbsoluteFilename under the current directory, returns false if the filename was invalid.
		/// </summary>
		/// <param name="filename">The name of the file to create an AbsoluteFilename path for under this directory path. </param>
		/// <param name="result">The new AbsoluteFilename.</param>
		/// <returns>True if successful</returns>
		public bool TryCreateFilename(RelativeFilename filename, out AbsoluteFilename result)
		{
			result = AbsoluteFilename.Empty;

			if (filename.IsEmpty)
				return false;

			try
			{
				result = CombineWithRelativePath(filename);

				return true;
			}
			catch (PathInvalidException)
			{
				return false;
			}
		}

		/// <summary>
		///		Tries to create an AbsoluteDirectory under the current directory, returns false if the directoryname was invalid.
		/// </summary>
		/// <param name="filename">The name of the directory to create an AbsoluteFilename path for under this directory path. 
		///	The filename can have optional directories preceding it and separated by \. </param>
		/// <param name="result">The new AbsoluteFilename.</param>
		/// <returns>True if successful</returns>
		public bool TryCreateDirectoryPath(string directoryname, out AbsoluteDirectory result)
		{
			result = Empty;

			if (string.IsNullOrWhiteSpace(directoryname))
				return false;

			var asRelativeDirectory = RelativeDirectory.FromPathString(directoryname);

			return TryCreateDirectoryPath(asRelativeDirectory, out result);
		}

		/// <summary>
		///		Tries to create an AbsoluteDirectory under the current directory, returns false if the directoryname was invalid.
		/// </summary>
		/// <param name="filename">The name of the directory to create an AbsoluteFilename path for under this directory path. 
		///	The filename can have optional directories preceding it and separated by \. </param>
		/// <param name="result">The new AbsoluteFilename.</param>
		/// <returns>True if successful</returns>
		public bool TryCreateDirectoryPath(RelativeDirectory directoryname, out AbsoluteDirectory result)
		{
			result = Empty;

			if (directoryname.IsEmpty)
				return false;

			try
			{
				result = CombineWithRelativePath(directoryname);

				return true;
			}
			catch (PathInvalidException)
			{
				return false;
			}
		}

		/// <summary>
		///		Creates this directory on the actual filesystem.
		/// </summary>
		/// <returns>this, for chaining calls.</returns>
		/// <exception cref="System.IO.IOExeption"></exception>
		/// <exception cref="System.UnauthorizedExcpetion"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		public AbsoluteDirectory CreateFileSystemDirectory()
		{
			// creates this dir and all parent directories necessary
			// this used to be a recursive call that creates parents first,
			// but when something went wrong at a higher level the exception
			// had a less specific path which is not helpful when debugging
			Directory.CreateDirectory(AbsolutePath); 
			
			return this;
		}

		/// <summary>
		///		Delete this directory on the actual filesystem if it exists
		/// </summary>
		/// <exception cref="System.IO.IOExeption"></exception>
		/// <exception cref="System.UnauthorizedExcpetion"></exception>
		/// <exception cref="System.IO.PathTooLongException"></exception>
		/// <exception cref="System.IO.DirectoryNotFoundException"></exception>
		public void DeleteFilesystemItemIfExists()
		{
			if (Exists())
				Directory.Delete(AbsolutePath, true);
		}

		/// <summary>
		///		Enumerates all files matching a given file extension inside this directory (and optionally inside all subdirectories).
		/// </summary>
		/// <remarks>The implementation is lazy using Directory.EnumerateFiles()</remarks>
		/// <param name="filter">The file Extension to filter for, may not be null.</param>
		/// <param name="searchInSubdirectories"></param>
		/// <returns>The AbsoluteFilenames of the files that were found</returns>
		/// <exception cref="ArgumentException">filter was invalid.</exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="SecurityException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public IEnumerable<AbsoluteFilename> GetFileSystemFiles(FileExtension filter, bool searchInSubdirectories = false)
		{
			if (filter.IsEmpty)
				throw new ArgumentNullException("filter");

			return GetFileSystemFiles("*" + filter.AsStringWithDot, searchInSubdirectories);
		}

		/// <summary>
		///		Enumerates all files matching a given file extension inside this directory (and optionally inside all subdirectories).
		/// </summary>
		/// <remarks>The implementation is lazy using Directory.EnumerateFiles()</remarks>
		/// <param name="filter">The file Extension to filter for, may not be null.</param>
		/// <param name="searchInSubdirectories">Flag to indicate if subdirectories should be searched.</param>
		/// <param name="workaround3CharactersSpecialCase">The underlying .NET methods have a weird special case for 3 character long file extensions that ends up
		/// returning all files matching longer file extensions as well (so a search for *.txt will match some.txtsomething). If this flag is set to true it will
		/// filter out filenames with longer extensions.</param>
		/// <returns>The AbsoluteFilenames of the files that were found</returns>
		/// <exception cref="ArgumentException">filter was invalid.</exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="SecurityException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public IEnumerable<AbsoluteFilename> GetFileSystemFiles(string filter="*", bool searchInSubdirectories = false, bool workaround3CharactersSpecialCase = true)
		{
			if (!Exists())
				throw new DirectoryNotFoundException("Tried to get files for a directory that does not exist");

			IEnumerable<string> filePaths = null;

			if (filter == null)
				filter = "*";
			
			filePaths = Directory.EnumerateFiles(AbsolutePath, filter, searchInSubdirectories ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

			// Doing this because of weird MS special case for 3 character long extensions, see http://msdn.microsoft.com/en-us/library/dd383571%28v=vs.110%29.aspx
			if (workaround3CharactersSpecialCase)
			{
				var regexFromFilter = new Regex("^" + filter.Replace("*", @"[^\\/:""<>|\r\n]*").Replace("?", @"[^\\/:""<>|\r\n]?") + "$", RegexOptions.IgnoreCase);
				filePaths = filePaths.Where(path => DoesExtensionReallyMatch(regexFromFilter, path));
			}

			return filePaths.Select(filePath => new AbsoluteFilename(filePath));
		}

		/// <summary>
		///		Enumerates all directories matching a given file extension inside this directory (and optionally inside all subdirectories).
		/// </summary>
		/// <remarks>The implementation is lazy using Directory.EnumerateDirectories()</remarks>
		/// <returns>The AbsoluteFilenames of the files that were found</returns>
		/// <exception cref="ArgumentException">filter was invalid.</exception>
		/// <exception cref="DirectoryNotFoundException"></exception>
		/// <exception cref="IOException"></exception>
		/// <exception cref="PathTooLongException"></exception>
		/// <exception cref="SecurityException"></exception>
		/// <exception cref="UnauthorizedAccessException"></exception>
		public IEnumerable<AbsoluteDirectory> GetFileSystemDirectories()
		{
			if (!Exists())
				throw new DirectoryNotFoundException("Tried to get files for a directory that does not exist");

			var thicCopy = this;
			return Directory.EnumerateDirectories(AbsolutePath).Select(dir => new AbsoluteDirectory(thicCopy, ExtractDirectoryName(dir)));
		}

		/// <summary>
		/// 		Returns (as an out parameter) the RelativeDirectory to get from the given base directory to this directory. Returns false if the two paths have 
		/// 		no common root, otherwise true. If the base directory is identical with this directory, the relativeDirectoryResult is the Empty value.
		/// </summary>
		/// <param name="baseDirectory">Base directory to get the relative path from. Must not be null.</param>
		/// <param name="relativeDirectoryResult">The relativeDirectory that represents the path from the baseDirectory to this directory.</param>
		/// <returns>True if the two paths have a common root directory, otherwise false</returns>
		public bool TryGetRelativePath(AbsoluteDirectory baseDirectory, out RelativeDirectory relativeDirectoryResult)
		{
			relativeDirectoryResult = RelativeDirectory.Empty;

			if (baseDirectory.IsEmpty)
				throw new ArgumentNullException("baseDirectory");

			var ancestors = PathUtilities.GetAncestorsAndSelf(this).ToList();
			var baseDirAncestors = PathUtilities.GetAncestorsAndSelf(baseDirectory).ToList();

			var zippedAncestors = PathUtilities.ZipExhaustive(baseDirAncestors, ancestors).ToList();

			var differentDirectories = new List<string>();

			// If the first item is different then the paths do not share a common root, return null
			if (zippedAncestors.Any() && !zippedAncestors[0].Item1.IsEmpty && !zippedAncestors[0].Item2.IsEmpty && !zippedAncestors[0].Item1.Equals(zippedAncestors[0].Item2))
				return false;

			foreach (var ancestorPair in zippedAncestors)
			{
				bool ancestorsEqual = false;
				if (!ancestorPair.Item1.IsEmpty && !ancestorPair.Item2.IsEmpty)
					ancestorsEqual = ancestorPair.Item1.Equals(ancestorPair.Item2);

				if (ancestorPair.Item1.IsEmpty)
					differentDirectories.Add(ancestorPair.Item2.DirectoryName);
				else if (ancestorPair.Item2.IsEmpty)
					differentDirectories.Insert(0, "..");
				else if (!ancestorsEqual)
				{
					differentDirectories.Insert(0, "..");
					differentDirectories.Add(ancestorPair.Item2.DirectoryName);
				}
			}

			if (differentDirectories.Any())
			{
				RelativeDirectory lastDirectory = RelativeDirectory.Empty;
				foreach (var dir in differentDirectories)
					lastDirectory = new RelativeDirectory(lastDirectory, dir);
				relativeDirectoryResult = lastDirectory;
			}
			return true;

		}

		/// <summary>
		///		Returns the RelativeDirectory to get from the given base directory to this directory. Returns null if the two paths have 
		///		no common root or <see cref="RelativeDirectory.Empty"/> if the base directory is identical with this directory.
		/// </summary>
		/// <param name="baseDirectory"></param>
		/// <returns></returns>
		public RelativeDirectory? GetRelativePathOrNull(AbsoluteDirectory baseDirectory)
		{
			RelativeDirectory result;
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
			if (!(obj is AbsoluteDirectory))
				return false;

			AbsoluteDirectory other = (AbsoluteDirectory) obj;

			return Equals(other);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(AbsoluteDirectory other)
		{
			if (IsEmpty ^ other.IsEmpty) // it either is empty but not the other
				return false;

			if (IsEmpty && other.IsEmpty)
				return true;

			return StringComparer.InvariantCultureIgnoreCase.Equals(m_AbsolutePath, other.m_AbsolutePath);
		}

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		public override int GetHashCode()
		{
			return m_AbsolutePath.ToLower().GetHashCode();
		}

		/// <summary>
		/// Returns the absolute path of this instance
		/// </summary>
		/// <returns>
		/// The absolute path.
		/// </returns>
		public override string ToString()
		{
			return m_AbsolutePath;
		}

		/// <summary>
		///		Combines an AbsoluteDirectory with a RelativeDirectory. BaseDir may not be Empty.
		/// </summary>
		/// <param name="baseDir"></param>
		/// <param name="relDir"></param>
		/// <returns>The combined AbsoluteDirectory</returns>
		public static AbsoluteDirectory operator +(AbsoluteDirectory baseDir, RelativeDirectory relDir)
		{
			if (baseDir.IsEmpty)
				throw new ArgumentException("baseDir must not be empty");

			return baseDir.CombineWithRelativePath(relDir);
		}

		/// <summary>
		///		Combines an AbsoluteDirectory with a RelativeFilename. BaseDir may not be Empty.
		/// </summary>
		/// <param name="baseDir"></param>
		/// <param name="relDir"></param>
		/// <returns>The combined AbsoluteFilename</returns>
		public static AbsoluteFilename operator +(AbsoluteDirectory baseDir, RelativeFilename relFile)
		{
			if (baseDir.IsEmpty)
				throw new ArgumentNullException("baseDir");

			return baseDir.CombineWithRelativePath(relFile);
		}

		public static bool operator ==(AbsoluteDirectory one, AbsoluteDirectory two)
		{
			return one.Equals(two);
		}

		public static bool operator !=(AbsoluteDirectory one, AbsoluteDirectory two)
		{
			return !(one == two);
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(AbsoluteDirectory one, AbsoluteDirectory? two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(AbsoluteDirectory one, AbsoluteDirectory? two)
		{
			return true;
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(AbsoluteDirectory? one, AbsoluteDirectory two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(AbsoluteDirectory? one, AbsoluteDirectory two)
		{
			return true;
		}

		private AbsoluteDirectory CombineWithRelativePath(RelativeDirectory relDir)
		{
			if (relDir.IsEmpty)
				throw new PathInvalidException("The given path was empty");

			AbsoluteDirectory currentParent = this;

			foreach (var item in PathUtilities.GetAncestorsAndSelf(relDir))
			{
				if (item.Equals(RelativeDirectory.UpOneDirectory))
					currentParent = currentParent.AbsoluteParent;
				else if (!item.Equals(RelativeDirectory.Empty))
					currentParent = new AbsoluteDirectory(currentParent, item.DirectoryName);
			}

			return currentParent;
		}

		private AbsoluteFilename CombineWithRelativePath(RelativeFilename relFile)
		{
			if (relFile.IsEmpty)
				throw new PathInvalidException("The given path was empty");

			AbsoluteDirectory currentParent = this;
			if (PathUtilities.GetAncestorsAndSelf(relFile.Parent).Any())
				currentParent = CombineWithRelativePath(relFile.Parent);

			return new AbsoluteFilename(currentParent, relFile.FilenameWithExtension);
		}

		private static string ExtractDirectoryName(string absolutePath)
		{
			return ExtractDirectoryName(WindowsPathDetails.AbsolutePathRegex.Match(absolutePath));
		}

		private static string ExtractDirectoryName(Match match)
		{
			string relativePath;

			if (match.Groups["file"].Success)
			{
				relativePath = match.Groups["file"].Value;
			}
			else
			{
				// if the path ended with a \, the last split will be empty and we need to take second to last string
				var nonEmptyFolders = match.Groups["folders"].Value.Split('\\').Where(part => !string.IsNullOrWhiteSpace(part)).ToList();
				relativePath = nonEmptyFolders.LastOrDefault();
			}
			return relativePath;
		}

		private static bool DoesExtensionReallyMatch(Regex regexFromFilter, string path)
		{
			var filename = WindowsPathDetails.FilenameRegex.Match(path).Groups["Filename"].Value;
			return regexFromFilter.IsMatch(filename);
		}
	}
}
