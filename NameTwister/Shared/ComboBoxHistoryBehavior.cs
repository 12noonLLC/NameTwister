using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Shared
{
	/// <summary>
	/// This behavior implements saving history of user entries in a Combo Box.
	/// </summary>
	/// <remarks>
	/// CTRL+D deletes the current text from history.
	/// </remarks>
	/// <example>
	/// public ObservableCollection<string> SourceExpressions = new();
	/// 
	/// xmlns:i="http://schemas.microsoft.com/xaml/behaviors"
	/// 
	/// <ComboBox ...>
	///	<i:Interaction.Behaviors>
	///		<shared:ComboBoxHistoryBehavior Entries="{Binding SourceExpressions}" Tag="SourceHistory" ConfirmationButton="{x:Reference Name=Confirm}" />
	///	</i:Interaction.Behaviors>
	/// </ComboBox>
	/// 
	/// <Button Margin="5" Padding="10,5" Width="80" Content="Rename" Click="RenameButton_Click" x:Name="Confirm" />
	/// </example>
	class ComboBoxHistoryBehavior : Microsoft.Xaml.Behaviors.Behavior<ComboBox>
	{
		// List of entries to be updated
		public Collection<string> Entries
		{
			get => (Collection<string>)GetValue(EntriesProperty);
			set => SetValue(EntriesProperty, value);
		}
		public static readonly DependencyProperty EntriesProperty =
			DependencyProperty.Register(nameof(Entries), typeof(Collection<string>), typeof(ComboBoxHistoryBehavior));

		// Button whose Click event should trigger saving a new history entry.
		public Button ConfirmationButton
		{
			get => (Button)GetValue(ConfirmationButtonProperty);
			set => SetValue(ConfirmationButtonProperty, value);
		}
		public static readonly DependencyProperty ConfirmationButtonProperty =
			DependencyProperty.Register(nameof(ConfirmationButton), typeof(Button), typeof(ComboBoxHistoryBehavior));

		// Identifier to use when saving the history list.
		public string Tag
		{
			get => (string)GetValue(TagProperty);
			set => SetValue(TagProperty, value);
		}
		public static readonly DependencyProperty TagProperty =
			DependencyProperty.Register(nameof(Tag), typeof(string), typeof(ComboBoxHistoryBehavior));


		private ComboBox TheControl => (ComboBox)AssociatedObject;

		protected override void OnAttached()
		{
			LoadHistory();

			TheControl.PreviewKeyDown += TheControl_PreviewKeyDown;
			ConfirmationButton.Click += ConfirmationButton_Click;
		}

		protected override void OnDetaching()
		{
			TheControl.PreviewKeyDown -= TheControl_PreviewKeyDown;
			ConfirmationButton.Click -= ConfirmationButton_Click;
		}


		private void LoadHistory()
		{
			// Do not clear unless we read some because there might be defaults.
			var entries = MyStorage.ReadStrings(Tag);
			if (entries.Any())
			{
				Entries.Clear();
				entries.ForEach(s => Entries.Add(s));
			}
		}

		private void SaveHistory()
		{
			MyStorage.WriteStrings(Tag, Entries);
		}

		private void ConfirmationButton_Click(object sender, RoutedEventArgs e)
		{
			string s = TheControl.Text;
			if (string.IsNullOrWhiteSpace(s) || Entries.Contains(s))
			{
				return;
			}

			Entries.Add(s);

			SaveHistory();
		}

		private void TheControl_PreviewKeyDown(object /*ComboBox*/ sender, System.Windows.Input.KeyEventArgs e)
		{
			if ((e.Key == System.Windows.Input.Key.Delete) && (e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.LeftCtrl) || e.KeyboardDevice.IsKeyDown(System.Windows.Input.Key.RightCtrl)))
			{
				//ComboBox cbox = (ComboBox)sender;
				//var savedIndex = cbox.SelectedIndex;

				Entries.Remove(TheControl.Text);

				SaveHistory();

				/// BUG: For some unknown reason, selecting a new item is NOT reflected in the UI.
				///		Even setting IsSynchronizedWithCurrentItem="True" changes nothing.

				//if (cbox.HasItems)
				//{
				//	if (savedIndex == -1)
				//	{
				//		savedIndex = 0;
				//	}

				//	cbox.SelectedIndex = (savedIndex >= cbox.Items.Count) ? cbox.Items.Count - 1 : savedIndex;
				//	cbox.SelectedItem = Entries[cbox.SelectedIndex];
				//	cbox.SelectedValue = Entries[cbox.SelectedIndex];
				//	ICollectionView view = CollectionViewSource.GetDefaultView(cbox.Items);
				//	view.MoveCurrentToPosition((savedIndex >= cbox.Items.Count) ? cbox.Items.Count - 1 : savedIndex);
				//	view.Refresh();
				//}
			}
		}
	}
}
