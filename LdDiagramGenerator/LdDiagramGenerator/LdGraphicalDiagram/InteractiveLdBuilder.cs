using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using LdLib;
using Raylib_CsLo;

namespace LdGraphicalDiagram;

public class InteractiveLdBuilder
{
    private Point Selected { get; set; }
    public Point SelectedNode { get; private set; }
    public bool IsPopupOpen => _openPopup != PopupKind.Nothing;
    public Action<LdDocument>? DocumentLoadedCallback { get; set; }

    public readonly Dictionary<Point, LdElem> LdElems = new();
    public readonly List<WireT> Wires = new();
    public readonly List<LineNumber> LineNumbers = new();

    private PopupKind _openPopup = PopupKind.Nothing;
    private LdDocument _currDoc;


    public struct LineNumber
    {
        public Point GirdPos;
        public int LineNo;
    }

    public struct LdElem
    {
        public Node? Node; //if output or wire
        public Sprite Kind;
        public string Label;
    }


    private void DrawContactProperties()
    {
        // ImGui.Begin("Contact");
        // {
        //     var tmp = Encoding.ASCII.GetBytes(LdElems[Selected].Label).Concat(new byte[1]).ToArray();
        //
        //     if (ImGui.InputText("Label", tmp, (uint)tmp.Length))
        //     {
        //         LdElems[Selected] = LdElems[Selected] with
        //         {
        //             Label = Encoding.ASCII.GetString(tmp)
        //         };
        //     }
        //
        //     if (ImGui.Button("Close") ||
        //         Raylib.IsKeyDown(KeyboardKey.KEY_ESCAPE) ||
        //         Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
        //     {
        _openPopup = PopupKind.Nothing; //disable this when im done with the other ui.
        //     }
        // }
        // ImGui.End();
    }

    public void DrawPopup()
    {
        switch (_openPopup)
        {
            default:
            case PopupKind.Nothing:
                break;
            case PopupKind.NoContactProperties:
                DrawContactProperties();
                break;
        }
    }

    public void PlaceItem(Sprite element, string label, Node? node = null) =>
        LdElems[Selected] = new LdElem()
        {
            Kind = element,
            Label = label,
            Node = node,
        };

    private Node InsertCoil(Node.NodeKind kind, string label)
    {
        var n = LdElems.GetValueOrDefault(SelectedNode, default);
        if (n.Node == null) return null;

        while (n.Node.Attached.Length != 0)
        {
            SelectNodeRight();
            n = LdElems.GetValueOrDefault(SelectedNode, default);
            if (n.Node == null) return null;
        }


        var newNode = new Node() { Label = label, Kind = kind, Attached = [] };

        if (n.Node.Kind.IsOutput())
        {
            //we already have coiles in this line, find nodes that connect to this coil, and tie the new 
            //coil into that list
            var attached = GetNodesThatConnectToMe(n.Node);
            foreach (var an in attached)
            {
                //an.attached.add newNode
                an.Attached = an.Attached.Concat([newNode]).ToArray();
            }

            return newNode;
        }
        else
        {
            //We are somewhere in the doc still not on the outputs
            n.Node.Attached = [newNode];
            return newNode;
        }


        return null;
    }

    private Node InsertContact(Node.NodeKind kind, string label)
    {
        if (!LdElems.TryGetValue(SelectedNode, out var ldElem)) return null;
        if (ldElem.Node == null) return null;
        var cn = _currDoc.GetAllDistinctNodes().First(x => ReferenceEquals(x, ldElem.Node!));

        var attached = cn.Attached;

        var newNode = new Node()
        {
            Label = label,
            Attached = attached,
            Kind = kind
        };

        cn.Attached = [newNode];
        return newNode;
    }

