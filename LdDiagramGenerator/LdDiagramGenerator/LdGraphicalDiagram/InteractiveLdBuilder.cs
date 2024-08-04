using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Text;
using LdLib;
using Raylib_CsLo;

namespace LdGraphicalDiagram;

public class InteractiveLdBuilder
{
    public Point Selected { get; private set; }
    public bool IsPopupOpen => _openPopup != PopupKind.Nothing;

    public readonly Dictionary<Point, LdElem> LdElems = new();
    public readonly List<WireT> Wires = new();
    public readonly List<LineNumber> LineNumbers = new();

    private PopupKind _openPopup = PopupKind.Nothing;

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

    public void PlaceItem(Sprite element, string label, Node? node = null) => LdElems[Selected] = new LdElem()
    {
        Kind = element,
        Label = label,
        Node = node,
    };


    public void DeleteItem() => LdElems.Remove(Selected);

    public void EditItemProperties()
    {
        if (!LdElems.TryGetValue(Selected, out var item)) return;
        _openPopup = item.Kind switch
        {
            Sprite.No => PopupKind.NoContactProperties,
            _ => PopupKind.Nothing
        };
    }

    public void SelectLeft()
    {
        Selected = Selected with { X = Selected.X - 1 };
    }

    public void SelectRight() => Selected = Selected with { X = Selected.X + 1 };
    public void SelectUp() => Selected = Selected with { Y = Selected.Y - 1 };
    public void SelectDown() => Selected = Selected with { Y = Selected.Y + 1 };

    private enum PopupKind
    {
        Nothing,
        NoContactProperties
    }

    public void MoveTo(Point point)
    {
        Selected = point;
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
            PlaceItem(Sprite.DownWire, "");
            SelectDown();
        }

        //output end line
        Selected = start with { X = start.X + MaxElementsLen };
        PlaceItem(Sprite.BranchEnd, "");
        SelectDown();
        PlaceItem(Sprite.DownWire, "");
        SelectDown();


        return;
    }
}