# Smart Path Library

The purpose of this .NET library is to provide simple, typesafe wrapper classes for filesystem paths, because keeping all your paths as strings is asking for trouble.

I use it everywhere you would normally use a string to store filenames or directory names. Because
there are different types for absolute filenames, absolute directories, relative filenames and 
relative directories it minimizes mistakes when working with paths.

To use the SmartPath library, install it via [NuGet](https://www.nuget.org/packages/smartpath/) or build it yourself from the [github repository](https://github.com/danyx23/smartpath).

I always like to get a quick look at how to use the library, so here is a typical example:

```csharp
// create a (absolute) filename
AbsoluteFilename filename = AbsoluteFilename.FromAbsolutePath(@"C:\temp\somefile.txt");

// print just the name of the file with and without extension
System.Console.WriteLine(filename.FilenameWithExtension); // somefile.txt
System.Console.WriteLine(filename.FilenameWithoutExtension); // somefile

// print absolute path
System.Console.WriteLine(filename.AbsolutePath); // C:\temp\somefile.txt

// build a path by creating a filename and an absolute directory and combining them to
// get an absolute filename
RelativeFilename relativeFilename = RelativeFilename.FromPathString("default.txt");
AbsoluteDirectory absoluteDirectory = AbsoluteDirectory.FromAbsolutePath("\\Server\share\project1");

AbsoluteFilename defaultFileAbsolutePath = absoluteDirectory.CreateFilename(relativeFilename);

System.Console.WriteLine(filename.AbsolutePath); // \\Server\share\project1\default.txt

// directories always output with a trailing backslash to disambiguate their string representation from
// filenames

System.Console.WriteLine(absoluteDirectory.AbsolutePath); // \\Server\share\project1\


```

## What kinds of paths are allowed? Are there any checks?

Currently the library is built to support windows filenames (i.e. backslash as the separating character). 
All file- and directory names are checked via regex on construction to see if they contain illegal characters. 

Currently there are no checks for the length of filenames (~260 charcters for most apis including
most .net ones). This means that this library will gladly let you build long paths that will then throw an exception when using it to open a file or similar.

The library allows only absolute paths starting with a drive letter and colon (C:\whatever) or
smb shares (\\server\share\whatever). Special long path urls (\\?\) or device urls (\\.\) are not supported.

The current state of the library does not attempt to reject all possibly invalid paths. The actual rules for windows paths are complex and often context sensitive (some characters are allowed on some underlying filesystems but not others). Therefore, this library tries to catch the most common errors (and will throw a PathInvalidException if e.g. a filename contains the question mark character), but constructing a valid AbsoluteFilename is by no means a guarantee that a File.Open on this path will not fail because of the path.

## Components of this Library

The main part of the smart path library is 5 value types: AbsoluteFilename, AbsoluteDirectory, RelativeFilename, RelativeDirectory and FileExtension.

As I hope the names communicate, AbsoluteDirectory represents a directory path where the full path is specified, AbsoluteFilename a filename where the path is fully specified, and RelativeFilename and RelativeDirectory represent relative paths (e.g. just "Somefile.txt" or "Somedir\Somefile.txt").

The main use case is to avoid stupid mistakes when building filenames that occur frequentyl when simply concatenating strings. E.g. when you have a directory path like "C:\Temp" and you string concat it together with "somefile.txt", you would of course get "C:\Tempsomefile.txt". 

Even if you use System.IO.Path.Combine() you can be hit by nasty corner cases like the second part accidentally being an absolute path. 

By splitting the concept of a "path" that is represented as a string into 4 distinct types, the code becomes safer and easier to read. The above mentioned problem of combining an absolute directory and absolute filename would result in a compile time error - the only overload that combines an absolute directory with a filename is AbsoluteDirectory.CreateFilename(RelativeFilename filename). 

## Basic usage

### Creation of paths from a string

AbsoluteFilename and AbsoluteDirctory have a static function .FromAbsolutePath() to create new instances and RelativeDirectory and RelativeFilename have a static function .FromRelativePath() for the same purpose. Both take a string parameter of the path and an optional bool that indicates if invalid paths should result in an exception or return the Empty value (this is the default case).

### Combining paths

A common pattern with this library is to create one AbsoluteDirectory from e.g. the current working directory and derive all other paths that reference files or directories within that using the .Create* methods. These methods create new instances representing the combined path, like this:

```csharp
// path manager class that hands out directories used in the application
public class PathManager
{
    private AbsoluteDirectory m_ApplicationRoot;
    
    private AbsoluteDirectory m_LogfileDirectory;
    // ...
    
    public PathManager(string startupDirectory)
    {
        m_ApplicationRoot = AbsoluteDirectory.FromAbsolutePath(startupDirectory);
        m_LogfileDirectory = m_ApplicationRoot.CreateDirectory("logs"); 
		// m_LogfileDirectory represents the path of the logs directory inside the application root
        
		m_Usagelog = m_LogfileDirectory.CreateFilename("usage.log");
		// m_Usagelog represents the path of the file usage.log inside the logs directory

		// Note that none of these actually create directories or files, they are just like string
		// that contain a path!
    }
}
```

### Interacting with the filesystem

Since AbsoluteFilename and AbsoluteDirectory map to potential actual filesystem objects, there are a couple of methods to create directories in the filesystem with the given name or delete them or enumerate over the contents of a directory. All of these are thin wrappers over the corresponding methods from the System.IO Namespace. The available methods are:

**AbsoluteFilename**

 * Exists()
 * DeleteFilesystemItemIfExits()

**AbsoluteDirectory**
 * Exists()
 * DeleteFilesystemItemIfExits()
 * CreateFileSystemDirectory()
 * GetFileSystemFiles(string filter) // enumerates all files within this actual filesystem directory matching a filter
 * GetFileSystemDirectories(string filter) // enumerates all directories within this actual filesystem directory matching a filter

For all other cases (like opening a stream etc), access the AbsolutePath property of the AbsoluteFilename or AbsoluteDirectory. I recommend doing so at the latest possible point in your code, so that all method signatures etc benefit from the typesafety of AbsoluteFilename etc instead of strings:

```csharp
public void WriteFile(AbsoluteFilename filename)
{
	using (var writer = new TextWriter(filename.AbsolutePath))
	{
		// ...
	}
}
```

## Roadmap

This release represents the stable 1.0 version and will use semantic versioning.

This library was developed for use in a Windows Forms Desktop environment using the .NET Framework 4. It should in theory work on all Windows systems and an all Framworks > 4, but it may need some changes to work with the new .NET Core.

If there is enough interest, I would consider adding posix path support for the 2.0 version including conversion between windows paths, posix paths and maybe urls.

Pull requests for Features or bugfixes are always welcome!

## License

The entire library is released under the MIT License (http://opensource.org/licenses/MIT).

Copyright 2015 by Daniel Bachler (http://danielbachler.de), originally developed for H.T.S. Informationssysteme.