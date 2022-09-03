using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace NameTwister;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public ViewModel TheViewModel { get; private set; } = new ViewModel();

	public MainWindow()
	{
		DataContext = this;

		InitializeComponent();

		SourceComboBox.Loaded += ComboBox_Loaded;
		TargetComboBox.Loaded += ComboBox_Loaded;
	}

	private void ComboBox_Loaded(object sender, RoutedEventArgs e)
	{
		ComboBox cb = (ComboBox)sender;
		TextBox tb = (TextBox)cb.Template.FindName("PART_EditableTextBox", cb);
		tb.TextChanged += TextBox_TextChanged;
	}

	private void RenameButton_Click(object sender, RoutedEventArgs e)
	{
		TheViewModel.RenameFiles();
		TheViewModel.UpdateFileList();
	}

	private void CloseButton_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}


	private void ListView_DragOver(object sender, DragEventArgs e)
	{
		if (e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			e.Effects = DragDropEffects.Copy;
		}
		else
		{
			e.Effects = DragDropEffects.None;
		}
	}

	private void ListView_Drop(object sender, DragEventArgs e)
	{
		if (!e.Data.GetDataPresent(DataFormats.FileDrop))
		{
			return;
		}

		object obj = e.Data.GetData(DataFormats.FileDrop);
		if (obj is string[] filepaths)
		{
			TheViewModel.AddFiles(filepaths.ToList());
		}
	}

	// SelectionChanged is only for selecting something from the list.
	private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		TheViewModel.UpdateFileList();
	}

	private void CheckBox_Changed(object sender, RoutedEventArgs e)
	{
		TheViewModel.UpdateFileList();
	}


	private void DeleteCommand_CanExecute(/*MainWindow*/ object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
	{
		ICollectionView view = CollectionViewSource.GetDefaultView(TheViewModel.Files);
		e.CanExecute = (view.CurrentItem is not null);
	}

	private void DeleteCommand_Executed(/*MainWindow*/ object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
	{
		foreach (var item in FileList.SelectedItems.Cast<MyFile>().ToList())
		{
			TheViewModel.Files.Remove(item);
		}
	}
}
