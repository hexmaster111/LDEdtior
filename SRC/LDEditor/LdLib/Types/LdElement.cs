using Newtonsoft.Json;

namespace LdLib.Types;

[JsonObject(MemberSerialization.OptIn)]
public class LdElement : NotifyObject
{
    private ElementType _elementElementType;

    private RowCol _linePos;
    private string _label = "";


    [JsonProperty("Pos")]
    public RowCol LinePos
    {
        get => _linePos;
        set => SetField(ref _linePos, value);
    }

    [JsonProperty("Lbl")]
    public string Label
    {
        get => _label;
        set => SetField(ref _label, value);
    }

    [JsonProperty("Typ")]
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