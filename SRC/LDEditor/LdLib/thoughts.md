a & (  b | c | ( d & e) | (f & ( g | h) )  ) 
```csharp
public enum Operator
{
    And,
    Or
}

public class Node
{
    public Operator Operator { get; set; }
    public Node Left { get; set; }
    public Node Right { get; set; }
    public string Value { get; set; }
}

public class Expression
{
    public Node Root { get; set; }
}

var root = new Node
{
    Operator = Operator.And,
    Left = new Node
    {
        Value = "a"
    },
    Right = new Node
    {
        Operator = Operator.Or,
        Left = new Node
        {
            Value = "b"
        },
        Right = new Node
        {
            Operator = Operator.Or,
            Left = new Node
            {
                Operator = Operator.And,
                Left = new Node
                {
                    Value = "d"
                },
                Right = new Node
                {
                    Value = "e"
                }
            },
            Right = new Node
            {
                Operator = Operator.And,
                Left = new Node
                {
                    Value = "f"
                },
                Right = new Node
                {
                    Operator = Operator.Or,
                    Left = new Node
                    {
                        Value = "g"
                    },
                    Right = new Node
                    {
                        Value = "h"
                    }
                }
            }
        }
    }
};


```
