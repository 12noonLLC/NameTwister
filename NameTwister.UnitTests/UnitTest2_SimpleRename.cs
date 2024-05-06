using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Collections.Generic;

namespace NameTwisterUnitTests;

[TestClass]
public class UnitTest2_SimpleRename
{
	public TestContext TestContext { get; set; }
	public static DirectoryInfo WorkingDirectory { get; private set; }


	[ClassInitialize]
	public static void ClassSetup(TestContext _)
	{
	}

	[ClassCleanup]
	public static void ClassTeardown()
	{
	}


	[TestInitialize]
	public void TestSetup()
	{
		WorkingDirectory = new DirectoryInfo(Path.Combine(TestContext.TestRunDirectory!, "Testing"));
		WorkingDirectory.Create();
		Assert.IsTrue(WorkingDirectory.Exists);
	}


	[TestCleanup]
	public void TestTeardown()
	{
		WorkingDirectory.Delete(recursive: true);
		Assert.IsFalse(WorkingDirectory.Exists);
	}




	// "cat" -> "dog" using Model
	[TestMethod]
	public void TestSimpleTextRenameModel()
	{
		// create "cat" file in WorkingDirectory
		NameTwister.MyFile myfile = new(CreateTestFile("cat"));

		// target
		Assert.IsTrue(myfile.IsSameName);
		myfile.TargetName = "dog";
		Assert.IsFalse(myfile.IsConflict);
		Assert.IsFalse(myfile.IsSameName);

		// rename
		Assert.IsTrue(myfile.Rename());
		Assert.IsTrue(File.Exists(myfile.TargetPath()));
		Assert.IsFalse(myfile.IsConflict);
		Assert.IsTrue(myfile.IsSameName);

		// clean up
		File.Delete(myfile.TargetPath());
		Assert.IsFalse(File.Exists(myfile.TargetPath()));
	}

