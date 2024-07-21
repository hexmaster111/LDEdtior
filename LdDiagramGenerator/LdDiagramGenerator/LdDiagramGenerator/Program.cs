using System.Collections;
using System.Diagnostics;

const int elemWidth = 10;
const int elemHeight = 2;


Node L0X2NO = new()
{
    Attached =
    [
        new Node()
        {
            Attached = [],
            Kind = Node.NodeKind.No,
            Label = "X4"
        }
    ],
    Label = "X2",
    Kind = Node.NodeKind.No
};

LineRootNode Line0Root = new()
{
    Outputs = ["O1"],
    Attached =
    [
        new()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X1"
        },
        new()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X3"
        },
        new()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X5"
        }
    ]
};

PrintRoot(Line0Root);

return;

void PrintRoot(LineRootNode rn)
{
    GridLayoutBuilder<Node> gsb = new();
    PrintNodes(rn.Attached, gsb, 0, 0, new());
    


    Console.Clear();

    while (gsb.GetNext(out var n, out var r, out var c))
    {
        Debug.Assert(n != null);
        Console.SetCursorPosition(c * elemWidth, r * elemHeight);
        Console.Write(NormaliseLabel(n!.Label));
        Console.SetCursorPosition(c * elemWidth, (r * elemHeight) + 1);
        Console.Write(n.Kind.LdSymble());
    }

    Console.SetCursorPosition(0, Console.WindowHeight - 1);
}

void PrintNodes(Node[] attached, GridLayoutBuilder<Node> sb, int r, int c, List<Node> printed)
{
    List<Node> printable = new();

    foreach (var an in attached)
    {
        if (!printed.Contains(an))
        {
            printed.Add(an);
            printable.Add(an);
            sb.Add(r, c, an);
            r += 1;
        }
    }

    c += 1;

    foreach (var an in printable)
    {
        PrintNodes(an.Attached, sb, 0, c, printed);
    }
}


string NormaliseLabel(string lbl)
{
    if (lbl.Length >= elemWidth)
    {
        //trim
        return lbl[..elemWidth];
    }


    if (lbl.Length <= elemWidth)
    {
        //center
        int len = lbl.Length ;
        int pad = (elemWidth - len) / 2;
        for (int i = 0; i < pad; i++)
        {
            lbl = " " + lbl;
        }

        return lbl;
    }

    //exact, do nothing
    return lbl;
}