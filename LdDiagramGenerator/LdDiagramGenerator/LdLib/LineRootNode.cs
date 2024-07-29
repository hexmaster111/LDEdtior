public class LineRootNode
{
    public Node[] Attached;


    private void FindOutputNodes(Node[] nodes, List<Node> output)
    {
        foreach (var n in nodes)
        {
            if (n.Kind.IsOutput())
            {
                output.Add(n);
            }

            if (n.Attached.Length == 0) continue;
            FindOutputNodes(n.Attached, output);
        }
    }

    public Node[] Outputs
    {
        get
        {
            var ret = new List<Node>();
            FindOutputNodes(Attached, ret);
            return ret.Distinct().ToArray();
        }
    }
}