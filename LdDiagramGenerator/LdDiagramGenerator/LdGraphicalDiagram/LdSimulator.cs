using System.Drawing;
using LdExecuter;
using static Raylib_CsLo.RayGui;
using static Raylib_CsLo.Raylib;
using Rectangle = Raylib_CsLo.Rectangle;

namespace LdGraphicalDiagram;

public class LdSimulator
{
    public readonly LdExec LdExe = new();

    private void DrawOutputTable(Point pos)
    {
        DrawText("IO Table", pos.X, pos.Y, 24, WHITE);
        int y = pos.Y + 25;
        int yStart = y;

        foreach (var item in LdExe.IOState)
        {
            DrawText($"{item.Key}", pos.X, y, 15, WHITE);
            y += 17;
        }

        y = yStart;
        foreach (var item in LdExe.IOState)
        {
            DrawText($"{item.Value}", pos.X + 100, y, 15, WHITE);
            y += 17;
        }

        y = yStart;
        foreach (var item in LdExe.IOState)
        {
            if (GuiButton(new Rectangle(pos.X + 150, y, 50, 15), "T"))
            {
                LdExe.IOState[item.Key] = !LdExe.IOState[item.Key];
            }

            y += 17;
        }
    }

    public void Draw(Point pos)
    {
        DrawOutputTable(pos);
    }
}