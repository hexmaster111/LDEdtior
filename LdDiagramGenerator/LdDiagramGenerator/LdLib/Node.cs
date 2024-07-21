public class Node
{
    public Node[] Attached;

    public string Label;
    public NodeKind Kind;


    public enum NodeKind
    {
        No,
        Nc
    }
}

public static class Ext
{
    public static string LdSymble(this Node.NodeKind kind) => kind switch
    {
        Node.NodeKind.No => "---| |---",
        Node.NodeKind.Nc => "---|/|---",
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };
}