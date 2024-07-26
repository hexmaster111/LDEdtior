using System.Numerics;

using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using static Raylib_CsLo.RayMath;
using static Raylib_CsLo.RlGl;

public class LdNode : Node
{
    public float X,Y;
    

    internal void Draw()
    {   
        GuiButton(new(X,Y,48,18), $"{Label}");
    }
}