	// "cat" -> "dog"
	[TestMethod]
	public void TestSimpleText()
	{
		TestRenames(
						"cat",
						"dog",
						bCaseSensitive: false,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("Cat", "dog", null),
								("cate", "doge", null),
								("acate", "adoge", null),
						}
					);
		TestRenames(
						"Cat",
						"dog",
						bCaseSensitive: true,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("Cat", "dog", null),
								("cate", "cate", null),
								("aCate", "adoge", null),
						}
					);
	}

	// "c.t" -> "dog"
	[TestMethod]
	public void TestSimpleRegEx()
	{
		TestRenames(
						"c.t",
						"dog",
						bCaseSensitive: false,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("cat", "dog", null),
								("Cote", "doge", null),
								("acUte", "adoge", null),
								("acte", "acte", null),
						}
					);
		TestRenames(
						"C.t",
						"doG",
						bCaseSensitive: true,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("cat", "cat", null),
								("COte", "doGe", null),
								("aCute", "adoGe", null),
								("acte", "acte", null),
						}
					);
	}

	// "c.t" -> "dog##"
	[TestMethod]
	public void TestSimpleSequence()
	{
		TestRenames(
						"c.t",
						"dog##",
						bCaseSensitive: false,
						bSequence: true,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("acUte", "adog01e", null),
								("cat", "dog02", null),
								("coTe", "dog03e", null),
								("cut", "dog04", null),
						}
					);
		TestRenames(
						"c.t",
						"dog##",
						bCaseSensitive: true,
						bSequence: true,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("acute", "adog01e", null),
								("Cat", "Cat", null),
								("cote", "dog02e", null),
								("cut", "dog03", null),
						}
					);
	}

	[TestMethod]
	public void TestSequenceStart()
	{
		TestRenames(
						"c.t",
						"do##g",
						bCaseSensitive: false,
						bSequence: true,
						sequenceStart: 8,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("acUte", "ado08ge", null),
								("cat", "do09g", null),
								("coTe", "do10ge", null),
								("cut", "do11g", null),
						}
					);
		TestRenames(
						"c.t",
						"dog#",
						bCaseSensitive: false,
						bSequence: true,
						sequenceStart: 7,
						bReplaceAll: false,
						files: new List<(string, string, NameTwister.MyFile?)>
						{
								("acUte", "adog7e", null),
								("cat", "dog8", null),
								("coTe", "dog9e", null),
								("cut", "dog10", null),
								("cOt", "dog11", null),
						}
					);
	}


	// "(blah)" -> "$1"
	[TestMethod]
	public void TestCaptureCaseInsensitive()
	{
		TestRenames(
						strSource: "(c.t)",
						strTarget: "$1dog##",
						bCaseSensitive: false,
						bSequence: true,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string filenameSource, string filenameTarget, NameTwister.MyFile? myFile)>
						{
								(filenameSource: "aCute", filenameTarget: "aCutdog01e", myFile: null),
								(filenameSource: "cat", filenameTarget: "catdog02", myFile: null),
								(filenameSource: "cOte", filenameTarget: "cOtdog03e", myFile: null),
								(filenameSource: "cuT", filenameTarget: "cuTdog04", myFile: null),
						}
					);
	}

	[TestMethod]
	public void TestCaptureCaseSensitive()
	{
		TestRenames(
						strSource: "(c.T)",
						strTarget: "$1dog##",
						bCaseSensitive: true,
						bSequence: true,
						sequenceStart: 1,
						bReplaceAll: false,
						files: new List<(string filenameSource, string filenameTarget, NameTwister.MyFile? myFile)>
						{
								(filenameSource: "acuTe", filenameTarget: "acuTdog01e", myFile: null),
								(filenameSource: "caT", filenameTarget: "caTdog02", myFile: null),
								(filenameSource: "cOte", filenameTarget: "cOte", myFile: null),
								(filenameSource: "cuT", filenameTarget: "cuTdog03", myFile: null),
						}
					);
	}


	// "CatDogFishDog" --> "CatXYZFishXYZ"
	[TestMethod]
	public void TestReplaceAllCaseInsensitive()
	{
		TestRenames(
						strSource: "d.g",
						strTarget: "XYZ",
						bCaseSensitive: false,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: true,
						files: new List<(string filenameSource, string filenameTarget, NameTwister.MyFile? myFile)>
						{
								(filenameSource: "1CatDogFrogDogDogFishDog", filenameTarget: "1CatXYZFrogXYZXYZFishXYZ", myFile: null),
								(filenameSource: "2CatdogFrogDogDogFishDog", filenameTarget: "2CatXYZFrogXYZXYZFishXYZ", myFile: null),
								(filenameSource: "3CatdogFrogDogdogFishDog", filenameTarget: "3CatXYZFrogXYZXYZFishXYZ", myFile: null),
								(filenameSource: "4CatdogFrogdogdogFishdog", filenameTarget: "4CatXYZFrogXYZXYZFishXYZ", myFile: null),
						}
					);
	}

	[TestMethod]
	public void TestReplaceAllCaseSensitive()
	{
		TestRenames(
						strSource: "D.g",
						strTarget: "XYZ",
						bCaseSensitive: true,
						bSequence: false,
						sequenceStart: 1,
						bReplaceAll: true,
						files: new List<(string filenameSource, string filenameTarget, NameTwister.MyFile? myFile)>
						{
								(filenameSource: "1CatDogFrogDogDogFishDog", filenameTarget: "1CatXYZFrogXYZXYZFishXYZ", myFile: null),
								(filenameSource: "2CatdogFrogDogDogFishDog", filenameTarget: "2CatdogFrogXYZXYZFishXYZ", myFile: null),
								(filenameSource: "3CatdogFrogDogdogFishDog", filenameTarget: "3CatdogFrogXYZdogFishXYZ", myFile: null),
								(filenameSource: "4CatdogFrogdogdogFishdog", filenameTarget: "4CatdogFrogdogdogFishdog", myFile: null),
						}
					);
	}


	/// <summary>
	/// This tests a batch of files being renamed.
	/// </summary>
	/// <remarks>We can't use anonymous classes because their members are read-only.</remarks>
	/// <example>new { Source = "cat", Target = "dog", File = (MyFile)null }</example>
	/// <param name="strSource">Source expression</param>
	/// <param name="strTarget">Target expression</param>
	/// <param name="bCaseSensitive">Case-sensitive comparison</param>
	/// <param name="bSequence">Replace ## with a sequence number</param>
	/// <param name="sequenceStart">Starting number for the sequence</param>
	/// <param name="bReplaceAll">Replace all occurrences of source expression</param>

	/// <param name="files">Collection of associated file information: current filename, expected target filename, MyFile, etc...</param>

	private static void TestRenames(string strSource, string strTarget,
												bool bCaseSensitive, bool bSequence, int sequenceStart, bool bReplaceAll,
												List<(string filenameSource, string filenameTarget, NameTwister.MyFile? myFile)> files)
	{
		// set up
		NameTwister.ViewModel vm = new()
		{
			EnteredSourceExpression = strSource,
			EnteredTargetExpression = strTarget,
			IsCaseSensitiveMatching = bCaseSensitive,
			IsSequenced = bSequence,
			SequenceStartText = sequenceStart.ToString(),
			IsReplaceAllMatches = bReplaceAll,
		};

		// create current files in WorkingDirectory
		foreach (var item in files.ToList())
		{
			// Add file to file list
			NameTwister.MyFile myfile = new(CreateTestFile(item.filenameSource));
			vm.Files.Add(myfile);
			Assert.IsTrue(myfile.IsSameName);

			// Tuples are read-only, so we can't do this: item.myFile = myfile;
			files.Remove(item);
			files.Add((item.filenameSource, item.filenameTarget, myfile));
		}

		vm.UpdateFileList();

		// verify constructed target information
		foreach (var item in files)
		{
			Assert.IsNotNull(item.myFile);
			System.Diagnostics.Debug.Assert(item.myFile is not null);
			Assert.AreEqual(item.filenameTarget, item.myFile.TargetName);
			Assert.IsFalse(item.myFile.IsConflict);

			// If the file is not supposed to be renamed, assert the name hasn't changed.
			if (item.filenameSource == item.filenameTarget)
			{
				Assert.IsTrue(item.myFile.IsSameName);
			}
			else // make sure it was renamed to the expected name
			{
				Assert.IsFalse(item.myFile.IsSameName);
			}
		}

		// rename
		vm.RenameFiles();

		// verify target files
		foreach (NameTwister.MyFile item in vm.Files)
		{
			Assert.IsTrue(File.Exists(item.TargetPath()));
			Assert.IsFalse(item.IsConflict);
			Assert.IsTrue(item.IsSameName);

			// clean up
			File.Delete(item.TargetPath());
			Assert.IsFalse(File.Exists(item.TargetPath()));
		}
	}


	[TestMethod]
	public void TestConflict()
	{
		NameTwister.ViewModel vm = new()
		{
			EnteredSourceExpression = "c",
			EnteredTargetExpression = "C",
			IsCaseSensitiveMatching = false,
			IsSequenced = false,
			IsReplaceAllMatches = false,
		};

		// Add file to file list
		NameTwister.MyFile myfile = new(CreateTestFile("cat"));
		vm.Files.Add(myfile);
		Assert.IsTrue(myfile.IsSameName);

		vm.UpdateFileList();

		// Test for conflict
		Assert.IsFalse(vm.Files.First().IsConflict);

		Assert.IsTrue(myfile.Rename());
		Assert.IsFalse(vm.Files.First().IsConflict);
		Assert.IsTrue(File.Exists(vm.Files.First().TargetPath()));
	}


	[TestMethod]
	public void TestConflictWithTargets()
	{
		/*
		 * start with two files
		 *		cat1
		 *		cat2
		 * rename both to "dog" => CONFLICT
		 */
		NameTwister.ViewModel vm = new()
		{
			EnteredSourceExpression = @"cat\d",
			EnteredTargetExpression = @"dog",
			IsCaseSensitiveMatching = false,
			IsSequenced = false,
			IsReplaceAllMatches = false,
		};

		// Add file to file list
		foreach (var name in new[] { "cat1", "cat2", })
		{
			NameTwister.MyFile myfile = new(CreateTestFile(name));
			vm.Files.Add(myfile);
			Assert.IsTrue(myfile.IsSameName);
		}

		vm.UpdateFileList();

		// Test for conflict
		Assert.IsTrue(vm.Files.Any(f => f.IsConflict));
		Assert.IsFalse(vm.Files.Skip(0).First().IsConflict);
		Assert.IsTrue(vm.Files.Skip(1).First().IsConflict);

		vm.RenameFiles();

		// If there was a conflict, the source file exists; else the target does.
		Assert.IsTrue(File.Exists(vm.Files.Skip(0).First().TargetPath()));
		Assert.IsTrue(File.Exists(vm.Files.Skip(1).First().SourcePath()));
	}


	/// <summary>
	/// Test a conflict with an existing file.
	/// </summary>
	[TestMethod]
	public void TestConflictWithExistingFiles()
	{
		string pathCat = CreateTestFile("cat");
		CreateTestFile("dog");

		NameTwister.ViewModel vm = new()
		{
			EnteredSourceExpression = @"cat",
			EnteredTargetExpression = @"dog",
			IsCaseSensitiveMatching = false,
			IsSequenced = false,
			IsReplaceAllMatches = false,
		};

		NameTwister.MyFile myFile = new(pathCat);
		vm.Files.Add(myFile);

		vm.UpdateFileList();

		Assert.IsFalse(myFile.Rename());
	}


	[TestMethod]
	[Ignore]
	public void TestConflictWhileRenaming()
	{
		/*
		 * create files cat1, cat3, cat4
		 * file list:
		 *		cat4
		 *		cat1
		 *		cat3
		 *	rename "cat\d" to "Cat#"
		 *	cat4 will conflict while being renamed.
		 */
		NameTwister.ViewModel vm = new()
		{
			EnteredSourceExpression = @"cat\d",
			EnteredTargetExpression = @"Cat#",
			IsCaseSensitiveMatching = false,
			IsSequenced = true,
			IsReplaceAllMatches = false,
		};
		vm.Files.Add(new(CreateTestFile("cat4")));
		vm.Files.Add(new(CreateTestFile("cat1")));
		vm.Files.Add(new(CreateTestFile("cat3")));

		vm.UpdateFileList();

		Assert.IsTrue(vm.Files.All(f => !f.IsConflict));

		vm.RenameFiles();

		/*
		 * TODO: we want NOT to conflict WHILE renaming files.
		 */
		Assert.IsTrue(vm.Files.All(f => File.Exists(f.TargetPath())));
	}


	/// <summary>
	/// Create a file with the passed name in the working directory.
	/// </summary>
	/// <param name="filename">Name of file to create</param>
	/// <returns>Path to the new file</returns>
	private static string CreateTestFile(string filename)
	{
		string filePath = Path.Combine(WorkingDirectory.FullName, filename);
		using FileStream fstream = File.Create(filePath);
		return filePath;
	}
}