    public void InsertNewNode(Node.NodeKind kind, string label)
    {
        Node newNode = null;
        switch (kind)
        {
            case Node.NodeKind.No:
            case Node.NodeKind.Nc:
                newNode = InsertContact(kind, label);
                break;
            case Node.NodeKind.Coil:
                newNode = InsertCoil(kind, label);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(kind), kind, null);
        }

        if (newNode == null) return;

        LoadDocument(_currDoc);
        var nn = LdElems.FirstOrDefault(x => ReferenceEquals(x.Value.Node, newNode));
        if (nn.Value.Node == null) return;
        SelectedNode = nn.Key;
    }

    public void EditItemProperties()
    {
        if (!LdElems.TryGetValue(Selected, out var item)) return;
        _openPopup = item.Kind switch
        {
            Sprite.No => PopupKind.NoContactProperties,
            _ => PopupKind.Nothing
        };
    }

    public void InsertNewLine()
    {
        var newNode = new Node() { Kind = Node.NodeKind.No, Attached = [], Label = "LBL" };
        _currDoc.Lines.Add(new LineRootNode([newNode]));
        LoadDocument(_currDoc);
        SelectedNode = FindLdElemFromNode(newNode).Key;
    }


    private KeyValuePair<Point, LdElem> FindLdElemFromNode(Node n) => LdElems
        .FirstOrDefault(x => ReferenceEquals(x.Value.Node, n));

    private Node[] GetNodesThatConnectToMe(Node me) => _currDoc
        .GetAllDistinctNodes()
        .Where(x => x.Attached
            .Contains(me)).ToArray();

    public void BackspaceNode()
    {
        var currNode = LdElems.GetValueOrDefault(SelectedNode, default);
        if (currNode.Node == null) return;
        var toRemove = currNode.Node!;

        var connectToMe = _currDoc
            .GetAllDistinctNodes()
            .Where(x => x.Attached
                .Contains(currNode.Node)).ToArray();

        if (connectToMe.Length != 0)
        {
            var attachedTo = toRemove.Attached;

            foreach (var conNode in connectToMe)
            {
                conNode.Attached = attachedTo;
            }
            //TODO: this dosnt work with branches

            LoadDocument(_currDoc);
            SelectedNode = GetElemFromNode(connectToMe[0]).Key;
        }
    }

    public void SelectNodeLeft()
    {
        var currNode = LdElems.GetValueOrDefault(SelectedNode, default);
        if (currNode.Node == null) return;
        var connectToMe = GetNodesThatConnectToMe(currNode.Node);

        var fcm = connectToMe.FirstOrDefault();
        if (fcm == null) return;
        var cn = FindLdElemFromNode(fcm);
        SelectedNode = cn.Key;
    }

    public void SelectNodeRight()
    {
        var currNode = LdElems.GetValueOrDefault(SelectedNode, default);
        if (currNode.Node == null) return;
        var firstAttached = currNode.Node.Attached.FirstOrDefault();
        if (firstAttached == null) return;
        var cn = FindLdElemFromNode(firstAttached);
        SelectedNode = cn.Key;
    }

    private int MyIdx(Node[] list, Node me)
    {
        for (int i = 0; i < list.Length; i++)
        {
            if (ReferenceEquals(list[i], me))
            {
                return i;
            }
        }

        return -1;
    }

    public void SelectNodeDown()
    {
        var currNode = LdElems.GetValueOrDefault(SelectedNode, default);
        if (currNode.Node == null) return;

        var attachTo = GetNodesThatConnectToMe(currNode.Node);
        if (attachTo.Length == 0)
        {
            //TODO: make some kinda ghost wire? or something, to show the user they are selecting an or branch to add
            //SAME AS THE MAX LEN CHECK
            return;
        }

        if (attachTo[0].Attached.Length > 1)
        {
            var firstAttached = attachTo[0].Attached;
            int ourIdx = MyIdx(firstAttached, currNode.Node);
            if (ourIdx == -1) throw new Exception("HOW~!");


            if (ourIdx + 1 /*going down */ >= firstAttached.Length)
            {
                //TODO: make some kinda ghost wire? or something, to show the user they are selecting an or branch to add
                return;
            }

            var selNode = firstAttached[ourIdx + 1];
            SelectedNode = FindLdElemFromNode(selNode).Key;
        }
    }

    public void SelectNodeUp()
    {
        var currNode = LdElems.GetValueOrDefault(SelectedNode, default);
        if (currNode.Node == null) return;

        var attachTo = GetNodesThatConnectToMe(currNode.Node);
        if (attachTo.Length == 0)
        {
            return;
        }

        if (attachTo[0].Attached.Length > 1)
        {
            var firstAttached = attachTo[0].Attached;
            int ourIdx = MyIdx(firstAttached, currNode.Node);
            if (ourIdx == -1) throw new Exception("HOW~!");

            if (ourIdx - 1 < 0 || ourIdx - 1 >= firstAttached.Length) return;
            var selNode = firstAttached[ourIdx - 1];
            SelectedNode = FindLdElemFromNode(selNode).Key;
        }
    }

    private int MyLineIdx(Node me)
    {
        for (var index = 0; index < _currDoc.Lines.Count; index++)
        {
            var line = _currDoc.Lines[index];
            foreach (var dn in line.GetAllNodes().Distinct())
            {
                if (ReferenceEquals(me, dn))
                {
                    return index;
                }
            }
        }

        return -1;
    }

    public void SelectLineDown()
    {
        var n = LdElems.GetValueOrDefault(SelectedNode);
        if (n.Node == null) return;

        var curr = MyLineIdx(n.Node);
        if (curr == -1) return;

        if (curr + 1 >= 0 && curr + 1 < _currDoc.Lines.Count)
            SelectedNode = GetElemFromNode(_currDoc.Lines[curr + 1].Attached.First()).Key;
    }

    public void SelectLineUp()
    {
        var n = LdElems.GetValueOrDefault(SelectedNode);
        if (n.Node == null) return;

        var curr = MyLineIdx(n.Node);
        if (curr == -1) return;

        if (curr - 1 >= 0 && curr - 1 < _currDoc.Lines.Count)
            SelectedNode = GetElemFromNode(_currDoc.Lines[curr - 1].Attached.First()).Key;
    }

    private void SelectLeft() => Selected = Selected with { X = Selected.X - 1 };
    private void SelectRight() => Selected = Selected with { X = Selected.X + 1 };
    private void SelectUp() => Selected = Selected with { Y = Selected.Y - 1 };
    private void SelectDown() => Selected = Selected with { Y = Selected.Y + 1 };

    private enum PopupKind
    {
        Nothing,
        NoContactProperties
    }


    /* X1              X2        X4      X7        X8               O1
     * ---| |---+-----| |---+---| |---+---| |---+---| |---+---------( )---
     * X3       |           |    X6   |         | X9      |
     * ---| |---+           +---| |---+         +---|\|---+
     * X5       |                     |                   |
     * ---| |---+                     |                   |
     * X11          attached to X7    |                   |
     * ---| |-------------------------+                   |
     *                                                    |
     * X10          *x10 node with no attached*           |
     * ---| |---------------------------------------------+
     */

    /*diagram loader stuff*/

    private void Load(Node n)
    {
        PlaceItem(n.Kind switch
        {
            Node.NodeKind.No => Sprite.No,
            Node.NodeKind.Nc => Sprite.Nc,
            Node.NodeKind.Coil => Sprite.Coil,
            _ => throw new ArgumentOutOfRangeException()
        }, n.Label, n);
    }

    private List<Node> _placed = new();

    public void Ld(Node[] currRoot)
    {
        foreach (var n in currRoot)
        {
            if (_placed.Contains(n) || n.Kind.IsOutput()) continue;
            _placed.Add(n);
            Load(n);
            if (!ReferenceEquals(n, currRoot.Last()))
            {
                SelectDown();
                SelectDown();
            }
        }


        if (currRoot.All(x => x.Attached == null || x.Attached.Length == 0))
        {
            //end of diagram
            return;
        }

        Selected = Selected with { Y = Selected.Y - ((currRoot.Length - 1) * 2) };

        SelectRight();
        SelectRight();

        //Next root
        foreach (var n in currRoot)
        {
            Ld(n.Attached);
        }
    }


    private KeyValuePair<Point, LdElem> GetElemFromNode(Node n) =>
        LdElems.FirstOrDefault(x => ReferenceEquals(x.Value.Node, n));

    public struct WireT
    {
        public const int Thickness = 3;
        public Vector2[] Points;
        public Node SourceNode;
    }


    public void Wire(Node[] rAttached)
    {
        // Wires
        foreach (var currentNode in rAttached)
        {
            foreach (var nextNode in currentNode.Attached)
            {
                ConnectNode(currentNode, nextNode);
            }

            Wire(currentNode.Attached);
        }
    }

    private static Vector2 ToVect(Point pt) => new Vector2(pt.X, pt.Y);

    /// <summary>
    ///     Adds wires into the <see cref="Wires"/> List
    /// </summary>
    private void ConnectNode(Node currentNode, Node nextNode)
    {
        var currentElem = GetElemFromNode(currentNode);
        var nextElem = GetElemFromNode(nextNode);

        //We are in the same line, so its just A---B
        if (currentElem.Key.Y == nextElem.Key.Y)
        {
            Wires.Add(new WireT
            {
                Points = [ToVect(currentElem.Key), ToVect(nextElem.Key)],
                SourceNode = currentNode
            });
            return;
        }

        //We are going to need to go down, and over, so 4 points.
        /*  ---| |+--+
         *           |
         *           +--+| |---
         *
         */
        Wires.Add(new WireT()
        {
            Points =
            [
                ToVect(currentElem.Key),
                new(nextElem.Key.X - .5f, currentElem.Key.Y),
                new(nextElem.Key.X - .5f, nextElem.Key.Y),
                ToVect(nextElem.Key)
            ],
            SourceNode = currentNode
        });
    }

    public void LoadDocument(LdDocument ldDocument)
    {
        LdElems.Clear();
        Wires.Clear();
        _placed.Clear();
        LineNumbers.Clear();
        SelectedNode = new Point(1, 0);

        _currDoc = ldDocument;

        int cols = 0;
        int lineNo = 1;
        foreach (var line in ldDocument.Lines)
        {
            LoadDiagram(line, new Point(1, cols));
            LineNumbers.Add(new LineNumber()
            {
                LineNo = lineNo++,
                GirdPos = new Point(-1, cols)
            });


            int c = LdElems.Max(x => x.Key.Y);
            cols = c + 2;
        }

        DocumentLoadedCallback?.Invoke(ldDocument);
    }

    public const int MaxElementsLen = 9;

    private void LoadDiagram(LineRootNode r, Point start)
    {
        Selected = start;
        Ld(r.Attached);


        //Outputs
        Selected = start with { X = MaxElementsLen };
        foreach (var o in r.Outputs)
        {
            PlaceItem(Sprite.Coil, o.Label, o);

            SelectDown();
            SelectDown();
        }

        Selected = start with { X = start.X - 1 };
        Wire(r.Attached);

        //First row line
        Selected = start with { X = start.X - 1 };
        for (int i = 0; i < r.Attached.Length; i++)
        {
            PlaceItem(Sprite.BranchStart, "");
            SelectDown();
            SelectDown();
        }


        //output end line
        Selected = start with { X = start.X + MaxElementsLen };

        for (int i = 0; i < r.Outputs.Length; i++)
        {
            PlaceItem(Sprite.BranchEnd, "");
            SelectDown();
            SelectDown();
        }


        return;
    }
}

