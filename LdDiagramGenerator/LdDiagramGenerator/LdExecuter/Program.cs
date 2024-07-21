/*     X1         X2         X4                     O1
 * ---|  |---+---|  |-------|  |-------------------( )---
 *     X3    |
 * ---|  |---+
 *    X5     |
 * ---|  |---+
 *
 */

Dictionary<string, bool> io = new()
{
    { "X1", false },
    { "X2", false },
    { "X3", false },
    { "X4", false },
    { "X5", false },
    { "O1", false }
};

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

ExecuteLine(Line0Root);

foreach (var v in io)
{
    Console.Write(v.Key);
    Console.Write(" ");
    Console.WriteLine(v.Value.ToString());
}

return;

bool Examine(Node n)
{
    bool examine = n.Kind switch
    {
        Node.NodeKind.No => io[n.Label],
        Node.NodeKind.Nc => !io[n.Label],
        _ => throw new ArgumentOutOfRangeException()
    };

    if (!examine) return false;

    //we examined to the end
    if (n.Attached.Length == 0) return true;

    foreach (var an in n.Attached)
    {
        if (Examine(an)) return true;
    }

    //we examend all attached, and none are true, so we return false for this branch
    return false;
}

void ExecuteLine(LineRootNode l)
{
    bool outputStateToSet = false;
    foreach (var n in l.Attached)
    {
        if (Examine(n))
        {
            outputStateToSet = true;
            break;
        }
    }

    foreach (var os in l.Outputs)
    {
        io[os] = outputStateToSet;
    }
}