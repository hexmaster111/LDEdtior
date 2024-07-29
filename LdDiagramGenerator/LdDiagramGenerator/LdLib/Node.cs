using System.Diagnostics;

[DebuggerDisplay("{GetDebuggerDisplay()}")]
public class Node
{
    public Node[] Attached;

    public string Label;
    public NodeKind Kind;


    public enum NodeKind
    {
        No,
        Nc,
        Coil
    }

    public string GetDebuggerDisplay() => $"[{GetHashCode()}:{Kind}:{Label}]";
}

public static class Ext
{
    public static bool IsOutput(this Node.NodeKind k) => k switch
    {
        Node.NodeKind.No => false,
        Node.NodeKind.Nc => false,
        Node.NodeKind.Coil => true,
        _ => throw new ArgumentOutOfRangeException(nameof(k), k, null)
    };
    
    
    public static string LdSymble(this Node.NodeKind kind) => kind switch
    {
        Node.NodeKind.No => "---| |---",
        Node.NodeKind.Nc => "---|/|---",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}