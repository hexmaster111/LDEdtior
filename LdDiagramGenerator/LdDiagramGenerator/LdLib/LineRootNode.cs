using Newtonsoft.Json;

namespace LdLib;

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


    // ---------------- save load --------------------------

    [JsonObject]
    public class SaveNode
    {
        public int NodeId;
        public int[] Attached;
    }

    [JsonObject]
    private class SaveObject
    {
        public SaveNode[] Nodes;
    }


    public string SaveString()
    {
        int idAccm = 0;
        var allDistinctNodes = GetAllNodes().Select(x =>
            new
            {
                RefNode = x,
                SaveNode = new SaveNode { NodeId = idAccm++ }
            }
        ).ToArray();

        foreach (var n in allDistinctNodes)
        {
            n.SaveNode.Attached = GetAttachedNodeIds(n.RefNode.Attached);
        }

        var saveObj = new SaveObject()
        {
            Nodes = allDistinctNodes.Select(x => x.SaveNode).ToArray()
        };

        return JsonConvert.SerializeObject(saveObj, Formatting.Indented);

        int[] GetAttachedNodeIds(Node[] attached)
        {
            List<int> ret = new();
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
            visitingFunc(n);
            VisitAllNodes(visitingFunc, n.Attached);
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
        return null;
    }
}