using System;

namespace HTS.SmartPath
{
	/// <summary>
	///		Exception that indicates that a path was invalid (e.g. because it contained illegal characters).
	/// </summary>
	public class PathInvalidException : Exception
	{
		public PathInvalidException(string message) : base(message)
		{
		}

		public PathInvalidException(string message, Exception innerException) : base(message, innerException)
		{
		}
	}
}
