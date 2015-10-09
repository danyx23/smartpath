using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTS.SmartPath.PathFragments
{
	public sealed class DirectoryFragment : PathFragment
	{
		public static DirectoryFragment UpOneDirectory = new DirectoryFragment("..");

		internal DirectoryFragment(string fragment) : base(fragment)
		{
		}

		public override string ConcatenableFragment { get { return Fragment + '\\'; } }
	}
}
