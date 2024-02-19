using System.Collections.ObjectModel;

namespace LdLib.Types;

public class LdLine : NotifyObject
{
    private string _comment = "";

    public string Comment
    {
        get => _comment;
        set => SetField(ref _comment, value);
    }

    public ObservableCollection<LdElement> Elements { get; init; } = new();

    public int GetMaxRows() => Elements.Max(x => x.LinePos.Row) + 1;
    public bool IsOpenSpace(RowCol pos) => Elements.Any(x => x.LinePos == pos);

    public string GetLogicalStatement()
    {
        return new LineStatementCompiler(Elements).ToString();
    }
}