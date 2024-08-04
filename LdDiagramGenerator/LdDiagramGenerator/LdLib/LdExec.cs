using LdLib;

namespace LdExecuter;

public class LdExec
{
    private LdDocument _document = null!;
    public readonly Dictionary<string, bool> IOState = new();
    public void Load(LdDocument document)
    {
        _document = document;
        foreach (var lbl in document.GetAllDistinctNodeLabels())
        {
            IOState[lbl] = false;
        }
    }

    public void DoCycle()
    {
        foreach (var line in _document.Lines)
        {
            ExecuteLine(line);
        }
    }


    bool Examine(Node n)
    {
        bool examine = n.Kind switch
        {
            Node.NodeKind.No => IOState[n.Label],
            Node.NodeKind.Nc => !IOState[n.Label],
            Node.NodeKind.Coil => true,
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
            IOState[os.Label] = outputStateToSet;
        }
    }
}
