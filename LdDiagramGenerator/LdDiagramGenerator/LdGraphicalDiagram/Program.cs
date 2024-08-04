using System.Drawing;
using System.Numerics;
using LdExecuter;
using LdLib;
using Newtonsoft.Json;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RlGl;
using Rectangle = Raylib_CsLo.Rectangle;

namespace LdGraphicalDiagram;

public class LdSimulator
{

    public LdExec ldExe = new();

    public void Draw(Point pos)
    {
        DrawText("Output Table", pos.X, pos.Y, 24, WHITE);
        int y = pos.Y + 25;
        int yStart = y;

        foreach (var item in ldExe.IOState)
        {
            DrawText($"{item.Key}", pos.X, y, 15, WHITE);
            y += 17;
        }

        y = yStart;
        foreach (var item in ldExe.IOState)
        {
            DrawText($"{item.Value}", pos.X+100, y, 15, WHITE);
            y += 17;
        }

    }
}



internal static class Program
{
    internal const int GridWidthPx = 64, GridHeightPx = 64, SpritePaddingPx = 1, LabelFontSize = 32;
    private static Texture _sprites;

    public static int Main(string[] args)
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

        LineRootNode easy = new([
            new Node()
            {
                Label = "1st",
                Kind = Node.NodeKind.No,
                Attached =
                [
                    new Node()
                    {
                        Label = "2nd",
                        Kind = Node.NodeKind.No,
                        Attached =
                        [
                            new Node()
                            {
                                Label = "3rd",
                                Kind = Node.NodeKind.Coil,
                                Attached = []
                            }
                        ]
                    }
                ]
            }
        ]);


        LineRootNode blinker = new([
            new Node(){
                Label ="BK0"
            }
        ]);

        Node Out1 = new()
        {
            Kind = Node.NodeKind.Coil,
            Label = "O1",
            Attached = []
        };

        Node Out2 = new()
        {
            Kind = Node.NodeKind.Coil,
            Label = "O2",
            Attached = []
        };
        Node L0X2NO = new()
        {
            Attached =
            [
                new Node()
                {
                    Attached =
                    [
                        new Node()
                        {
                            Attached = [Out1, Out2],
                            Kind = Node.NodeKind.Nc,
                            Label = "M001"
                        }
                    ],
                    Kind = Node.NodeKind.No,
                    Label = "X4"
                },
                new Node()
                {
                    Attached = [Out1, Out2],
                    Kind = Node.NodeKind.No,
                    Label = "X6"
                }
            ],
            Label = "X2",
            Kind = Node.NodeKind.No
        };

        LineRootNode Line0Root = new([
            new()
            {
                Attached = [L0X2NO],
                Kind = Node.NodeKind.No,
                Label = "X1"
            },
            new()
            {
                Attached = [L0X2NO],
                Kind = Node.NodeKind.No,
                Label = "X3"
            },
            new()
            {
                Attached = [L0X2NO],
                Kind = Node.NodeKind.No,
                Label = "X5"
            }
        ]);

        SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);

        // RL Initialization
        //--------------------------------------------------------------------------------------
        const int screenWidth = 800;
        const int screenHeight = 450;

        InitWindow(screenWidth, screenHeight, "LD");


        Camera2D camera = new();
        camera.zoom = 1.0f;

        _sprites = LoadTexture("Assets/sc.png");
        if (_sprites.id == 0) throw new Exception("Could not load assets!");

        bool showGridLines = false;

        InteractiveLdBuilder interact = new();
        LdSimulator simulator = new();
        var document = new LdDocument([
            Line0Root,
            simple,
            easy
        ]);

        interact.LoadDocument(document);
        simulator.ldExe.Load(document);


        SetTargetFPS(60);
        SetExitKey(0);
        //HideCursor();
        // Main game loop
        while (!WindowShouldClose())
        {
            if (interact.IsPopupOpen) goto Draw;
            // Update
            //----------------------------------------------------------------------------------

            // Translate based on mouse right click
            if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) || IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
            {
                Vector2 delta = GetMouseDelta();
                delta = RayMath.Vector2Scale(delta, -1.0f / camera.zoom);
                camera.target = RayMath.Vector2Add(camera.target, delta);
            }

            // scrolling
            float wheel = GetMouseWheelMove();
            if (wheel != 0)
            {
                camera.offset.Y += wheel * 20f;
            }

            if (IsKeyPressed(KeyboardKey.KEY_LEFT)) interact.SelectLeft();
            if (IsKeyPressed(KeyboardKey.KEY_RIGHT)) interact.SelectRight();
            if (IsKeyPressed(KeyboardKey.KEY_UP)) interact.SelectUp();
            if (IsKeyPressed(KeyboardKey.KEY_DOWN)) interact.SelectDown();

            if (IsKeyPressed(KeyboardKey.KEY_BACKSPACE)) interact.DeleteItem();
            if (IsKeyPressed(KeyboardKey.KEY_P)) interact.EditItemProperties();

            if (IsKeyPressed(KeyboardKey.KEY_ONE)) interact.PlaceItem(Sprite.Wire, "");
            if (IsKeyPressed(KeyboardKey.KEY_TWO)) interact.PlaceItem(Sprite.No, "LBL");
            if (IsKeyPressed(KeyboardKey.KEY_THREE)) interact.PlaceItem(Sprite.Nc, "LBL");
            if (IsKeyPressed(KeyboardKey.KEY_FOUR)) interact.PlaceItem(Sprite.Coil, "LBL");
            if (IsKeyPressed(KeyboardKey.KEY_FIVE)) interact.PlaceItem(Sprite.OrBranch, "");
            if (IsKeyPressed(KeyboardKey.KEY_SIX)) interact.PlaceItem(Sprite.BranchStart, "");
            if (IsKeyPressed(KeyboardKey.KEY_SEVEN)) interact.PlaceItem(Sprite.DownWire, "");
            if (IsKeyPressed(KeyboardKey.KEY_EIGHT)) interact.PlaceItem(Sprite.OrBranchEnd, "");
            if (IsKeyPressed(KeyboardKey.KEY_NINE)) interact.PlaceItem(Sprite.OrBranchStart, "");
            if (IsKeyPressed(KeyboardKey.KEY_ZERO)) interact.PlaceItem(Sprite.BranchEnd, "");

            if(IsKeyPressed(KeyboardKey.KEY_S)) simulator.ldExe.DoCycle();


            //----------------------------------------------------------------------------------

            // Draw
            //----------------------------------------------------------------------------------
            Draw:
            BeginDrawing();
            ClearBackground(BLACK);
            BeginMode2D(camera);

            if (showGridLines)
            {
                // Draw the 3d grid, rotated 90 degrees and centered around 0,0 
                // just so we have something in the XY plane
                rlPushMatrix();
                rlTranslatef(0, 25 * 64, 0);
                rlRotatef(90, 1, 0, 0);
                DrawGrid(100, 64);
                rlPopMatrix();
            }


            SetMouseOffset((int)camera.target.X, (int)camera.target.Y);
            var mp = GetMousePosition();
            //DrawCircle((int)mp.X, (int)mp.Y, 5, RED);
            //GuiDrawIcon((int)GuiIconName.ICON_CURSOR_HAND, (int)mp.X, (int)mp.Y, 1, WHITE);

            DrawPointerOnGrid(interact.Selected);

            foreach (var wire in interact.Wires)
            {
                DrawLdWireOnGrid(wire);
            }


            foreach (var e in interact.LdElems)
            {
                DrawLdSpriteOnGrid(e.Key.Y, e.Key.X, e.Value.Label, e.Value.Kind);
            }

            foreach (var ln in interact.LineNumbers)
            {
                DrawTextOnGrid(ln.GirdPos.Y, ln.GirdPos.X, ln.LineNo.ToString("0000"));
            }

            simulator.Draw(new Point(700, 0));

            SetMouseOffset(0, 0);
            EndMode2D();
            showGridLines = RayGui.GuiCheckBox(new Rectangle(0, 0, 32, 24), "Show Grid Lines", showGridLines);

            DrawText($"{interact.Selected}", 20, 0, 12, YELLOW);
            if (interact.IsPopupOpen) interact.DrawPopup();

            EndDrawing();
            //----------------------------------------------------------------------------------
        }


        // De-Initialization
        //--------------------------------------------------------------------------------------
        CloseWindow(); // Close window and OpenGL context
        //--------------------------------------------------------------------------------------

        return 0;
    }

    private static void DrawLdWireOnGrid(InteractiveLdBuilder.WireT wire)
    {
        for (int i = 0; i < wire.Points.Length - 1; i++)
        {
            var currentPt = wire.Points[i];
            var nextPt = wire.Points[i + 1];
            DrawWire(currentPt, nextPt);
        }
    }

    private static void DrawWire(Vector2 gridPoint1, Vector2 gridPoint2)
    {
        const int wireThickness = InteractiveLdBuilder.WireT.Thickness;
        Vector2 screenPos1 = new() { X = gridPoint1.X * GridWidthPx, Y = gridPoint1.Y * GridHeightPx };
        Vector2 screenPos2 = new() { X = gridPoint2.X * GridWidthPx, Y = gridPoint2.Y * GridHeightPx };

        screenPos1.Y += .5f * GridHeightPx;
        screenPos2.Y += .5f * GridHeightPx;

        DrawLineEx(screenPos1, screenPos2, wireThickness, YELLOW);
    }

    private static void DrawLdSpriteOnGrid(int row, int col, string lbl, Sprite spriteidx)
    {
        DrawTextOnGrid(row - 1, col, lbl);
        DrawSpriteOnGrid(row, col, spriteidx);
    }

    private static void DrawTextOnGrid(int row, int col, string text)
    {
        int tlPx = GridWidthPx * col;
        int tlPy = (GridHeightPx * row) + LabelFontSize;

        DrawText(text, tlPx, tlPy, LabelFontSize, WHITE);
    }

    private static void DrawSpriteOnGrid(int row, int col, Sprite spriteIdx)
    {
        int tlPx = GridWidthPx * col;
        int tlPy = GridHeightPx * row;


        var scLocation = new Rectangle((int)spriteIdx * 64 + (SpritePaddingPx * (int)spriteIdx), 0, 64, 64);
        var destLocation = new Rectangle(tlPx, tlPy, 64, 64);
        //DrawRectangleRec(destLocation, BLACK);
        DrawTexturePro(_sprites, scLocation, destLocation, new(0, 0), 0, YELLOW);
    }

    private static void DrawPointerOnGrid(Point gp)
    {
        DrawRectangle(gp.X * GridWidthPx, gp.Y * GridHeightPx, GridWidthPx, GridHeightPx, BLUE);
    }
}