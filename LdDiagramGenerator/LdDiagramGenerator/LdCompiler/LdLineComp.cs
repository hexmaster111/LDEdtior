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
            l.Add(VmOpp.Read(pinLookup.ToPin(n.Label)));
            orc += 1;
        }

        l.Add(VmOpp.Or(orc));

        if (andC != 0 && orc != 0)
        {
            
        }


        foreach (var n in ns)
        {
            CompileNodes(n.Attached, l, pinLookup);
        }
    }

    public static List<VmOpp> Compile(LineRootNode root, ILabelToPinLookup pinLookup)
    {
        var ret = new List<VmOpp>();

        CompileNodes(root.Attached, ret, pinLookup);


        return ret;
    }
}