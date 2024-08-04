using LdLib;

namespace Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }
    
    

    [Test]
    public void SaveAndLoadIntoSaveSaveObjectString()
    {
        Node l1Out = new Node()
        {
            Kind = Node.NodeKind.Coil,
            Attached = [],
            Label = "O0"
        };

        Node l1X003 = new Node()
        {
            Label = "X003",
            Kind = Node.NodeKind.Nc,
            Attached =
            [
                new()
                {
                    Attached = [l1Out],
                    Kind = Node.NodeKind.Nc,
                    Label = "X005"
                },
                new()
                {
                    Attached = [l1Out],
                    Kind = Node.NodeKind.No,
                    Label = "X006"
                }
            ]
        };
        LineRootNode simple = new([
            new Node
            {
                Attached = [l1X003],
                Kind = Node.NodeKind.No,
                Label = "X001"
            },
            new Node()
            {
                Attached = [l1X003],
                Kind = Node.NodeKind.No,
                Label = "X002"
            },
            new Node()
            {
                Kind = Node.NodeKind.No,
                Label = "X004",
                Attached = [l1Out]
            }
        ]);


        string s = simple.SaveString();
        
        LineRootNode loaded = LineRootNode.Load(s);
        string other = loaded.SaveString();
        
        Assert.That(other.SequenceEqual(s));
        Assert.Pass();
    }
}