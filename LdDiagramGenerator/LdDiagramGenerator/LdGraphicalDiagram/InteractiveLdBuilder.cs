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

    
    
    public void Wire(Node[] rAttached)
    {
        foreach (var n in rAttached)
        {
            var elem = GetElemFromNode(n);

            MoveTo(elem.Key);
            SelectRight();

            var othersBelowMeConnect = OthersFrontBelowMeConnect(elem.Key);
            var connectToOthersAboveMe = ConnectToOthersFrontAboveMe(elem.Key);
            var connectToInfront = ConnectToOthersForward(elem.Key);
            
            Console.WriteLine($"Wire {n.GetDebuggerDisplay()}");

            if (othersBelowMeConnect && !connectToOthersAboveMe)
            {
                PlaceItem(Sprite.OrBranch, "");
                SelectDown();
                PlaceItem(Sprite.DownWire, "");
            }
            else if (connectToOthersAboveMe && !othersBelowMeConnect)
            {
                PlaceItem(Sprite.OrBranchEnd, "");
            }
            else if (othersBelowMeConnect && connectToOthersAboveMe)
            {
                PlaceItem(Sprite.BranchEnd, "");
                SelectDown();
                PlaceItem(Sprite.DownWire, "");
            }else if (connectToInfront)
            {
                PlaceItem(Sprite.Wire, "");
            }
            else
            {
                
            }

            if (n.Attached.Length == 0) continue;
            Wire(n.Attached);
        }
    }

    /// <summary>
    ///     returns if the node that is visualy above us connects with us
    /// </summary>
    private bool ConnectToOthersForward(Point pos)
    {
        if (!LdElems.TryGetValue(pos, out var me)) return false;//throw new Exception($"Nothing here {pos}");
        if (me.Node == null) throw new Exception("me.Node == null");
        var forward = pos with { X = pos.X + 2};
        if (!LdElems.TryGetValue(forward, out var inFront)) return false;
        if (inFront.Node == null) return false; //this is a wire or something...

        if (me.Node.Attached.Contains(inFront.Node))
        {
            //i connect to the one in front of me
            return true;
        }
        
        // foreach (var n in inFront.Node.Attached)
        // {
        //     foreach (var meNode in me.Node.Attached)
        //     {
        //         if (ReferenceEquals(n, meNode))
        //         {
        //             return true;
        //         }
        //     }
        // }

        return false;
    }
    
    /// <summary>
    ///     returns if the node that is visualy above us connects with us
    /// </summary>
    private bool ConnectToOthersFrontAboveMe(Point pos)
    {
        if (!LdElems.TryGetValue(pos, out var me)) return false;
        if (me.Node == null) throw new Exception("me.Node == null");
        var aboveMe = pos with { Y = pos.Y - 2 };
        if (!LdElems.TryGetValue(aboveMe, out var below)) return false;
        if (below.Node == null) return false; //this is a wire or something...

        //if the node below me connects to any i connect with, true
        foreach (var n in below.Node.Attached)
        {
            foreach (var meNode in me.Node.Attached)
            {
                if (ReferenceEquals(n, meNode))
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    ///     Returns if the node that is visualy below us needs to connect with this wire
    /// </summary>
    private bool OthersFrontBelowMeConnect(Point pos)
    {
        if (!LdElems.TryGetValue(pos, out var me)) return false;
        if (me.Node == null) throw new Exception("me.Node == null");
        var belowMePt = pos with { Y = pos.Y + 2 };
        if (!LdElems.TryGetValue(belowMePt, out var below)) return false;
        if (below.Node == null) return false; //this is a wire or something...

        //if the node below me connects to any i connect with, true
        foreach (var n in below.Node.Attached)
        {
            foreach (var meNode in me.Node.Attached)
            {
                if (ReferenceEquals(n, meNode))
                {
                    return true;
                }
            }
        }

        return false;
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
            PlaceItem(Sprite.Coil, o.Label);

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
        PlaceItem(Sprite.BranchEnd, "");
        SelectDown();
        PlaceItem(Sprite.DownWire, "");
        SelectDown();


        return;
    }
}