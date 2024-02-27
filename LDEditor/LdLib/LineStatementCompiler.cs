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


    private LdElement? GetElement(int row, int col)
    {
        if (col < 0 || col > _maxCols || row < 0 || row > _maxRows) return null;
        return _elements[col, row];
    }

    private LdElement? GetElement(RowCol pos) => GetElement(pos.Row, pos.Col);

    private string _result = "";

    public void Parse()
    {
        var root = Parse(0, 0);
        _result = root;
    }

    public override string ToString() => _result;


    //TODO: Think like we are walking down a diagram one element at a time like you would read it
    //            a           b          c           d                   X
    //        ---| |---and---| |---orw---| |---and---| |--------orw-----( )-- 
    //                             |         X           Y      |
    //                             obs------| |---and---| |---obe //TODO: parse this bit
    // a & b & ( ( c & d ) || ( x & y ) ) = x
    // no a & b
    //    ( c & d ) || ( x & y ) = x


    /// <param name="isRecursiveOrBranchCall">used to tell if we end parsing at OBE or at COIL element</param>
    private string Parse(int r, int c, bool isRecursiveOrBranchCall = false)
    {
        List<LdElement> andList = new();
        var o = "";
        Stack<RowCol> nextBranchStart = new();
        while (true)
        {
            var elem = GetElement(r, c);
            if (elem == null) throw new LdException("Expected Element", new RowCol(r, c));
            switch (elem.ElementType)
            {
                case ElementType.OrWire: // handles c & d
                {
                    //Figuring out what the elem below is

                    var oneBelow = GetElement(r + 1, c);

                    if (oneBelow?.ElementType == ElementType.OrBranchStart)
                    {
                        nextBranchStart.Push(oneBelow.LinePos);
                        goto start_or_wire;
                    }

                    if (oneBelow?.ElementType == ElementType.OrBranchEnd)
                    {
                        goto end_or_wire;
                    }

                    throw new LdException("Expected OrBranchStart or OrBranchEnd", new RowCol(r, c));


                    end_or_wire: // the elem r + 1 is an OrBranchEnd
                    if (andList.Count == 0) goto nextElement;

                    var lastAndElement = andList.Last();

                    o += "(";
                    foreach (var e in andList)
                    {
                        o += e.Label;
                        if (!ReferenceEquals(e, lastAndElement))
                        {
                            o += "&";
                        }
                    }

                    o += ")";


                    andList.Clear();

                    o += "||";

                    //we hit the end of this and branch, lets go do the next or

                    var pos = nextBranchStart.Pop();


                    var next = "(" + Parse(pos.Row, pos.Col, true) + ")";

                    o +=  next;

                    break;

                    start_or_wire: // the elem r + 1 is an OrBranchStart
                    if (andList.Count != 0)
                    {
                        //print a & b
                        var lastAndElem = andList.Last();
                        foreach (var e in andList)
                        {
                            if (!string.IsNullOrEmpty(o) && o.Last() != '&') o += '&';

                            o += e.Label;
                            if (!ReferenceEquals(e, lastAndElem))
                            {
                                o += "&";
                            }
                        }

                        andList.Clear();

                        o += "&";
                    }

                    goto nextElement;
                }

                case ElementType.OrBranchEnd:
                    if (isRecursiveOrBranchCall)
                    {
                        if (andList.Count != 0)
                        {
                            //print a & b
                            var lastAndElem = andList.Last();
                            foreach (var e in andList)
                            {
                                o += e.Label;
                                if (!ReferenceEquals(e, lastAndElem))
                                {
                                    o += "&";
                                }
                            }

                            andList.Clear();
                        }

                        if (nextBranchStart.TryPop(out var next))
                        {
                            var p = Parse(next.Row, next.Col, true);
                            return $"({o})||({p})";
                        }

                        return o;
                    }

                    break;
                case ElementType.OrBranchStart:
                {
                    // Check if there is a OBS below us, to make sure this isnt appart of a larger 
                    //or calling chain
                    var below = GetElement(r + 1, c);
                    if (below != null && below.ElementType == ElementType.OrBranchStart)
                    {
                        nextBranchStart.Push(below.LinePos);
                    }

                    goto nextElement;
                }

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
                            if (!string.IsNullOrEmpty(o) && o.Last() != '&') o += "&";

                            o += e.Label;
                            if (!ReferenceEquals(e, lastAndElement))
                            {
                                o += "&";
                            }
                        }
                    }

                    return o;

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