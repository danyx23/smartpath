using System;
using HTS.SmartPath;
using NUnit.Framework;

namespace SmartPathTests
{
	[TestFixture]
	public class FileExtensionTests
	{

		[Test]
		public void TestFileExtension()
		{
			var extension = new FileExtension("txt");

			Assert.IsTrue(StringComparer.InvariantCultureIgnoreCase.Equals((string) extension.AsStringWithDot, ".txt"));
			Assert.IsTrue(StringComparer.InvariantCultureIgnoreCase.Equals((string) extension.AsStringWithoutDot, "txt"));

			var sameExtension = new FileExtension("TXT");

			Assert.That(extension.Equals(sameExtension));

			Assert.IsFalse(extension.Equals(null));

			var otherExtension = new FileExtension("txtx");

			Assert.IsFalse(extension.Equals(otherExtension));
		}

		[Test, 
		 ExpectedException(typeof(PathInvalidException))]
		public void TestFileExtensionInvalidConstructor()
		{
			var extension = new FileExtension(".txt");
		}

		[Test]
		public void TestEqualsHashCode()
		{
			var txt = new FileExtension("txt");
			var TXT = new FileExtension("TXT");
			var csv = new FileExtension("csv");

			Assert.IsTrue(txt == txt);
			Assert.IsTrue(txt == TXT);
			Assert.IsFalse(txt != TXT);
			Assert.IsFalse(txt == csv);

			Assert.AreEqual(TXT.GetHashCode(), txt.GetHashCode());
			Assert.AreNotEqual(TXT.GetHashCode(), csv.GetHashCode());

			Assert.AreEqual(TXT.ToString(), "TXT");
			Assert.AreEqual(txt.ToString(), "txt");
		}

	}
}