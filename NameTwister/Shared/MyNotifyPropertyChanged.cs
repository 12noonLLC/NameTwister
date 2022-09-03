using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;


namespace Shared
{
	/// <summary>
	/// This class facilitates raising the PropertyChanged event.
	/// </summary>
	/// <remarks>
	/// This cannot be used by partial classes, such as MainWindow.
	/// </remarks>
	/// <seealso cref="NotifyProperty"/>
	/// <example>
	/// private string _title;
	/// public string Title
	/// {
	/// 	get => _title;
	/// 	private set => CheckForPropertyChange(ref _title, value);
	/// }
	/// </example>
	/// <see cref="http://geekswithblogs.net/brians/archive/2010/08/02/inotifypropertychanged-with-less-code-using-an-expression.aspx"/>
	/// <see cref="http://10rem.net/blog/2010/12/16/strategies-for-improving-inotifypropertychanged-in-wpf-and-silverlight"/>
	/// <see cref="http://joshsmithonwpf.wordpress.com/2007/08/29/a-base-class-which-implements-inotifypropertychanged/"/>
	/// <see cref="http://northhorizon.net/2011/the-right-way-to-do-inotifypropertychanged/"/>
	/// <see cref="http://bhrnjica.net/2012/03/18/new-feature-in-c-5-0-callermembername/"/>
	public class MyNotifyPropertyChanged : INotifyPropertyChanged
	{
		public MyNotifyPropertyChanged() { }

		/// <summary>
		/// Determine if the new value will change the existing property value.
		/// If it does, raise PropertyChanged event.
		/// </summary>
		/// <example>CheckForPropertyChange(ref _name, value);</example>
		/// <typeparam name="T"></typeparam>
		/// <param name="currentValue"></param>
		/// <param name="value"></param>
		/// <param name="propertyName"></param>
		/// <returns>Return true if the property value has changed.</returns>
		public bool CheckForPropertyChange<T>(ref T currentValue, T value, [CallerMemberName] string propertyName = "")
		{
			if ((value is null) && (currentValue is null))
			{
				return false;
			}

			if ((value is not null) && (currentValue is not null) && value.Equals(currentValue))
			{
				return false;
			}

			currentValue = value;

			RaisePropertyChanged(propertyName);

			return true;
		}

		/// <summary>
		/// Determine if the new value will change the existing property value.
		/// If it does, raise PropertyChanged event.
		/// </summary>
		/// <example>CheckForPropertyChange(ref _name, value, () => Name);</example>
		/// <typeparam name="T"></typeparam>
		/// <param name="currentValue"></param>
		/// <param name="value"></param>
		/// <param name="expr"></param>
		/// <returns>Return true if the property value has changed.</returns>
		public bool CheckForPropertyChange<T>(ref T currentValue, T value, Expression<Func<T>> expr)
		{
			if ((value is null) && (currentValue is null))
			{
				return false;
			}

			if ((value is not null) && (currentValue is not null) && value.Equals(currentValue))
			{
				return false;
			}

			currentValue = value;

			RaisePropertyChanged<T>(expr);

			return true;
		}

		/// <example>
		/// RaisePropertyChanged(() => this.Title);
		/// </example>
		/// <typeparam name="T"></typeparam>
		/// <param name="expr"></param>
		public void RaisePropertyChanged<T>(Expression<Func<T>> expr)
		{
			if (PropertyChanged is null)
			{
				return;
			}

			if (expr.Body is not MemberExpression body)
			{
				return;
			}

			RaisePropertyChanged(body.Member.Name);
		}

		/// <example>
		/// RaisePropertyChanged(nameof(Title));
		/// </example>
		/// <param name="propertyName"></param>
		public void RaisePropertyChanged(string propertyName)
		{
			VerifyPropertyName(propertyName);

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		[Conditional("DEBUG")]
		private void VerifyPropertyName(string propertyName)
		{
			Type type = GetType();

			// Look for the public property with the specified name.
			if (type.GetProperty(propertyName) is null)
			{
				// The property could not be found, so alert developer of the problem.
				Debug.Fail($"{propertyName} is not a property of {type.FullName}");
			}
		}


		#region INotifyPropertyChanged Members

#nullable enable
		public event PropertyChangedEventHandler? PropertyChanged;
#nullable restore

		#endregion INotifyPropertyChanged Members
	}
}
