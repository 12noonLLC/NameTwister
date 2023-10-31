using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;

namespace NameTwister;

public class MyFile : Shared.MyNotifyPropertyChanged, IEqualityComparer<MyFile>
{
	private string _sourceName = String.Empty;
	public string SourceName
	{
		get => _sourceName;
		private set
		{
			if (CheckForPropertyChange(ref _sourceName, value))
			{
				RaisePropertyChanged(nameof(IsSameName));
			}
		}
	}

	private string _targetName = String.Empty;
	public string TargetName
	{
		get => _targetName;
		set
		{
			if (CheckForPropertyChange(ref _targetName, value))
			{
				RaisePropertyChanged(nameof(IsSameName));
			}
		}
	}

	public bool IsSameName => (SourceName == TargetName);

	public string FolderPath { get; private set; }

	private bool _isConflict = false;
	public bool IsConflict
	{
		get => _isConflict;
		set => CheckForPropertyChange(ref _isConflict, value);
	}

	public string SourcePath() => Path.Combine(FolderPath, SourceName);
	public string TargetPath() => Path.Combine(FolderPath, TargetName);
	public bool AreSourceTargetFilenamesSame() => (String.Compare(SourcePath(), TargetPath(), ignoreCase: false) == 0);



	public MyFile(string pathSource)
	{
		FileInfo info = new(pathSource);
		SourceName = info.Name;
		TargetName = SourceName;
		FolderPath = info.DirectoryName ?? String.Empty;
	}


	public bool Rename()
	{
		/*
		 * This could be handled by the catch below, but we don't want to display a message box.
		 * If there is a file with the same name (and SAME case), there is a conflict.
		 * It is not enough to check if the file exists because that does not take
		 * into account if the filename differs only by case.
		 */
		if (AreSourceTargetFilenamesSame())
		{
			IsConflict = true;
			return false;
		}

		/*
		 * If we rename it successfully, change the source name to the new (target) name.
		 * Else, highlight item as an error.
		 */
		try
		{
			File.Move(SourcePath(), TargetPath());
			SourceName = TargetName;
			IsConflict = false;
		}
		catch (Exception ex) when ((ex is IOException) || (ex is UnauthorizedAccessException) || (ex is PathTooLongException))
		{
			MessageBox.Show($"Unable to rename \"{SourcePath()}\" to \"" + TargetPath() + "\"." + Environment.NewLine + Environment.NewLine + ex.Message);
			IsConflict = true;
			return false;
		}
		catch (Exception ex)
		{
			MessageBox.Show($"Unable to rename \"{SourcePath()}\" to \"" + TargetPath() + "\"." + Environment.NewLine + Environment.NewLine + ex.Message);
			IsConflict = true;
			return false;
		}

		return true;
	}

	#region IEqualityComparer Methods

	public bool Equals(MyFile? x, MyFile? y)
	{
		if ((x is null) && (y is null))
		{
			return true;
		}

		if ((x is null) || (y is null))
		{
			return false;
		}

		return (x.SourceName.Equals(y.SourceName, StringComparison.CurrentCultureIgnoreCase) && x.FolderPath.Equals(y.FolderPath, StringComparison.CurrentCultureIgnoreCase));
	}

	public int GetHashCode(MyFile obj)
	{
		return SourceName.GetHashCode() ^ TargetName.GetHashCode() ^ FolderPath.GetHashCode();
	}

	#endregion IEqualityComparer Methods
}
