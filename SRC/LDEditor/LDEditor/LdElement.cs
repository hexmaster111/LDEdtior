using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;

namespace LDEditor;

public class LdDocument : NotifyObject
{
    public ObservableCollection<LdLine> Lines { get; init; } = new();
}

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
}

public class LdElement : NotifyObject
{
    private ElementType _elementElementType;

    private RowCol _linePos;
    private string _label = "";


    public RowCol LinePos
    {
        get => _linePos;
        set => SetField(ref _linePos, value);
    }

    public string Label
    {
        get => _label;
        set => SetField(ref _label, value);
    }

    public ElementType ElementType
    {
        get => _elementElementType;
        set => SetFieldAndTell(ref _elementElementType, value,
            nameof(ElementType),
            nameof(ElementString),
            nameof(CenterBarText));
    }

    public string ElementString => ElementType switch
    {
        ElementType.Nothing => "       ",
        ElementType.Wire => "-------",
        ElementType.NormallyOpenContact => "--| |--",
        ElementType.NormallyClosedContact => "--|/|--",
        ElementType.Coil => "--( )--",
        ElementType.NegatedCoil => "--(/)--",
        ElementType.SetLatchCoil => "--(S)--",
        ElementType.ResetLatchCoil => "--(R)--",
        ElementType.OrWire => "---+---",
        ElementType.OrBranchEnd => "---+   ",
        ElementType.OrBranchStart => "   +---",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string CenterBarText => ElementType switch
    {
        ElementType.OrBranchEnd => "|",
        ElementType.OrBranchStart => "|",
        _ => ""
    };
}

public enum ElementType
{
    Wire = 0,
    Nothing,
    OrWire,
    OrBranchEnd,
    OrBranchStart,
    NormallyOpenContact,
    NormallyClosedContact,

    // PositiveTransitionSendingContact,
    // NegativeTransmissionSendingContact,

    Coil,
    NegatedCoil,
    SetLatchCoil,
    ResetLatchCoil,

    // PositiveTransitionSendingCoil,
    // NegativeTransitionSendingCoil,
}

public struct RowCol
{
    public int Row;
    public int Col;


    public override bool Equals(object? obj)
    {
        if (obj is not RowCol rc) return false;
        return rc.Row == Row && rc.Col == Col;
    }

    public override int GetHashCode() => HashCode.Combine(Row.GetHashCode(), Col.GetHashCode());

    public override string ToString() => $"Row {Row} - Col {Col}";

    public static bool operator ==(RowCol left, RowCol right) => left.Equals(right);

    public static bool operator !=(RowCol left, RowCol right) => !(left == right);
}