using LdLib;

namespace LdCompiler;

public interface ILabelToPinLookup
{
    int ToPin(string lbl);
    string ToLbl(int pin);
}

public static class LdLineComp
{
    private static void CompileNodes(Node[] ns, List<VmOpp> l, ILabelToPinLookup pinLookup, int andC = 0)
    {
        int orc = 0;
        foreach (var n in ns)
        {
            if (n.Kind.IsOutput()) l.Add(VmOpp.Set(pinLookup.ToPin(n.Label)));
            else l.Add(VmOpp.Read(pinLookup.ToPin(n.Label)));
            orc += 1;
        }

        if (ns.Length > 1)
        {
            l.Add(VmOpp.Or(orc));
        }
        else andC = andC + 1;

        foreach (var n in ns)
        {
            CompileNodes(n.Attached, l, pinLookup, andC);
        }
    }

    public static List<VmOpp> Compile(LineRootNode root, ILabelToPinLookup pinLookup)
    {
        var ret = new List<VmOpp>();

        CompileNodes(root.Attached, ret, pinLookup);


        return ret;
    }
}