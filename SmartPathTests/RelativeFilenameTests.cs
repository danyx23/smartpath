using HTS.SmartPath;
using NUnit.Framework;

namespace SmartPathTests
{
	[TestFixture]
	class RelativeFilenameTests
	{
		[Test]
		public void TestCreation()
		{
			var filename = RelativeFilename.FromPathString("Filename.txt");
			var filenameWithoutExtension = RelativeFilename.FromPathString("Filename");
			Assert.AreNotEqual(filename, filenameWithoutExtension);

			var filenameFromPath = RelativeFilename.FromPathString("filename.txt");
			Assert.AreEqual(filename, filenameFromPath);

		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation()
		{
			var filename = RelativeFilename.FromPathString("Some<file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation2()
		{
			var filename = RelativeFilename.FromPathString("Some?file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation3()
		{
			var filename = RelativeFilename.FromPathString("Some*file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation()
		{
			var filename = RelativeDirectory.FromPathString("Some<file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation2()
		{
			var filename = RelativeDirectory.FromPathString("Some?file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation3()
		{
			var filename = RelativeDirectory.FromPathString("Some*file.txt", true);
		}

		[Test]
		public void TestLeadingBlank()
		{
			var filename = RelativeFilename.FromPathString(" Somefile.txt", true);
		}

		[Test]
		public void TestInvalidFromPathString()
		{
			Assert.That(RelativeFilename.FromPathString("<test").IsEmpty);
			Assert.That(RelativeFilename.FromPathString("").IsEmpty);
		}

		[Test]
		public void TestParentGeneration()
		{
			var fileInSubdir = RelativeFilename.FromPathString("somedir\\test.txt");
			Assert.AreEqual("test.txt", fileInSubdir.FilenameWithExtension);
			Assert.That(!(fileInSubdir.Parent.IsEmpty));
			Assert.AreEqual("somedir", fileInSubdir.Parent.DirectoryName);
			Assert.AreEqual(fileInSubdir.Extension, new FileExtension("txt"));

			Assert.That(fileInSubdir.Parent.Parent.IsEmpty);

			var parentDirSeparateCreate = RelativeDirectory.FromPathString("somedir");
			Assert.AreEqual(fileInSubdir.Parent, parentDirSeparateCreate);
		}

		[Test]
		public void TestRelativeDirectoryParentForFileNullIsEmpty()
		{
			Assert.That(PathUtilities.GetRelativeDirectoryParentForFile(null).IsEmpty);
		}
	}
}
