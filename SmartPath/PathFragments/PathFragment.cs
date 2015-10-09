using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTS.SmartPath
{
	public abstract class PathFragment : IEquatable<PathFragment>
	{
		private string m_Fragment;

		public string Fragment { get { return m_Fragment; } }

		public abstract string ConcatenableFragment { get; }

		public bool IsEmpty { get { return string.IsNullOrWhiteSpace(m_Fragment); } }

		internal PathFragment(string fragment)
		{
			if (fragment == null)
				throw new ArgumentNullException("fragment");

			m_Fragment = fragment;
		}

		public bool Equals(PathFragment other)
		{
			if (other == null)
				return false;

			return GetType().Equals(other.GetType())
				   && StringComparer.InvariantCultureIgnoreCase.Equals(m_Fragment, other.m_Fragment);
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as PathFragment);
		}

		public override int GetHashCode()
		{
			return m_Fragment.GetHashCode();
		}

		public override string ToString()
		{
			return m_Fragment;
		}
	}
}
