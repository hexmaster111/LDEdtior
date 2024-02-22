using LdLib.Types;

namespace LdLib.CompilerTypes;

public class Node
{

    public Node(Node? leftNode, LdElement? leftElement, Operator op, Node? rightNode, LdElement? rightElement)
    {
        
    }

    public Node()
    {
        
    }

    public Operator Operator { get; set; }
    public Dynamic<Node, LdElement> Left { get; set; }
    public Dynamic<Node, LdElement> Right { get; set; }


    public override string ToString()
    {
        var l = Left.TheOne().ToString();
        var r = Right.TheOne().ToString();
        return $"{l} {Operator} {r}";
    }
}