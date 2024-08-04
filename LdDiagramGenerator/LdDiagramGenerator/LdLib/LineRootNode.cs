using Newtonsoft.Json;

namespace LdLib;

public class LdDocument(LineRootNode[] lines)
{
    public LineRootNode[] Lines = lines;
}

// ---------------- save load --------------------------

#region Save Load Types

[JsonObject(MemberSerialization.OptIn)]
public class SaveNode
{
    [JsonProperty] public int NodeId;
    [JsonProperty] public int[] Attached = [];
    [JsonProperty] public string Label;
    [JsonProperty] public Node.NodeKind Kind;

    [JsonConstructor]
    public SaveNode(int nodeId, int[] attached, string label, Node.NodeKind kind)
    {
        NodeId = nodeId;
        Attached = attached;
        Label = label;
        Kind = kind;
    }


    public SaveNode(int nodeId, Node.NodeKind kind, string label)
    {
        NodeId = nodeId;
        Kind = kind;
        Label = label;
    }
}

[JsonObject(MemberSerialization.OptIn)]
public class LineRootNodeSaveObject
{
    [JsonProperty] public SaveNode[] Nodes;
    [JsonProperty] public int[] RootNodes;

    public LineRootNodeSaveObject(SaveNode[] nodes, int[] rootNodes)
    {
        Nodes = nodes;
        RootNodes = rootNodes;
    }
}

#endregion

public class LineRootNode(Node[] attached)
{
    public Node[] Attached = attached;

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


    public string SaveString()
    {
        int idAccm = 0;
        var allDistinctNodes = GetAllNodes().Select(x =>
            new
            {
                RefNode = x,
                SaveNode = new SaveNode(idAccm++, x.Kind, x.Label)
            }
        ).ToArray();

        foreach (var n in allDistinctNodes)
            n.SaveNode.Attached = GetAttachedNodeIds(n.RefNode.Attached);

        var nodes = allDistinctNodes.Select(x => x.SaveNode).ToArray();

        List<int> rootIds = new();
        foreach (var rn in Attached)
        {
            var renode = allDistinctNodes.First(x => ReferenceEquals(x.RefNode, rn));
            rootIds.Add(renode.SaveNode.NodeId);
        }


        var saveObj = new LineRootNodeSaveObject(nodes, rootIds.ToArray());
        return JsonConvert.SerializeObject(saveObj, Formatting.Indented);

        int[] GetAttachedNodeIds(Node[] attached)
        {
            List<int> ret = [];
            foreach (var at in attached)
            {
                foreach (var n in allDistinctNodes)
                {
                    if (ReferenceEquals(at, n.RefNode))
                    {
                        ret.Add(n.SaveNode.NodeId);
                    }
                }
            }

            return ret.ToArray();
        }
    }

    private void VisitAllNodes(Action<Node> visitingFunc, Node[] nodes)
    {
        foreach (var n in nodes)
        {
            VisitAllNodes(visitingFunc, n.Attached);
            visitingFunc(n);
        }
    }

    private List<Node> GetAllNodes()
    {
        List<Node> ret = new();
        VisitAllNodes(n => ret.Add(n), Attached);
        return ret.Distinct().ToList();
    }

    public static LineRootNode Load(string saveString)
    {
        LineRootNodeSaveObject? so = JsonConvert.DeserializeObject<LineRootNodeSaveObject>(saveString);
        if (so == null) return null;

        List<(Node Node, SaveNode Save)> nodes = so.Nodes.Select(sn => new ValueTuple<Node, SaveNode>(new Node()
        {
            Kind = sn.Kind,
            Label = sn.Label,
            Attached = new Node[sn.Attached.Length]
        }, sn)).ToList();
        var nd = nodes.ToDictionary(x => x.Save.NodeId);

        foreach (var tn in nodes)
        {
            for (var i = 0; i < tn.Node.Attached.Length; i++)
            {
                tn.Node.Attached[i] = nd[tn.Save.Attached[i]].Node;
            }
        }

        var n = nodes
            .Where(x => so.RootNodes.Contains(x.Save.NodeId))
            .Select(x => x.Node)
            .ToArray();
        return new LineRootNode(n);
    }
}