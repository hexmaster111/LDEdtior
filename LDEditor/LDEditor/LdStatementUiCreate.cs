using System.Collections;
using System.Diagnostics;
using LdLib.Types;

namespace LDEditor;

internal enum LineType
{
    no,
    orbr,
    endbr,
    set
}

internal class StatementLine
{
    public LineType Type;
    public string Name = null!;

    //For the factory enforcement agency
    private StatementLine()
    {
    }

    public static StatementLine? FromLine(string line)
    {
        var parts = line.Split(' ');

        return Enum.TryParse<LineType>(parts[0], true, out var s)
            ? new StatementLine()
            {
                Type = s,
                Name = parts.Length > 1 ? parts[1] : string.Empty
            }
            : null;
    }
}

public class LdStatementUiCreate
{
    public const int LdDiagramWidth = 9;

    public LdLine ParseLineFromStatement(string ldList)
    {
        Stack<RowCol> _branchPoses = new();

        var lines = ldList.Split(Environment.NewLine, StringSplitOptions.TrimEntries);
        var slines = lines.Select(StatementLine.FromLine).ToList();
        if (slines.Any(x => x == null))
        {
            var l = slines.First(x => x == null);
            var i = slines.IndexOf(l);
            throw new LdException(i,
                $"Invalid Line! expected {Environment.NewLine}" +
                $"{string.Join(Environment.NewLine, Enum.GetNames<LineType>())}");
        }

        var ret = new LdLine();

        var pos = new RowCol(0, 0);

        foreach (var line in slines)
        {
            Debug.Assert(line != null, nameof(line) + " != null");
            switch (line.Type)
            {
                case LineType.no:
                    ret.Elements.Add(new LdElement()
                    {
                        ElementType = ElementType.NormallyOpenContact,
                        Label = line.Name,
                        LinePos = pos,
                    });
                    NextCol();
                    break;
                case LineType.orbr:
                    OrNextRow();
                    break;
                case LineType.endbr:
                    OrBranchDone(line);
                    break;
                case LineType.set:
                {
                    ret.Elements.Add(new LdElement()
                    {
                        ElementType = ElementType.Coil,
                        Label = line.Name,
                        LinePos = pos,
                    });
                    NextCol();
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        return ret;


        void OrBranchDone(StatementLine statementLine)
        {
            ret.Elements.Add(new LdElement()
            {
                ElementType = ElementType.OrBranchEnd,
                LinePos = pos
            });

            pos.Row -= 1;
            ret.Elements.Add(new LdElement()
            {
                ElementType = ElementType.OrWire,
                LinePos = pos
            });
            pos.Col += 1;
        }

        void OrNextRow()
        {
            pos.Row += 1;

            pos.Col = 0; //todo nono
        }


        void NextCol()
        {
            pos.Col += 1;
        }
    }
}

public class LdException : Exception
{
    public int Line;

    public LdException(int line, string what) : base(what)
    {
        Line = line;
    }
}