using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace NameTwisterUnitTests;

[TestClass]
public class UnitTest1_Basics : Shared.MyNotifyPropertyChanged
{
	[TestMethod]
	public void Parameters()
	{
		NameTwister.ViewModel vm = new();

		Assert.AreEqual(1, vm.SequenceStart);

		// Renaming an empty list shouldn't crash.
		vm.RenameFiles();
	}

	[TestMethod]
	public void TestMyFile()
	{
		// Same case
		NameTwister.MyFile myFile1 = new(@"C:\Test\Path\File.txt");
		NameTwister.MyFile myFile2 = new(@"C:\Test\Path\File.txt");
		Assert.IsFalse(myFile1 == myFile2, "Two objects are distinct.");
		Assert.IsTrue(myFile1.Equals(myFile1, myFile2), "Identical paths should be equal.");

		// Different case
		myFile1 = new(@"C:\Test\Path\FILE.txt");
		myFile2 = new(@"C:\Test\Path\file.txt");
		Assert.IsTrue(myFile1.Equals(myFile1, myFile2), "Identical paths should be equal.");

		myFile1 = new(@"C:\Test\PATH\File.txt");
		myFile2 = new(@"C:\Test\path\File.txt");
		Assert.IsTrue(myFile1.Equals(myFile1, myFile2), "Identical paths should be equal.");

		myFile1 = new(@"C:\Test\Path\File.txt");
		myFile2 = new(@"c:\Test\Path\File.txt");
		Assert.IsTrue(myFile1.Equals(myFile1, myFile2), "Identical paths should be equal.");

		// Different paths
		myFile1 = new(@"C:\Test\Path\File.txt");
		myFile2 = new(@"C:\Test\Path2\File.txt");
		Assert.IsFalse(myFile1.Equals(myFile1, myFile2), "Different paths should not be equal.");

		myFile1 = new(@"C:\Test\Path1\File.txt");
		myFile2 = new(@"C:\Test\Path2\File.txt");
		Assert.IsFalse(myFile1.Equals(myFile1, myFile2), "Different paths should not be equal.");

		myFile1 = new(@"C:\Test\Path\File.txt");
		myFile2 = new(@"D:\Test\Path\File.txt");
		Assert.IsFalse(myFile1.Equals(myFile1, myFile2), "Different paths should not be equal.");
	}


	private string _testProperty = string.Empty;
	public string TestProperty { get => _testProperty; set => _testProperty = value; }

	[TestMethod]
	public void TestMyNotifyPropertyChanged()
	{
		Assert.IsTrue(CheckForPropertyChange(ref _testProperty, "cat", propertyName: nameof(TestProperty)), "Property should change value.");
		Assert.IsFalse(CheckForPropertyChange(ref _testProperty, "cat", propertyName: nameof(TestProperty)), "Property should not change value.");
		Assert.IsTrue(CheckForPropertyChange(ref _testProperty, "dog", propertyName: nameof(TestProperty)), "Property should change value.");
	}


	[TestMethod]
	public void TestMyStorage()
	{
		const string Tag = "test";
		try
		{
			List<string> list = new();
			list.Add("111");
			list.Add("222");
			list.Add("333");
			list.Add("444");

			Shared.MyStorage.WriteStrings(Tag, list);
			var read = Shared.MyStorage.ReadStrings(Tag);
			Assert.AreEqual(list.Count, read.Count);

			list.Remove("222");

			Shared.MyStorage.WriteStrings(Tag, list);
			read = Shared.MyStorage.ReadStrings(Tag);
			Assert.AreEqual(list.Count, read.Count);
		}
		finally
		{
			Shared.MyStorage.Delete(Tag);
		}
	}
}
