using System.Drawing;
using System.Numerics;
using Raylib_CsLo;
using rlImGui_cs;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RlGl;
using Rectangle = Raylib_CsLo.Rectangle;

namespace LdGraphicalDiagram;

internal static class Program
{
    internal const int GridWidthPx = 64, GridHeightPx = 64, SpritePaddingPx = 1, LabelFontSize = 32;
    private static Texture _sprites;

    public static int Main(string[] args)
    {
        Node l0X7No = new()
        {
            Attached =
            [
                new Node()
                {
                    Attached = [],
                    Kind = Node.NodeKind.No,
                    Label = "X8"
                },
                new Node()
                {
                    Attached =
                        [],
                    Kind = Node.NodeKind.Nc,
                    Label = "X9"
                }
            ],
            Label = "X7",
            Kind = Node.NodeKind.No
        };

        Node l0X2No = new()
        {
            Attached =
            [
                new Node()
                {
                    Attached = [l0X7No],
                    Kind = Node.NodeKind.No,
                    Label = "X4"
                },
                new Node()
                {
                    Attached = [l0X7No],
                    Kind = Node.NodeKind.Nc,
                    Label = "X6"
                }
            ],
            Label = "X2",
            Kind = Node.NodeKind.No
        };

        LineRootNode line0Root = new()
        {
            Attached =
            [
                new Node()
                {
                    Attached = [l0X2No],
                    Kind = Node.NodeKind.No,
                    Label = "X1"
                },
                new Node()
                {
                    Attached = [l0X2No],
                    Kind = Node.NodeKind.No,
                    Label = "X3"
                },
                new Node()
                {
                    Attached = [l0X2No],
                    Kind = Node.NodeKind.No,
                    Label = "X5"
                },
                new Node()
                {
                    Attached = [],
                    Kind = Node.NodeKind.No,
                    Label = "X10"
                },
                new Node()
                {
                    Attached = [l0X7No],
                    Kind = Node.NodeKind.No,
                    Label = "X11"
                },
            ]
        };

        //   X001           X003       O1
        // ---| |---+--------|\|--------( )---
        //   X002   |
        // ---| |---+
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
                },
                new()
                {
                    Attached = [l1Out],
                    Kind = Node.NodeKind.No,
                    Label = "X016"
                }
            ]
        };
        LineRootNode simple = new()
        {
            Attached =
            [
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
            ]
        };

        SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);

// RL Initialization
//--------------------------------------------------------------------------------------
        const int screenWidth = 800;
        const int screenHeight = 450;

        InitWindow(screenWidth, screenHeight, "LD");
        rlImGui.Setup(true); // sets up ImGui with ether a dark or light default theme


        Camera2D camera = new();
        camera.zoom = 1.0f;

        _sprites = LoadTexture("Assets/sc.png");
        if (_sprites.id == 0) throw new Exception("Could not load assets!");

        bool showGridLines = false;

        InteractiveLdBuilder interact = new();
        interact.LoadDiagram(simple);

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
                camera.offset.Y += wheel;
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

            bool ToolBarButton(int buttonLoc, string label) =>
                RayGui.GuiButton(new Rectangle(128 * buttonLoc, 0, 128, 32), label);

            void DrawLast()
            {
                ToolBarButton(0, @"-------");
                ToolBarButton(1, @"--| |--");
                ToolBarButton(2, @"--|\|--");
                ToolBarButton(3, @"--( )--");
                ToolBarButton(4, @"BRANCH");
                ToolBarButton(5, @"END BR");
            }

            //----------------------------------------------------------------------------------

            // Draw
            //----------------------------------------------------------------------------------
            Draw:
            BeginDrawing();
            rlImGui.Begin(); // starts the ImGui content mode. Make all ImGui calls after this
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


            foreach (var e in interact.LdElems)
            {
                DrawLdSpriteOnGrid(e.Key.Y, e.Key.X, e.Value.Label, e.Value.Kind);
            }


            SetMouseOffset(0, 0);
            EndMode2D();
            showGridLines = RayGui.GuiCheckBox(new Rectangle(0, 0, 32, 24), "Show Grid Lines", showGridLines);

            DrawText($"{interact.Selected}", 20, 0, 12, YELLOW);
            if (interact.IsPopupOpen) interact.DrawPopup();

            rlImGui.End(); // ends the ImGui content mode. Make all ImGui calls before this
            EndDrawing();
            //----------------------------------------------------------------------------------
        }

        rlImGui.Shutdown(); // cleans up ImGui

// De-Initialization
//--------------------------------------------------------------------------------------
        CloseWindow(); // Close window and OpenGL context
//--------------------------------------------------------------------------------------

        return 0;
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
        DrawTexturePro(_sprites, scLocation, destLocation, new(0, 0), 0, YELLOW);
    }

    private static void DrawPointerOnGrid(Point gp)
    {
        DrawRectangle(gp.X * GridWidthPx, gp.Y * GridHeightPx, GridWidthPx, GridHeightPx, BLUE);
    }
}