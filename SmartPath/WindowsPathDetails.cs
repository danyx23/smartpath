using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace HTS.SmartPath
{
	static class WindowsPathDetails
	{
		internal static Regex AbsolutePathRegex = new Regex(@"^(?<root>(?:[a-z]:|\\\\[a-z0-9_.$●-]+\\[a-z0-9_.$●-]+)\\?)
															   (?<folders>[^\\/:*?""<>|\r\n]+\\)*
															   (?<file>[^\\/:*?""<>|\r\n]+)?$"
															, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

		internal static Regex RelativePathRegex = new Regex(@"^(?<root>(?:[a-z]:|\\\\[a-z0-9_.$●-]+\\[a-z0-9_.$●-]+)\\?)?
															   (?<folders>([^\\/:*?""<>|\r\n]+\\)+)?
															   (?<file>[^\\/:*?""<>|\r\n]+)?$"
															, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

		internal static char DirectorySeparator = '\\';

		internal static Regex FileExtensionRegex = new Regex(@"^(!?.*) (?# anything, non-greedy)
																\.     (?# then a dot; after that valid extension characters that are captured as the extension group)
																(?<Extension>[^\\/:*? "" <>|\r\n]+)$", RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

		internal static Regex FilenameRegex = new Regex(@"^(!?.*\\)? (?# anything, non-greedy, ending with a backslash - this captures everything up until the last backslash if it is present or nothing)
														   (?<Filename>[^\\/:*? "" <>|\r\n]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace);
	}
}
