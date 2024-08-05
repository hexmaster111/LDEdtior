using System.Drawing;
using static Raylib_CsLo.Raylib;

namespace LdGraphicalDiagram;

public class TankSimulator
{
    public readonly Dictionary<string, bool> IO = new()
    {
        { "HL", false },
        { "LL", false },
        { "FILL", false }
    };

    public float Level = 0;

    public void Update()
    {
        if (Level > 0) Level -= 0.01f;

        if (IO["FILL"])
        {
            Level += .03f;
            if (Level >= 1f) Level = 1f;
        }

        IO["HL"] = Level >= .99f;
        IO["LL"] = Level <= .01f;
    }

    internal void Draw(Point point)
    {
        DrawText("TANK", point.X, point.Y, 20, WHITE);
        DrawRectangle(point.X, point.Y - (int)(Level * 100), 64, (int)(Level * 100), BLUE);
    }
}