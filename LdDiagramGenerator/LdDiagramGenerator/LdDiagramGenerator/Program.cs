using System.Collections;

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
        }
    ]
};

PrintRoot(Line0Root);

return;


void PrintRoot(LineRootNode rn)
{
    GridLayoutBuilder<Node> gsb = new();
    PrintNodes(rn.Attached, gsb, 0, 0, new());
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


class GridLayoutBuilder<TItem> 
{
    struct QueItem
    {
        public int Row, Col;
        public TItem Val;
    }

    private readonly Queue<QueItem> _writeQueue = new();

    public void Add(int r, int c, TItem val) => _writeQueue.Enqueue(new QueItem()
    {
        Row = r, Col = c, Val = val
    });

    public bool GetNext(out TItem? val, out int row, out int col)
    {

        bool b = _writeQueue.TryDequeue(out var qi);
        if (b)
        {
            val = qi.Val;
            row = qi.Row;
            col = qi.Col;
        }
        else
        {
            val = default(TItem);
            row = 0;
            col = 0;
        }

        return b;
    }

    
}