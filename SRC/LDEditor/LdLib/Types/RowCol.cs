namespace LdLib.Types;

public struct RowCol
{
    public int Row;
    public int Col;

    public RowCol(int r, int c)
    {
        Row = r;
        Col = c;
    }


    public override bool Equals(object? obj)
    {
        if (obj is not RowCol rc) return false;
        return rc.Row == Row && rc.Col == Col;
    }

    public override int GetHashCode() => HashCode.Combine(Row.GetHashCode(), Col.GetHashCode());

    public override string ToString() => $"Row {Row} - Col {Col}";

    public static bool operator ==(RowCol left, RowCol right) => left.Equals(right);

    public static bool operator !=(RowCol left, RowCol right) => !(left == right);
}