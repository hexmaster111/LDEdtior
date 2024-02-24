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

    public int GetMaxRows()
    {
        if (!Elements.Any()) return 0;
        return Elements.Max(x => x.LinePos.Row) + 1;
    }

    public bool IsOpenSpace(RowCol pos)
    {
        return Elements.Any(x => x.LinePos == pos);
    }

    public string GetLogicalStatement()
    {
        var comp = new LineStatementCompiler(Elements);
        comp.Parse();
        return comp.ToString();
    }
}