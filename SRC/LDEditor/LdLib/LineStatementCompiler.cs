using System.Collections;
using System.Runtime.InteropServices;
using LdLib.Types;

namespace LdLib;

public enum Operator
{
    And,
    Or
}

public struct ThisOrThat<TThis, TThat>
{
    public TThis? This;
    public TThat? That;

    public ThisOrThat(TThis? @this, TThat? that)
    {
        if (@this == null && that == null) throw new Exception("Both This and That are null");
        if (@this != null && that != null) throw new Exception("Both This and That are not null");
        This = @this;
        That = that;
    }
}

public class Node
{
    public Operator Operator { get; set; }
    public ThisOrThat<Node, LdElement> Left { get; set; }
    public ThisOrThat<Node, LdElement> Right { get; set; }


    public override string ToString()
    {
        if (Element != null) return Element.Label;
        var left = Left?.ToString() ?? "";
        var right = Right?.ToString() ?? "";
        return $"({left} {Operator} {right})";
    }
}

public class LineStatementCompiler
{
    public LineStatementCompiler(IList<LdElement> elements)
    {
        _maxCols = elements.Max(x => x.LinePos.Col);
        _maxRows = elements.Max(x => x.LinePos.Row);
        _elements = new LdElement[_maxCols + 1, _maxRows + 1];
        _coils = new LdElement[_maxRows + 1];

        foreach (var elem in elements)
        {
            var row = elem.LinePos.Row;
            var col = elem.LinePos.Col;
            if (elem.ElementType is
                ElementType.Coil or
                ElementType.NegatedCoil or
                ElementType.SetLatchCoil or
                ElementType.ResetLatchCoil)
            {
                _coils[row] = elem;
                continue;
            }

            _elements[col, row] = elem;
        }
    }

    private readonly LdElement[ /*col*/, /*row*/] _elements;
    private readonly LdElement[] _coils;
    private readonly int _maxCols;
    private readonly int _maxRows;


    private LdElement? GetElement(int col, int row)
    {
        if (col < 0 || col > _maxCols || row < 0 || row > _maxRows) return null;
        return _elements[col, row];
    }

    private LdElement? GetElement(RowCol pos) => GetElement(pos.Col, pos.Row);

    public override string ToString()
    {
        var root = Parse(0, 0);

        return root.ToString();
    }

    private Node Parse(int r, int c)
    {
        if (r > _maxRows) return new Node();
        if (c >= _maxCols) return new Node();
        var elem = GetElement(c, r);
        if (elem == null) throw new LdException("Expected Element", new(r, c));
        switch (elem.ElementType)
        {
            case ElementType.Nothing: throw new LdException("Expected Element", new(r, c));
            case ElementType.Wire: return Parse(r, c + 1);
            case ElementType.NormallyOpenContact:
            case ElementType.NormallyClosedContact:

                return new Node()
                {
                    Operator = Operator.And,
                    Left = new Node()
                    {
                        Element = elem
                    },
                    Right = Parse(r, c + 1)
                };


            default: throw new Exception();
        }
    }
}

class LdException : Exception
{
    public RowCol Where;

    public LdException(string msg, RowCol where) : base(msg)
    {
        Where = where;
    }
}