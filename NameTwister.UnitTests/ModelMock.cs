using System.Collections.ObjectModel;

namespace NameTwister;

public class Model
{
	public ObservableCollection<string> SourceExpressions { get; set; } = new();
	public ObservableCollection<string> TargetExpressions { get; set; } = new();
}
