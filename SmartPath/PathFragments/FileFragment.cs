using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTS.SmartPath.PathFragments
{
	public sealed class FileFragment : PathFragment
	{
		internal FileFragment(string fragment) : base(fragment)
		{
		}

		public override string ConcatenableFragment { get { return Fragment; } }
	}
}
