using HTS.SmartPath;
using HTS.SmartPath.PathFragments;
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
			Assert.AreEqual("Filename", filename.FilenameWithoutExtension);

			TestUtilities.AssertCollectionEqual(new PathFragment[] {new FileFragment("Filename.Txt")}, filename.PathFragments);

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
			TestUtilities.AssertCollectionEqual(new PathFragment[] {new DirectoryFragment("somedir"), new FileFragment("test.txt")}, fileInSubdir.PathFragments);
			Assert.AreEqual(fileInSubdir.Extension, new FileExtension("txt"));

			var parentDirSeparateCreate = RelativeDirectory.FromPathString("somedir");
			Assert.AreEqual("somedir", parentDirSeparateCreate.DirectoryName);
			TestUtilities.AssertCollectionEqual(new PathFragment[] { new DirectoryFragment("somedir") }, parentDirSeparateCreate.PathFragments);
		}


		[Test]
		public void TestGenerationFromFragmenst()
		{
			var fragments = new PathFragment[] {new DirectoryFragment("somedir"), new FileFragment("test.txt")};
			var fileInSubdir = RelativeFilename.FromPathFragments(fragments);

			Assert.AreEqual("test.txt", fileInSubdir.FilenameWithExtension);
			TestUtilities.AssertCollectionEqual(new PathFragment[] { new DirectoryFragment("somedir"), new FileFragment("test.txt") }, fileInSubdir.PathFragments);
			Assert.AreEqual(fileInSubdir.Extension, new FileExtension("txt"));
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestCreateWithRootFragmentInvalid()
		{
			var fragments = new PathFragment[] { new RootFragment("C:\\"), new FileFragment("test.txt") };
			var fileInSubdir = RelativeFilename.FromPathFragments(fragments, true);
		}

		[Test,
		 ExpectedException(typeof (PathInvalidException))]
		public void TestCreateWithoutFileFragmentInvalid()
		{
			var fragments = new PathFragment[] {new DirectoryFragment("somdedir"), new DirectoryFragment("test.txt"),};
			var fileInSubdir = RelativeFilename.FromPathFragments(fragments, true);

		}

		[Test]
		public void TestFileextensionOfEmpty()
		{
			var filename = RelativeFilename.FromPathString("Filename");
			Assert.AreEqual(FileExtension.Empty, filename.Extension);

			Assert.AreEqual(FileExtension.Empty, RelativeFilename.Empty.Extension);
		}

		[Test,
		 ExpectedException(typeof(PathInvalidException))]
		public void TestCreateWithEmptyInvalid()
		{
			var dir = RelativeDirectory.FromPathString("somedir");
			dir.CreateFilename("");
		}

		[Test,
		 ExpectedException(typeof(PathInvalidException))]
		public void TestCreateFromEmptyStringInvalidIfThrowExceptionsTrue()
		{
			var dir = RelativeFilename.FromPathString("", true);
		}

		[Test]
		public void TestCreateFromEmptyStringValidIfThrowExceptionsFalse()
		{
			var dir = RelativeFilename.FromPathString("");
		}

		[Test,
		 ExpectedException(typeof(PathInvalidException))]
		public void TestCreateWithRootInvalid()
		{
			var dir = RelativeFilename.FromPathString(@"C:\test.txt", true);
		}

		[Test,
		 ExpectedException(typeof(PathInvalidException))]
		public void TestCreateWithFromDirectoryInvalid()
		{
			var dir = RelativeFilename.FromPathString("test\\", true);
		}

		[Test]
		public void TestRelativeFilenameComparisionOperators()
		{
			var filename1 = RelativeFilename.FromPathString(@"dir\test.txt");
			var filename2 = RelativeFilename.FromPathString(@"Dir\Test.txt");
			RelativeFilename? filename1Nullable = RelativeFilename.FromPathString(@"dir\test.txt");

			Assert.AreEqual(filename1, filename2);
			Assert.AreEqual(filename1, filename1Nullable);
			Assert.AreEqual(filename1, filename1Nullable.Value);
			Assert.IsTrue(filename1 == filename2);
			Assert.IsFalse(filename1 != filename2);

			// comparision with nullable relativefilename yields false (is overloaded so that we can
			// warn when the users compares with null)
			Assert.IsFalse(filename1 == filename1Nullable);
			Assert.IsFalse(filename1Nullable == filename1);
			Assert.IsFalse(filename1 == null);

		}

		[Test]
		public void TestRelativeDirectoryComparisionOperators()
		{
			var dir1 = RelativeDirectory.FromPathString(@"dir");
			var dir2 = RelativeDirectory.FromPathString(@"Dir");
			RelativeDirectory? dir1Nullable = RelativeDirectory.FromPathString(@"dir");

			Assert.AreEqual(dir1, dir2);
			Assert.AreEqual(dir1, dir1Nullable);
			Assert.AreEqual(dir1, dir1Nullable.Value);
			Assert.IsTrue(dir1 == dir2);
			Assert.IsFalse(dir1 != dir2);

			// comparision with nullable relativefilename yields false (is overloaded so that we can
			// warn when the users compares with null)
			Assert.IsFalse(dir1 == dir1Nullable);
			Assert.IsFalse(dir1Nullable == dir1);
			Assert.IsFalse(dir1 == null);

		}
	}
}
