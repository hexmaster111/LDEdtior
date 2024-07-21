public class GridLayoutBuilder<TItem>
{
    struct QueItem
    {
        public int Row, Col;
        public TItem Val;
        public string Vals;
    }

    private readonly Queue<QueItem> _writeQueue = new();

    public void Add(int r, int c, TItem val) => _writeQueue.Enqueue(new QueItem()
    {
        Row = r, Col = c, Val = val
    });

    public void Add(int r, int c, string val) => _writeQueue.Enqueue(new QueItem()
    {
        Row = r, Col = c, Vals = val
    });
    
    
    public bool GetNext(out TItem? val,out string? str, out int row, out int col)
    {
        bool b = _writeQueue.TryDequeue(out var qi);
        if (b)
        {
            val = qi.Val;
            row = qi.Row;
            col = qi.Col;
            str = qi.Vals;
        }
        else
        {
            val = default(TItem);
            row = 0;
            col = 0;
            str = null;
        }

        return b;
    }

    
}