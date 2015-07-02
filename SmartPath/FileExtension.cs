using System;

namespace HTS.SmartPath
{
	/// <summary>
	///		Represents a file extension, i.e. the part after the last dot in a filename. It is basically just a wrapper
	///		around a string but provides type safety and convenience properties to handle getting a string including 
	///		and excluding the dot
	/// </summary>
	public struct FileExtension : IEquatable<FileExtension>
	{
		/// <summary>
		///		Default value for this type that represents no fileextension.
		/// </summary>
		public static FileExtension Empty = new FileExtension();

		private readonly string m_Extension;

		/// <summary>
		///		Constructor that constructs from a file extension string (without the leading dot)
		/// </summary>
		/// <param name="extensionWithoutDot"></param>
		/// <exception cref="PathInvalidException">The extension started with a dot.</exception>
		public FileExtension(string extensionWithoutDot)
		{
			if (extensionWithoutDot.StartsWith("."))
				throw new PathInvalidException("File extensions must not start with a dot");

			m_Extension = extensionWithoutDot.Trim();
		}

		/// <summary>
		///		The extension as a string with the leading dot.
		/// </summary>
		public string AsStringWithDot
		{
			get
			{
				return "." + AsStringWithoutDot;
			}
		}

		/// <summary>
		///		The extension as a string without the leading dot.
		/// </summary>
		public string AsStringWithoutDot { get { return m_Extension; } }

		/// <summary>
		///		Indicates if the file extension is Empty
		/// </summary>
		public bool IsEmpty { get { return string.IsNullOrEmpty(m_Extension); } }

		/// <summary>
		///		Flag that indicates if this file extension is valid. At the moment this simply means if it is not empty,
		///		as all invalid characters etc would already be rejected by the contsturctor.
		/// </summary>
		public bool IsValid { get { return !IsEmpty; } }

		/// <summary>
		/// Returns the hash code for this instance.
		/// </summary>
		/// <returns>
		/// A 32-bit signed integer that is the hash code for this instance.
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override int GetHashCode()
		{
			return m_Extension.ToLower().GetHashCode();
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
			if (!(obj is FileExtension))
				return false;

			return Equals((FileExtension) obj);
		}

		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <returns>
		/// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
		/// </returns>
		/// <param name="other">An object to compare with this object.</param>
		public bool Equals(FileExtension other)
		{
			if (IsEmpty ^ other.IsEmpty) // it either is empty but not the other
				return false;

			if (IsEmpty && other.IsEmpty)
				return true;

			return StringComparer.InvariantCultureIgnoreCase.Equals(m_Extension, other.m_Extension);
		}

		/// <summary>
		/// Returns the extension as a string.
		/// </summary>
		/// <returns>
		/// </returns>
		/// <filterpriority>2</filterpriority>
		public override string ToString()
		{
			return AsStringWithoutDot;
		}

		public static bool operator ==(FileExtension one, FileExtension two)
		{
			return one.Equals(two);
		}

		public static bool operator !=(FileExtension one, FileExtension two)
		{
			return !(one == two);
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(FileExtension one, FileExtension? two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(FileExtension one, FileExtension? two)
		{
			return true;
		}

		[Obsolete("Use IsValid instead of comparing this value type to null!")]
		public static bool operator ==(FileExtension? one, FileExtension two)
		{
			return false;
		}

		[Obsolete("Use IsEmpty instead of comparing this value type to null!")]
		public static bool operator !=(FileExtension? one, FileExtension two)
		{
			return true;
		}
	}
}
