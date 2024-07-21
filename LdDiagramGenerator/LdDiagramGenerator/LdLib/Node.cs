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