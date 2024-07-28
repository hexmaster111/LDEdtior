using System.Diagnostics;
using System.Drawing;
using System.Text;
using ImGuiNET;
using Raylib_CsLo;

namespace LdGraphicalDiagram;

public class InteractiveLdBuilder
{
    public Point Selected { get; private set; }
    public bool IsPopupOpen => _openPopup != PopupKind.Nothing;

    public readonly Dictionary<Point, LdElem> LdElems = new();
    private PopupKind _openPopup = PopupKind.Nothing;

    public struct LdElem
    {
        public Node? Node; //if output or wire
        public Sprite Kind;
        public string Label;
    }


    private void DrawContactProperties()
    {
        ImGui.Begin("Contact");
        {
            var tmp = Encoding.ASCII.GetBytes(LdElems[Selected].Label).Concat(new byte[1]).ToArray();

            if (ImGui.InputText("Label", tmp, (uint)tmp.Length))
            {
                LdElems[Selected] = LdElems[Selected] with
                {
                    Label = Encoding.ASCII.GetString(tmp)
                };
            }

            if (ImGui.Button("Close") ||
                Raylib.IsKeyDown(KeyboardKey.KEY_ESCAPE) ||
                Raylib.IsKeyPressed(KeyboardKey.KEY_ENTER))
            {
                _openPopup = PopupKind.Nothing;
            }
        }
        ImGui.End();
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
            _ => throw new ArgumentOutOfRangeException()
        }, n.Label, n);
    }


    private List<Node> _placed = new();

    public void Ld(Node[] currRoot)
    {
        foreach (var n in currRoot)
        {
            if (_placed.Contains(n)) continue;
            _placed.Add(n);
            Load(n);
            if (!ReferenceEquals(n, currRoot.Last()))
            {
                SelectDown();
                SelectDown();
            }
        }


        if (currRoot.All(x => x == null || x.Attached.Length == 0))
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

    public void Wr(KeyValuePair<Point, LdElem> a, KeyValuePair<Point, LdElem> b)
    {
    }

    public void Wire(Node[] rAttached)
    {
        foreach (var n in rAttached)
        {
            var ld = GetElemFromNode(n);
            Debug.Assert(ld.Value.Node != null);
            foreach (var a in n.Attached)
            {
                var aLd = GetElemFromNode(a);
                Debug.Assert(aLd.Value.Node != null);
                Wr(ld, aLd);
            }
        }
    }

    public void LoadDiagram(LineRootNode r)
    {
        const int maxLen = 9;
        LdElems.Clear();
        var start = Selected = new(1, 1);
        _placed = new();
        Ld(r.Attached);
        Wire(r.Attached);

        //Outputs
        Selected = start with { X = maxLen };
        foreach (var o in r.Outputs)
        {
            PlaceItem(Sprite.Coil, o);

            SelectDown();
            SelectDown();
        }

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
        Selected = start with { X = start.X + maxLen };
        for (int i = 0; i < r.Outputs.Length; i++)
        {
            PlaceItem(Sprite.BranchEnd, "");
            SelectDown();
            PlaceItem(Sprite.DownWire, "");
            SelectDown();
        }

        return;
    }
}