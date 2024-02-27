using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace LdLib.Types;

public class LdDocument : NotifyObject
{
    public ObservableCollection<LdLine> Lines { get; init; } = new();
}