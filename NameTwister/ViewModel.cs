using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NameTwister;

public class ViewModel
{
	private const char NumberSequenceChar = '#';

	public ObservableCollection<MyFile> Files { get; private set; } = new();

	public ObservableCollection<string> SourceExpressions { get; set; } = new();
	public string SelectedSourceExpression { get; set; } = String.Empty;
	public string EnteredSourceExpression { get; set; } = String.Empty;

	public ObservableCollection<string> TargetExpressions { get; set; } = new();
	public string SelectedTargetExpression { get; set; } = String.Empty;
	public string EnteredTargetExpression { get; set; } = String.Empty;

	public bool IsCaseSensitiveMatching { get; set; } = false;

	public bool IsReplaceAllMatches { get; set; } = false;
	public bool IsSequenced { get; set; } = false;
	public string SequenceStartText { get; set; } = "1";
	public int SequenceStart => Convert.ToInt32(SequenceStartText);


	public ViewModel()
	{
		/// Add default expressions
		SourceExpressions.Add(@"IMG_(\d{4})(\d\d)(\d\d)");
		SourceExpressions.Add(@"\.jpeg$");

		TargetExpressions.Add(@"$1-$2-$3 IMG");
		TargetExpressions.Add(@".jpg");
	}

	internal void AddFiles(List<string> filepaths)
	{
		/*
		 * Add paths of dropped files or directories to collection
		 */
		filepaths.ForEach(path => DropPath(path));

		UpdateFileList();
	}

	private void DropPath(string path)
	{
		// if this is a directory, recurse
		if (Directory.Exists(path))
		{
			foreach (string pathFile in Directory.GetFiles(path))
			{
				DropFile(pathFile);
			}
			foreach (string pathDir in Directory.GetDirectories(path))
			{
				DropPath(pathDir);
			}
		}
		else
		{
			DropFile(path);
		}
	}

	private void DropFile(string path)
	{
		MyFile myfile = new(path);

		// if this path is already in the list, skip it
		if (Files.Contains(myfile, myfile))
		{
			return;
		}

		Files.Add(myfile);
	}


	public void UpdateFileList()
	{
		// if there's no source expression, the Target is unchanged
		if (String.IsNullOrEmpty(EnteredSourceExpression))
		{
			foreach (MyFile f in Files)
			{
				f.TargetName = f.SourceName;
				f.IsConflict = false;
			}
			return;
		}

		Regex regex;


		/*
		 * Apply the source and target expressions to the list of files
		 * 
		 * http://msdn.microsoft.com/en-us/library/hs600312.aspx
		 */
		int iSequence;
		try
		{
			regex = new Regex(EnteredSourceExpression, IsCaseSensitiveMatching ? RegexOptions.None : RegexOptions.IgnoreCase);
			iSequence = SequenceStart;
		}
		catch (ArgumentException)
		{
			return;
		}

		int nRenames = 0;
		int nSame = 0;
		int nConflicts = 0;

		List<string> filepathsTarget = new();
		foreach (MyFile f in Files)
		{
			Match m = regex.Match(f.SourceName);

			// if there's no match, the Target is unchanged
			if (!m.Success)
			{
				f.TargetName = f.SourceName;
				++nSame;
				continue;
			}

			if (EnteredTargetExpression is null)
			{
				EnteredTargetExpression = String.Empty;
			}

			// apply regex to source filename and set the target filename to the result
			if (IsReplaceAllMatches)
			{
				f.TargetName = regex.Replace(f.SourceName, EnteredTargetExpression);
			}
			else
			{
				f.TargetName = regex.Replace(f.SourceName, EnteredTargetExpression, 1 /*# of matches*/);
			}


			iSequence = ApplySequence(iSequence, f);

			if (f.SourceName == f.TargetName)
			{
				++nSame;
				continue;
			}

			/*
			 * Does the resulting file path exist on disk or is it already in the collection?
			 * If so, there's a collision, so we highlight the second file.
			 */
			FileInfo fileTarget = new(Path.Combine(f.FolderPath, f.TargetName));
			if (f.AreSourceTargetFilenamesSame() || filepathsTarget.Contains(fileTarget.FullName))
			{
				f.IsConflict = true;
				++nConflicts;
			}
			else
			{
				f.IsConflict = false;
				++nRenames;
			}

			filepathsTarget.Add(fileTarget.FullName);
		}
	}


	/// <summary>
	/// Change a series of "#" characters to the next number in the sequence.
	/// 
	/// Note:
	///	Doing it AFTER the regex replace affects (positively, I hope)
	///	what the user can do with sequences.
	/// </summary>
	/// <example>
	/// Cat##Dog
	/// Start = "Cat"
	/// End = "Dog"
	/// # hashes = 2
	/// 
	/// ##Dog
	/// Start = ""
	/// End = "Dog"
	/// # hashes = 2
	/// 
	/// Cat##
	/// Start = "Cat"
	/// End = ""
	/// # hashes = 2
	/// </example>
	/// <param name="iSequence">The number in the sequence to use</param>
	/// <param name="f">The file whose name is to be substituted</param>
	/// <returns>The next number in the sequence to use (which might be the same one)</returns>
	private int ApplySequence(int iSequence, MyFile f)
	{
		if (!IsSequenced)
		{
			return iSequence;
		}

		// Get all characters before first hash
		string strStart = new(f.TargetName.TakeWhile(c => c != NumberSequenceChar).ToArray());

		// Get all characters after last hash
		string strEnd = new(f.TargetName.SkipWhile(c => c != NumberSequenceChar).SkipWhile(c => c == NumberSequenceChar).ToArray());

		// Count the number of consecutive hash marks
		int nHashes = f.TargetName.SkipWhile(c => c != NumberSequenceChar).TakeWhile(c => c == NumberSequenceChar).Count();

		f.TargetName = strStart + iSequence.ToString("D" + nHashes) + strEnd;
		return iSequence + 1;
	}


	public void RenameFiles()
	{
		// if there's a conflict or the names aren't different, skip it
		Files.Where(f => !f.IsConflict && !f.IsSameName).ToList().ForEach(f => f.Rename());
	}
}
