using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using HTS.SmartPath;
using HTS.SmartPath.PathFragments;
using NUnit.Framework;

namespace SmartPathTests
{
	[TestFixture]
	public class AbsolutePathTests
	{
		// Todo: 
		// * AbsoluteDirectory.FromAbsoluteOrRelativePath
		// * AbsoluteDirectory.FromPathFragments
		// * AbsoluteDirectory.IsBelow / IsAbove
		// * All TryCreate*
		// * CombineWithRelativePath should test if .. works

		[Test]
		public void TestBasicDirCreation()
		{
			var rootPath = AbsoluteDirectory.FromAbsolutePath("C:\\");
			Assert.That(!rootPath.IsEmpty);
			Assert.IsTrue(rootPath.IsRoot);
			Assert.AreEqual("C:\\", rootPath.AbsolutePath);
			TestUtilities.AssertCollectionEqual(new PathFragment[] {new RootFragment("C:\\")}, rootPath.PathFragments);

			var shareRoot = AbsoluteDirectory.FromAbsolutePath(@"\\Server\projects");
			Assert.That(!shareRoot.IsEmpty);
			Assert.IsTrue(shareRoot.IsRoot);
			Assert.AreEqual(@"\\Server\projects\", shareRoot.AbsolutePath);
			TestUtilities.AssertCollectionEqual(new PathFragment[] { new RootFragment(@"\\Server\projects\") }, shareRoot.PathFragments);
		}

		[Test]
		public void TestExoticCharacters()
		{
			var path1 = AbsoluteFilename.FromAbsolutePath("C:\\鄭和.txt");
			Assert.That(!path1.IsEmpty);
			Assert.AreEqual("鄭和.txt", path1.FilenameWithExtension);
			Assert.AreEqual(path1.Extension, new FileExtension("txt"));
			TestUtilities.AssertCollectionEqual(new PathFragment[] { new RootFragment("C:\\"), new FileFragment("鄭和.txt") }, path1.PathFragments);
		}

		[Test]
		public void TestBasicDirCreation2()
		{
			var path1 = AbsoluteDirectory.FromAbsolutePath("C:\\somedir");
			Assert.That(!path1.IsEmpty);
			Assert.IsFalse(path1.IsRoot);
			Assert.AreEqual("C:\\somedir\\", path1.AbsolutePath);
			Assert.AreNotEqual("c:\\SOMEDIR\\", path1.AbsolutePath);
			Assert.AreEqual("somedir", path1.DirectoryName);
			TestUtilities.AssertCollectionEqual(new PathFragment[] { new RootFragment("C:\\"), new DirectoryFragment("SOMEDIR"),  }, path1.PathFragments);

			var path2 = AbsoluteDirectory.FromAbsolutePath("C:\\somedir\\");
			Assert.That(!path2.IsEmpty);
			Assert.IsFalse(path2.IsRoot);
			Assert.AreEqual("C:\\somedir\\", path2.AbsolutePath);
			Assert.AreEqual(path1, path2);
			Assert.AreEqual("somedir", path2.DirectoryName);
			TestUtilities.AssertCollectionEqual(path2.PathFragments, path1.PathFragments);

			var root = AbsoluteDirectory.FromAbsolutePath("C:\\");
			var path3 = root.CreateDirectoryPath("somedir");
			Assert.That(!path3.IsEmpty);
			Assert.IsFalse(path3.IsRoot);
			Assert.IsTrue(path1.IsBelow(root));
			Assert.AreEqual("C:\\somedir\\", path3.AbsolutePath);
			Assert.AreEqual(path1, path3);
			Assert.AreEqual("somedir", path3.DirectoryName);
			TestUtilities.AssertCollectionEqual(path3.PathFragments, path1.PathFragments);

			Assert.IsFalse(path1.IsBelow(AbsoluteDirectory.Empty));
			Assert.IsFalse(path1.IsBelow(AbsoluteDirectory.FromAbsolutePath("D:\\")));

			Assert.AreNotEqual(root, path3);
			Assert.AreEqual(root, path3.AbsoluteParent);

			var path4 = AbsoluteDirectory.FromAbsolutePath("c:\\SOMEDIR\\");
			Assert.That(!path4.IsEmpty);
			Assert.IsFalse(path4.IsRoot);
			Assert.AreEqual(path1, path4);
			TestUtilities.AssertCollectionEqual(path4.PathFragments, path1.PathFragments);
		}

		[Test]
		public void TestFilenameCreation()
		{
			var extension = new FileExtension("txt");

			var path1 = AbsoluteFilename.FromAbsolutePath("C:\\somedir\\somefile.txt");
			Assert.That(!path1.IsEmpty);
			Assert.AreEqual("somefile.txt", path1.FilenameWithExtension);

			var parentDir = AbsoluteDirectory.FromAbsolutePath("C:\\somedir");
			Assert.AreEqual(path1.AbsoluteParent, parentDir);
			Assert.IsTrue(path1.IsBelow(parentDir));

			var path2 = AbsoluteFilename.FromAbsolutePath("C:\\SOMEDIR\\SOMEFILE.TXT");
			Assert.That(!path1.IsEmpty);
			Assert.AreEqual("SOMEFILE.TXT", path2.FilenameWithExtension);
			Assert.AreEqual(path1, path2);
			Assert.AreEqual(path2.AbsoluteParent, parentDir);

			var path3 = parentDir.CreateFilename("somefile.txt");
			Assert.That(!path3.IsEmpty);
			Assert.AreEqual("somefile.txt", path3.FilenameWithExtension);
			Assert.AreEqual(path1, path3);
			Assert.AreEqual(path3.AbsoluteParent, parentDir);

			var path4 = path3.WithChangedExtension(new FileExtension("csv"));
			Assert.That(!path3.IsEmpty);
			Assert.AreNotEqual("somefile.txt", path4.FilenameWithExtension);
			Assert.AreEqual("somefile.csv", path4.FilenameWithExtension);

		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation()
		{
			var filename = AbsoluteFilename.FromAbsolutePath("C:\\Some<file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation2()
		{
			var filename = AbsoluteFilename.FromAbsolutePath("C:\\Some?file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersCreation3()
		{
			var filename = AbsoluteFilename.FromAbsolutePath("C:\\Some*file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation()
		{
			var filename = AbsoluteDirectory.FromAbsolutePath("C:\\Some<dir", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation2()
		{
			var filename = AbsoluteDirectory.FromAbsolutePath("Some?file.txt", true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void TestInvalidCharactersDirectoryCreation3()
		{
			var filename = AbsoluteDirectory.FromAbsolutePath("Some*file.txt", true);
		}

		[Test]
		public void TestSpacesValid()
		{
			var filename = AbsoluteFilename.FromAbsolutePath("C:\\Some file.txt", true);
			Assert.IsFalse(filename.IsEmpty);
			Assert.AreEqual("Some file.txt", filename.FilenameWithExtension);
			Assert.AreEqual("Some file", filename.FileNameWithoutExtension);
		}

		[Test]
		public void TestParentGeneration()
		{
			var filename = AbsoluteFilename.FromAbsolutePath("C:\\Dir\\File.txt");
			Assert.That(!filename.IsEmpty);

			var createdParent = filename.AbsoluteParent;
			Assert.That(!createdParent.IsEmpty);
			var parentDirSeparateCreation = AbsoluteDirectory.FromAbsolutePath("C:\\Dir");
			Assert.That(!parentDirSeparateCreation.IsEmpty);

			Assert.AreEqual(createdParent, parentDirSeparateCreation);

			var rootDirSeparateCreation = AbsoluteDirectory.FromAbsolutePath("C:\\");
			Assert.That(!rootDirSeparateCreation.IsEmpty);
			Assert.AreEqual(createdParent.AbsoluteParent, rootDirSeparateCreation);

			Assert.That(rootDirSeparateCreation.AbsoluteParent.IsEmpty);
		}

		[Test]
		public void TestCreationWithRelativePaths()
		{
			var baseDir = AbsoluteDirectory.FromAbsolutePath("C:\\dir");
			var filename = RelativeFilename.FromPathString("file.txt");
			var filenameWithSubDir = RelativeFilename.FromPathString("somedir\\file.txt");

			var absFilename = baseDir.CreateFilename(filename);
			Assert.AreEqual("C:\\dir\\file.txt", absFilename.AbsolutePath);

			var absFilenameWithSubDir = baseDir.CreateFilename(filenameWithSubDir);
			Assert.AreEqual("C:\\dir\\somedir\\file.txt", absFilenameWithSubDir.AbsolutePath);

			TestUtilities.AssertCollectionEqual(new PathFragment[]
			{
				new RootFragment("C:\\"),
				new DirectoryFragment("dir"),
				new DirectoryFragment("somedir"),
				new FileFragment("file.txt")
			}, absFilenameWithSubDir.PathFragments);

			Assert.AreNotEqual(absFilename, absFilenameWithSubDir);
		}

		[Test]
		public void TestRelativePathCreation()
		{
			var path = AbsoluteFilename.FromAbsolutePath("C:\\dir1\\dir2\\somefile.txt");

			var parentDir = AbsoluteDirectory.FromAbsolutePath("C:\\dir1\\");

			RelativeFilename relativePath;
			bool shareRoot = path.TryGetRelativePath(parentDir, out relativePath);
			Assert.IsTrue(shareRoot);
			var expectedRelativePath = RelativeDirectory.FromPathString("dir2").CreateFilename("somefile.txt");
			Assert.AreEqual(expectedRelativePath, relativePath);

			var differentBaseDir = AbsoluteDirectory.FromAbsolutePath("C:\\dirA\\dirB");

			RelativeFilename relativePathToDifferentBaseDir;
			bool differentBaseDirShareRoot = path.TryGetRelativePath(differentBaseDir, out relativePathToDifferentBaseDir);
			Assert.IsTrue(differentBaseDirShareRoot);
			var expectedRelativePathToDifferentBaseDir = RelativeFilename.FromPathString("..\\..\\dir1\\dir2\\somefile.txt");
			Assert.AreEqual(expectedRelativePathToDifferentBaseDir, relativePathToDifferentBaseDir);

			var differentRootDir = AbsoluteDirectory.FromAbsolutePath("D:\\dirA");
			RelativeFilename relativePathToDifferentRoot;
			var differentRootDirSharesRoot = path.TryGetRelativePath(differentRootDir, out relativePathToDifferentRoot);
			Assert.IsFalse(differentRootDirSharesRoot);
			Assert.That(relativePathToDifferentRoot.IsEmpty);

			var sameDir = AbsoluteDirectory.FromAbsolutePath("C:\\dir1\\dir2");
			var expectedPathFromSameDir = RelativeFilename.FromPathString("somefile.txt");

			RelativeFilename relativePathFromSameDir;
			bool sameDirShareRoot = path.TryGetRelativePath(sameDir, out relativePathFromSameDir);
			Assert.AreEqual(expectedPathFromSameDir, relativePathFromSameDir);

		}

		[Test]
		public void TestRelativePathCreationFromRoot()
		{
			var path = AbsoluteFilename.FromAbsolutePath("C:\\dir1\\dir2\\somefile.txt");

			var parentDir = AbsoluteDirectory.FromAbsolutePath("C:\\");

			RelativeFilename relativePath;
			bool parentDirIsOnSameRoot = path.TryGetRelativePath(parentDir, out relativePath);
			Assert.IsTrue(parentDirIsOnSameRoot);
			var expectedRelativePath = RelativeDirectory.FromPathString("dir1").CreateDirectoryPath("dir2").CreateFilename("somefile.txt");
			Assert.AreEqual(expectedRelativePath, relativePath);

			var differentBaseDir = AbsoluteDirectory.FromAbsolutePath("C:\\dirA\\dirB");

			RelativeFilename relativePathToDifferentBaseDir;
			bool differentBaseDirIsOnSameRoot = path.TryGetRelativePath(differentBaseDir, out relativePathToDifferentBaseDir);
			Assert.IsTrue(differentBaseDirIsOnSameRoot);
			var expectedRelativePathToDifferentBaseDir = RelativeFilename.FromPathString("..\\..\\dir1\\dir2\\somefile.txt");
			Assert.AreEqual(expectedRelativePathToDifferentBaseDir, relativePathToDifferentBaseDir);


			RelativeDirectory parentDirToPath;
			bool parentDirToPathHasSameRoot = parentDir.TryGetRelativePath(path.AbsoluteParent, out parentDirToPath);
			var expectedParentDirToPath = RelativeDirectory.FromPathString("..\\..");
			Assert.AreEqual(expectedParentDirToPath, parentDirToPath);

			var differentRootDir = AbsoluteDirectory.FromAbsolutePath("D:\\dirA");
			RelativeFilename relativePathToDifferentRoot;
			bool differentRootDirIsUnderSameRoot = path.TryGetRelativePath(differentRootDir, out relativePathToDifferentRoot);
			Assert.IsFalse(differentRootDirIsUnderSameRoot);
			Assert.That(relativePathToDifferentRoot.IsEmpty);

			var sameDir = AbsoluteDirectory.FromAbsolutePath("C:\\dir1\\dir2");
			var expectedPathFromSameDir = RelativeFilename.FromPathString("somefile.txt");
			RelativeFilename relativePathToSameDirFilename;
			bool sameDirSharesRoot = path.TryGetRelativePath(sameDir, out relativePathToSameDirFilename);
			Assert.IsTrue(sameDirSharesRoot);
			Assert.AreEqual(expectedPathFromSameDir, relativePathToSameDirFilename);

		}

		[Test]
		public void TestRelativeParentGeneration()
		{
			var origin= RelativeFilename.FromPathString("..\\..\\dir1\\dir2\\somefile.txt");
			var parent = RelativeDirectory.FromPathString("..\\..\\dir1\\dir2\\");
			var parentAlternative = RelativeDirectory.FromPathString("..\\..\\dir1\\dir2");
			var parent2 = RelativeDirectory.FromPathString("..\\..\\dir1\\");
			var parent3 = RelativeDirectory.FromPathString("..\\..\\");
			var parent4 = RelativeDirectory.FromPathString("..\\");

			Assert.AreEqual(origin, origin);
			TestUtilities.AssertCollectionEqual(origin.PathFragments.Take(origin.PathFragments.Count() - 1), parent.PathFragments);
			TestUtilities.AssertCollectionEqual(origin.PathFragments.Take(origin.PathFragments.Count() - 1), parentAlternative.PathFragments);
			TestUtilities.AssertCollectionEqual(origin.PathFragments.Take(origin.PathFragments.Count() - 2), parent2.PathFragments);
			TestUtilities.AssertCollectionEqual(origin.PathFragments.Take(origin.PathFragments.Count() - 3), parent3.PathFragments);
			TestUtilities.AssertCollectionEqual(origin.PathFragments.Take(origin.PathFragments.Count() - 4), parent4.PathFragments);

			var ancestors = origin.PathFragments.Take(origin.PathFragments.Count() - 1) // we only compare "parents" here, so take up until the last
								  .Scan((list, fragment) => // scan is similar to aggregate but yields every intermediate result
										{
											list.Add(fragment);
											return list;
										},
										new List<PathFragment>())
								  .Skip(1) // skip the empty list that is yielded in the beginning
								  .Select(fragmentList => RelativeDirectory.FromPathFragments(fragmentList));
            var expectedAncestors = new List<RelativeDirectory>() {parent4, parent3, parent2, parent};

			TestUtilities.AssertCollectionEqual(expectedAncestors, ancestors);
		}

		[Test]
		public void TestPathIsDescendingOnly()
		{
			var descending = RelativeFilename.FromPathString("dir1\\dir2\\dir3\\somefile.txt");
			var notDescending = RelativeFilename.FromPathString("..\\..\\dir1\\dir2\\somefile.txt");
			var alsoNotDescending = RelativeFilename.FromPathString("dir1\\..\\..\\somefile.txt");

			Assert.IsTrue(PathUtilities.IsEntirePathDescendingOnly(RelativeDirectory.Empty));
			Assert.IsTrue(PathUtilities.IsEntirePathDescendingOnly(TestUtilities.GetParent(descending)));
			Assert.IsFalse(PathUtilities.IsEntirePathDescendingOnly(TestUtilities.GetParent(notDescending)));
			Assert.IsFalse(PathUtilities.IsEntirePathDescendingOnly(TestUtilities.GetParent(alsoNotDescending)));
		}

		[Test]
		public void TestDirectoryCreationDeletion()
		{
			// This test tries to write into the directory where the test dll is stored. May not work on all configurations.

			var workingDirectory = AbsoluteFilename.FromAbsolutePath(Assembly.GetExecutingAssembly().Location).AbsoluteParent;
			var testDirectory = workingDirectory.CreateDirectoryPath("UnitTestWorkingDirectory");

			Assert.AreEqual(System.IO.Directory.Exists(testDirectory.AbsolutePath), testDirectory.Exists());

			if (System.IO.Directory.Exists(testDirectory.AbsolutePath))
			{
				testDirectory.DeleteFilesystemItemIfExists();
				Assert.IsFalse(testDirectory.Exists());
			}

			testDirectory.CreateFileSystemDirectory();

			Assert.IsTrue(testDirectory.Exists());

			var testSubDirectory = testDirectory.CreateDirectoryPath("test1");
			if (testSubDirectory.Exists())
			{
				testSubDirectory.DeleteFilesystemItemIfExists();
				Assert.IsFalse(testSubDirectory.Exists());
			}
			testSubDirectory.CreateFileSystemDirectory();
			Assert.IsTrue(testSubDirectory.Exists());

			testSubDirectory.DeleteFilesystemItemIfExists();
			Assert.IsFalse(testSubDirectory.Exists());
		}

		[Test]
		public void TestDirectoryFileQueryingAndDeletion()
		{
			// This test tries to write into the directory where the test dll is stored. May not work on all configurations.

			var workingDirectory = AbsoluteFilename.FromAbsolutePath(Assembly.GetExecutingAssembly().Location).AbsoluteParent;
			var testDirectory = workingDirectory.CreateDirectoryPath("UnitTestWorkingDirectory");

			Assert.AreEqual(System.IO.Directory.Exists(testDirectory.AbsolutePath), testDirectory.Exists());

			if (System.IO.Directory.Exists(testDirectory.AbsolutePath))
			{
				testDirectory.DeleteFilesystemItemIfExists();
				Assert.IsFalse(testDirectory.Exists());
			}

			testDirectory.CreateFileSystemDirectory();

			Assert.IsTrue(testDirectory.Exists());

			Assert.IsFalse(testDirectory.GetFileSystemDirectories().Any());
			Assert.IsFalse(testDirectory.GetFileSystemFiles().Any());

			var testSubDirectory = testDirectory.CreateDirectoryPath("test1");
			if (testSubDirectory.Exists())
			{
				testSubDirectory.DeleteFilesystemItemIfExists();
				Assert.IsFalse(testSubDirectory.Exists());
			}
			testSubDirectory.CreateFileSystemDirectory();
			Assert.IsTrue(testSubDirectory.Exists());

			Assert.AreEqual(1, testDirectory.GetFileSystemDirectories().Count());
			Assert.IsFalse(testDirectory.GetFileSystemFiles().Any());

			var testfile = testSubDirectory.CreateFilename("test file.tmp");

			if (testfile.Exists())
				testfile.DeleteFilesystemItemIfExists();

			Assert.IsFalse(testfile.Exists());

			using (var filestream = new System.IO.FileStream(testfile.AbsolutePath, FileMode.CreateNew))
			{
			}

			Assert.IsTrue(testfile.Exists());
			Assert.AreEqual(1, testSubDirectory.GetFileSystemFiles().Count());
			Assert.AreEqual(1, testSubDirectory.GetFileSystemFiles("*.tmp").Count());

			testfile.DeleteFilesystemItemIfExists();
			Assert.IsFalse(testfile.Exists());

			testSubDirectory.DeleteFilesystemItemIfExists();
			Assert.IsFalse(testSubDirectory.Exists());
		}
		
		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void NoFilenameInServerShareInvalid()
		{
			var filename = AbsoluteFilename.FromAbsolutePath(@"\\Server\share\", true);
		}

		[Test]
		public void AsRelativeFilenameWorks()
		{
			var absFilename = AbsoluteFilename.FromAbsolutePath("C:\\somedir\\file.txt");
			var relativeFilename = RelativeFilename.FromPathString("file.txt");
			Assert.AreEqual(relativeFilename, absFilename.AsRelativeFilename());
		}

		[Test]
		public void GetRelativePath()
		{
			var absFilename = AbsoluteFilename.FromAbsolutePath("C:\\dirA\\dirB\\file.txt");
			var differentRoot = AbsoluteDirectory.FromAbsolutePath("D:\\");
			Assert.IsNull(absFilename.GetRelativePathOrNull(differentRoot));

			var dirA = AbsoluteDirectory.FromAbsolutePath("C:\\dirA");
			var expectedPathFromDirA = RelativeFilename.FromPathString("dirB\\file.txt");
			Assert.AreEqual(expectedPathFromDirA, absFilename.GetRelativePathOrNull(dirA));
			TestUtilities.AssertCollectionEqual(expectedPathFromDirA.PathFragments, absFilename.GetRelativePathOrNull(dirA).Value.PathFragments);
		}

		[Test]
		public void FromPathFragments()
		{
			var absFilename = AbsoluteFilename.FromAbsolutePath("C:\\dirA\\dirB\\file.txt");
			var fragments = new PathFragment[] {new RootFragment("C:\\"), new DirectoryFragment("dirA"), new DirectoryFragment("dirB"), new FileFragment("file.txt")};
			var fromFragments = AbsoluteFilename.FromPathFragments(fragments);
			Assert.AreEqual(absFilename, fromFragments);
			TestUtilities.AssertCollectionEqual(absFilename.PathFragments, fromFragments.PathFragments);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void FromPathFragmentsInvalidIfRootMissing()
		{
			var fragments = new PathFragment[] { new DirectoryFragment("dirA"), new DirectoryFragment("dirB"), new FileFragment("file.txt") };
			var fromFragments = AbsoluteFilename.FromPathFragments(fragments, true);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void FromPathFragmentsInvalidIfFileMissing()
		{
			var fragments = new PathFragment[] { new RootFragment("C:\\"), new DirectoryFragment("dirA"), new DirectoryFragment("dirB") };
			var fromFragments = AbsoluteFilename.FromPathFragments(fragments, true);
		}

		[Test]
		public void FromPathFragmentsDirectory()
		{
			var absFilename = AbsoluteDirectory.FromAbsolutePath("C:\\dirA\\dirB");
			var fragments = new PathFragment[] { new RootFragment("C:\\"), new DirectoryFragment("dirA"), new DirectoryFragment("dirB") };
			var fromFragments = AbsoluteDirectory.FromPathFragments(fragments, true);
			Assert.AreEqual(absFilename, fromFragments);
			TestUtilities.AssertCollectionEqual(absFilename.PathFragments, fromFragments.PathFragments);
		}

		[Test,
		ExpectedException(typeof(PathInvalidException))]
		public void FromPathFragmentsInvalidIfRootMissingDirectory()
		{
			var fragments = new PathFragment[] { new DirectoryFragment("dirA"), new DirectoryFragment("dirB")};
			var fromFragments = AbsoluteDirectory.FromPathFragments(fragments, true);
		}

		[Test]
		public void TestAbsoluteFilenameComparisionOperators()
		{
			var filename1 = AbsoluteFilename.FromAbsolutePath(@"C:\dir\test.txt");
			var filename2 = AbsoluteFilename.FromAbsolutePath(@"C:\Dir\Test.txt");
			AbsoluteFilename? filename1Nullable = AbsoluteFilename.FromAbsolutePath(@"C:\dir\test.txt");

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
		public void TestAbsoluteDirectoryComparisionOperators()
		{
			var dir1 = AbsoluteDirectory.FromAbsolutePath(@"C:\dir");
			var dir2 = AbsoluteDirectory.FromAbsolutePath(@"C:\Dir");
			AbsoluteDirectory? dir1Nullable = AbsoluteDirectory.FromAbsolutePath(@"C:\dir");

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

		[Test]
		public void TestRelativePathOrNull()
		{
			var path = AbsoluteDirectory.FromAbsolutePath(@"C:\dirA\dirB\dirC");
			var basePath = AbsoluteDirectory.FromAbsolutePath(@"C:\dirA\");
			var differentRoot = AbsoluteDirectory.FromAbsolutePath(@"D:\dirA");

			var relativePath = path.GetRelativePathOrNull(basePath);
			Assert.AreEqual(RelativeDirectory.FromPathString(@"dirB\dirC"), relativePath);

			var relativePathInverted = basePath.GetRelativePathOrNull(path);
			Assert.AreEqual(RelativeDirectory.FromPathString(@"..\.."), relativePathInverted);

			var unrelatedPath = path.GetRelativePathOrNull(differentRoot);
			Assert.AreEqual(unrelatedPath, null);

			var identicalRelativePath = path.GetRelativePathOrNull(path);
			Assert.AreNotEqual(identicalRelativePath, null);
			Assert.IsTrue(identicalRelativePath.Value.IsEmpty);
		}

		[Test]
		public void TestIsAboveBelow()
		{
			var path = AbsoluteDirectory.FromAbsolutePath(@"C:\dirA\dirB\dirC");
			var basePath = AbsoluteDirectory.FromAbsolutePath(@"C:\dirA\");
			var differentRoot = AbsoluteDirectory.FromAbsolutePath(@"D:\dirA");

			Assert.IsTrue(path.IsBelow(basePath));
			Assert.IsFalse(path.IsAbove(basePath));
			Assert.IsFalse(basePath.IsBelow(path));
			Assert.IsTrue(basePath.IsAbove(path));
			
			Assert.IsFalse(differentRoot.IsBelow(path));
			Assert.IsFalse(differentRoot.IsAbove(path));
			Assert.IsFalse(path.IsBelow(differentRoot));
			Assert.IsFalse(path.IsAbove(differentRoot));

		}
	}
}
