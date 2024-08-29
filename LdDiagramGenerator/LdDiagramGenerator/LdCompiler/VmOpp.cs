public struct VmOpp
{
    public VmOppKind Kind;
    public int Arg;

    public static VmOpp Read(int pin) => new() { Arg = pin, Kind = VmOppKind.Read };
    public static VmOpp Or(int popCount) => new() { Arg = popCount, Kind = VmOppKind.Or };
    public static VmOpp And(int popCount) => new() { Arg = popCount, Kind = VmOppKind.And };
}

public enum VmOppKind
{
    Read,
    Set,
    Or,
    And
}