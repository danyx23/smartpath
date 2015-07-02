using System;

namespace HTS.SmartPath
{
	/// <summary>
	///		Represents a relative filename (one that doesn't have a root), e.g. "filename.txt". Can also
	///		include relative parent directories (e.g. "somedir\filename.txt") or even parent directory
	///		specifiers (e.g. "..\filename.txt")
	/// </summary>
	public struct RelativeFilename : IEquatable<RelativeFilename>
	{
		private readonly string m_EntireRelativePath;
		private readonly string m_Filename;

		/// <summary>
		///		Default value for this type that represents no filename.
		/// </summary>
		public static RelativeFilename Empty = new RelativeFilename();

		/// <summary>
		///		The filename including the file extension or an empty string for the Empty value.
		/// </summary>
		public string FilenameWithExtension { get { return m_Filename ?? ""; } }

		/// <summary>
		///		The filename without the file extension or the empty string for the Empty value.
		/// </summary>
		public string FilenameWithoutExtension
		{
			get
			{
				if (m_Filename == null)
					return "";

				var lastIndexOfDot = m_Filename.LastIndexOf(".");
				if (lastIndexOfDot >= 0)
					return m_Filename.Substring(0, lastIndexOfDot);
				return m_Filename;
			}
		}

		/// <summary>
		///		The full path of this relative Filename (i.e. with the relative directories
		///		that were included in the construction or that it was constructed from), or
		///		an empty string for the Empty value.
		/// </summary>
		public string FullPath { get { return m_EntireRelativePath ?? ""; } }

		/// <summary>
		///		The FileExtension of this filename or the Empty Fileextension if it doesn't have one.
		/// </summary>
		public FileExtension Extension
		{
			get
			{
				if (m_Filename == null)
					return FileExtension.Empty;

				var extensionMatch = WindowsPathDetails.FileExtensionRegex.Match(m_Filename);

				if (!extensionMatch.Success)
					return FileExtension.Empty;
				return new FileExtension(extensionMatch.Groups["Extension"].Value);
			}
		}

		/// <summary>
		///		Indicates if the path is Empty
		/// </summary>
		public bool IsEmpty { get { return string.IsNullOrEmpty(m_EntireRelativePath); } }

		/// <summary>
		///		Flag that indicates if this path is valid. At the moment this simply means if it is not empty,
		///		as all invalid characters etc would already be rejected by the contsturctor.
		/// </summary>
		public bool IsValid { get { return !IsEmpty; } }

		/// <summary>
		///		The ParentDirectory of this Element or the Empty value if no parent directory was specified.
		/// </summary>
		public RelativeDirectory Parent
		{
			get { return PathUtilities.GetRelativeDirectoryParentForFile(m_EntireRelativePath); }
		}

		internal RelativeFilename(RelativeDirectory parent, string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath))
				throw new PathInvalidException("The filename was empty");

			relativePath = relativePath.Trim();

			var match = WindowsPathDetails.RelativePathRegex.Match(relativePath);
			if (!match.Success)
				throw new PathInvalidException("The given path contained illegal characters: " + relativePath);
			if (match.Groups["root"].Success || match.Groups["folders"].Success || !match.Groups["file"].Success)
				throw new PathInvalidException("The given path was not just a relative filename: " + relativePath);

			m_Filename = relativePath;
			m_EntireRelativePath = parent.FullPath + m_Filename;
		}

		internal RelativeFilename(string relativePath)
		{
			if (string.IsNullOrWhiteSpace(relativePath))
				throw new PathInvalidException("The filename was empty");

			relativePath = relativePath.Trim();

			var match = WindowsPathDetails.RelativePathRegex.Match(relativePath);

			if (!match.Success)
				throw new PathInvalidException("The given path contained illegal characters: " + relativePath);

			if (match.Groups["root"].Success || !match.Groups["file"].Success)
				throw new PathInvalidException("The given path was not just a relative filename: " + relativePath);

			string basePath = "";

			if (match.Groups["folders"].Success)
			{
				basePath = PathUtilities.EnsureEndsWithBackslash(RelativeDirectory.FromPathString(match.Groups["folders"].Value).FullPath);
			}

			m_Filename = match.Groups["file"].Value;
			m_EntireRelativePath = basePath + m_Filename;
		}

		/// <summary>
		///		Creates a RelativeFilenamne from a given string. Can contain folders, separated by \, these will then be created
		///		as <see cref="RelativeDirectory"/>ies and set as the parent/ancestors of this filename.
		/// </summary>
		/// <param name="relativePath">Path to create the filename from.</param>
		/// <param name="throwExceptionForInvalidPaths">Flag to indicate if an exception should be thrown in the path is invalid.</param>
		/// <returns>The RelativeFilename constructed from relativePath or null.</returns>
		/// <exception cref="PathInvalidException">The parameter throwExceptionForInvlalidPaths is true and the path was invalid.</exception>
		public static RelativeFilename FromPathString(string relativePath, bool throwExceptionForInvalidPaths = false)
		{
			if (!throwExceptionForInvalidPaths && string.IsNullOrWhiteSpace(relativePath))
				return Empty;

			try
			{
				return new RelativeFilename(relativePath);
			}
			catch (PathInvalidException)
			{
				if (throwExceptionForInvalidPaths)
					throw;

				return Empty;
			}
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
			if (!(obj is RelativeFilename))
				return false;

			return Equals((RelativeFilename)obj);
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
			return FullPath.ToLower().GetHashCode();
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(RelativeFilename other)
		{
			if (IsEmpty ^ other.IsEmpty) // it either is empty but not the other
				return false;

			if (IsEmpty && other.IsEmpty)
				return true;

			return StringComparer.InvariantCultureIgnoreCase.Equals(m_EntireRelativePath, other.m_EntireRelativePath);
		}

		/// <summary>
		/// Returns the fully qualified type name of this instance.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.String"/> containing a fully qualified type name.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			return FullPath;
		}

		public static bool operator ==(RelativeFilename one, RelativeFilename two)
		{
			return one.Equals(two);
		}

		public static bool operator !=(RelativeFilename one, RelativeFilename two)
		{
			return !(one == two);
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(RelativeFilename one, RelativeFilename? two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(RelativeFilename one, RelativeFilename? two)
		{
			return true;
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(RelativeFilename? one, RelativeFilename two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(RelativeFilename? one, RelativeFilename two)
		{
			return true;
		}
	}
}
