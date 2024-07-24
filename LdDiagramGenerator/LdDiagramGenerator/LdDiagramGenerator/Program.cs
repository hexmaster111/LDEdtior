using System.Collections;
using System.Diagnostics;

const int elemWidth = 9;
const int elemHeight = 2;

Node L0X7NO = new()
{
    Attached =
    [
        new Node()
        {
            Attached = [],
            Kind = Node.NodeKind.No,
            Label = "X8"
        },
        new Node()
        {
            Attached =
            [],
            Kind = Node.NodeKind.Nc,
            Label = "X9"
        }
    ],
    Label = "X7",
    Kind = Node.NodeKind.No
};

Node L0X2NO = new()
{
    Attached =
    [
        new Node()
        {
            Attached = [L0X7NO],
            Kind = Node.NodeKind.No,
            Label = "X4"
        },
        new Node()
        {
            Attached = [L0X7NO],
            Kind = Node.NodeKind.Nc,
            Label = "X6"
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

    while (gsb.GetNext(out var n, out var s, out var r, out var c))
    {
        if (n != null)
        {
            Console.SetCursorPosition(c * elemWidth, r * elemHeight);
            Console.Write(NormaliseLabel(n!.Label));
            Console.SetCursorPosition(c * elemWidth, (r * elemHeight) + 1);
            Console.Write(n.Kind.LdSymble());
        }
        else if (s != null)
        {
            Console.SetCursorPosition(c * elemWidth, r * elemHeight);
            Console.Write(s);
        }
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

            if (c > 0 && r > 0)
            {
                //print left side | wire
                sb.Add(r, c, "|");
            }

            if (an.Attached.Length != 0 && r != 0)
            {
                // branch up | right side
                sb.Add(r, c + 1, "|");
            }

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
        int len = lbl.Length;
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