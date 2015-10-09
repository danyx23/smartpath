using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTS.SmartPath.PathFragments
{
	public sealed class RootFragment : PathFragment
	{
		internal RootFragment(string fragment) : base(fragment)
		{
		}

		public override string ConcatenableFragment { get { return Fragment; } }
	}
}
