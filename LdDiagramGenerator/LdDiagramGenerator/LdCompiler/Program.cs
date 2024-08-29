//compiles latter diagrams into TinyVm IL

using LdCompiler;
using LdLib;

Node output = new()
{
    Label = "X0",
    Kind = Node.NodeKind.Coil,
    Attached = []
};

LineRootNode lr = new(
    [
        new()
        {
            Label = "X1",
            Kind = Node.NodeKind.No,
            Attached =
            [
                new()
                {
                    Label = "X2",
                    Attached = [output]
                },
                new()
                {
                    Label = "X0",
                    Attached = [output]
                }
            ]
        }
    ]
);

var line = LdLineComp.Compile(lr, new SimpleLookup());

return;

class SimpleLookup : ILabelToPinLookup
{
    private Dictionary<int, string> Map = new()
    {
        { 0, "X0" },
        { 1, "X1" },
        { 2, "X2" }
    };

    public int ToPin(string lbl) => Map.FirstOrDefault(x => x.Value == lbl).Key;
    public string ToLbl(int pin) => Map[pin];
}