using System.Collections;
using System.Runtime.InteropServices;
using LdLib.CompilerTypes;
using LdLib.Types;

namespace LdLib;

public enum Operator
{
    And,
    Or
}

public interface IPrintable
{
    string ToString();
}

public class LineStatementCompiler
{
    public LineStatementCompiler(IList<LdElement> elements)
    {
        _maxCols = elements.Max(x => x.LinePos.Col) + 1;
        _maxRows = elements.Max(x => x.LinePos.Row) + 1;
        _elements = new LdElement[_maxCols + 1, _maxRows + 1];

        foreach (var elem in elements)
        {
            var row = elem.LinePos.Row;
            var col = elem.LinePos.Col;
           _elements[col, row] = elem;
        }
    }

    private readonly LdElement[ /*col*/, /*row*/] _elements;
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


    //TODO: Think like we are walking down a diagram one element at a time like you would read it
    //            a           b          c           d                   X
    //        ---| |---and---| |---orw---| |---and---| |--------orw-----( )-- 
    //                             |         X           Y      |
    //                             obs------| |---and---| |---obe //TODO: parse this bit
    // a & b & ( ( c & d ) || ( x & y ) ) = x
    // no a & b
    //    ( c & d ) || ( x & y ) = x
    private string Parse(int r, int c)
    {
        List<LdElement> andList = new();
        var o = "";

        while (true)
        {
            var elem = GetElement(c, r);
            if (elem == null) throw new LdException("Expected Element", new RowCol(r, c));
            switch (elem.ElementType)
            {
                case ElementType.OrWire: // handles c & d
                {
                    //Figuring out what the elem below is

                    var oneBelow = GetElement(c, r + 1);

                    if (oneBelow?.ElementType == ElementType.OrBranchStart) goto start_or_wire;
                    if (oneBelow?.ElementType == ElementType.OrBranchEnd) goto end_or_wire;
                    throw new LdException("Expected OrBranchStart or OrBranchEnd", new RowCol(r, c));

                   


                    end_or_wire: // the elem r + 1 is an OrBranchEnd
                    if (andList.Count == 0) throw new LdException("No elements in branch!", new RowCol(r, c));

                    {
                        var lastAndElement = andList.Last();
                        foreach (var e in andList)
                        {
                            o += e.Label;
                            if (!ReferenceEquals(e, lastAndElement))
                            {
                                o += " & ";
                            }
                        }

                        andList.Clear();

                        o += ") ||";
                    }


                    start_or_wire: // the elem r + 1 is an OrBranchStart
                    if (andList.Count != 0)
                    {
                        //print a & b
                        var lastAndElement = andList.Last();
                        foreach (var e in andList)
                        {
                            o += e.Label;
                            if (!ReferenceEquals(e, lastAndElement))
                            {
                                o += " & ";
                            }
                        }

                        andList.Clear();

                        o += "&(";
                    }

                    o += '(';
                    goto nextElement;
                }

                case ElementType.OrBranchEnd: break;
                case ElementType.OrBranchStart: break;

                case ElementType.NormallyOpenContact:
                case ElementType.NormallyClosedContact:
                    andList.Add(elem);
                    goto nextElement;

                case ElementType.Wire:
                    nextElement:
                    c += 1;
                    continue;

                case ElementType.Coil:
                case ElementType.NegatedCoil:
                case ElementType.SetLatchCoil:
                case ElementType.ResetLatchCoil:

                    if (andList.Count != 0)
                    {
                        var lastAndElement = andList.Last();
                        foreach (var e in andList)
                        {
                            o += e.Label;
                            if (!ReferenceEquals(e, lastAndElement))
                            {
                                o += " & ";
                            }
                        }
                    }
                    
                    return "(" + o + ") = " + elem.Label;
                case ElementType.Nothing:
                default:
                    throw new LdException("Unknown Element", new RowCol(r, c));
            }
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