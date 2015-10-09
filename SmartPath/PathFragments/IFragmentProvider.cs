using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HTS.SmartPath.PathFragments
{
	public interface IFragmentProvider
	{
		IEnumerable<PathFragment> PathFragments { get; } 
	}
}
