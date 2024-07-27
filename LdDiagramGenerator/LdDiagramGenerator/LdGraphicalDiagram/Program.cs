using LdGraphicalDiagram;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using static Raylib_CsLo.RayMath;
using static Raylib_CsLo.RlGl;
using System.Numerics;


LdNode L0X7NO = new()
{
    Attached =
    [
        new LdNode()
        {
            Attached = [],
            Kind = Node.NodeKind.No,
            Label = "X8"
        },
        new LdNode()
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

LdNode L0X2NO = new()
{
    Attached =
    [
        new LdNode()
        {
            Attached = [L0X7NO],
            Kind = Node.NodeKind.No,
            Label = "X4"
        },
        new LdNode()
        {
            Attached = [L0X7NO],
            Kind = Node.NodeKind.Nc,
            Label = "X6"
        }
    ],
    Label = "X2",
    Kind = Node.NodeKind.No
};

LineRootNode Line0Root = new()
{
    Outputs = ["O1"],
    Attached =
    [
        new LdNode()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X1"
        },
        new LdNode()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X3"
        },
        new LdNode()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X5"
        }
    ]
};


SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);

// RL Initialization
//--------------------------------------------------------------------------------------
const int screenWidth = 800;
const int screenHeight = 450;

InitWindow(screenWidth, screenHeight, "LD");

Camera2D camera = new();
camera.zoom = 1.0f;

int zoomMode = 0; // 0-Mouse Wheel, 1-Mouse Move

var sprites = LoadTexture("Assets/sc.png");
if (sprites.id == 0) throw new Exception("Could not load assets!");

int inBetweenSpriteSpace = 1;
int spriteSize = 64;

SetTargetFPS(60);
// Main game loop
while (!WindowShouldClose())
{
    // Update
    //----------------------------------------------------------------------------------
    if (IsKeyPressed(KeyboardKey.KEY_ONE)) zoomMode = 0;
    else if (IsKeyPressed(KeyboardKey.KEY_TWO)) zoomMode = 1;

    // Translate based on mouse right click
    if (IsMouseButtonDown(MouseButton.MOUSE_BUTTON_RIGHT) || IsKeyDown(KeyboardKey.KEY_LEFT_ALT))
    {
        Vector2 delta = GetMouseDelta();
        delta = Vector2Scale(delta, -1.0f / camera.zoom);
        camera.target = Vector2Add(camera.target, delta);
    }

    // if (zoomMode == 0)
    // {
    //     // Zoom based on mouse wheel
    //     float wheel = GetMouseWheelMove();
    //     if (wheel != 0)
    //     {
    //         // Get the world point that is under the mouse
    //         Vector2 mouseWorldPos = GetScreenToWorld2D(GetMousePosition(), camera);

    //         // Set the offset to where the mouse is
    //         camera.offset = GetMousePosition();

    //         // Set the target to match, so that the camera maps the world space point 
    //         // under the cursor to the screen space point under the cursor at any zoom
    //         camera.target = mouseWorldPos;

    //         // Zoom increment
    //         float scaleFactor = 1.0f + (0.25f * fabsf(wheel));
    //         if (wheel < 0) scaleFactor = 1.0f / scaleFactor;
    //         camera.zoom = Math.Clamp(camera.zoom * scaleFactor, 0.125f, 64.0f);
    //     }
    // }

    //----------------------------------------------------------------------------------

    // Draw
    //----------------------------------------------------------------------------------
    BeginDrawing();
    ClearBackground(BLACK);

    BeginMode2D(camera);

    // Draw the 3d grid, rotated 90 degrees and centered around 0,0 
    // just so we have something in the XY plane
    rlPushMatrix();
    rlTranslatef(0, 25 * 64, 0);
    rlRotatef(90, 1, 0, 0);
    DrawGrid(100, 64);
    rlPopMatrix();


    SetMouseOffset((int)camera.target.X, (int)camera.target.Y);
    var mp = GetMousePosition();
    DrawCircle((int)mp.X, (int)mp.Y, 5, RED);
    //GuiDrawIcon((int)GuiIconName.ICON_CURSOR_HAND, (int)mp.X, (int)mp.Y, 1, WHITE);

    for (var i = 1; i < 15; i++)
    {
        DrawSpriteOnGrid(i, 0, Sprite.DownWire);
        DrawSpriteOnGrid(i, 10, Sprite.DownWire);
    }

    DrawSpriteOnGrid(1, 0, Sprite.BranchStart);
    DrawSpriteOnGrid(1, 1, Sprite.Wire);

    DrawLdSpriteOnGrid(1, 2, "X01", Sprite.No);

    //DrawSpriteOnGrid(1, 2, Sprite.No);
    DrawSpriteOnGrid(1, 3, Sprite.OrBranch);

    DrawSpriteOnGrid(2, 3, Sprite.DownWire);

    DrawSpriteOnGrid(3, 3, Sprite.OrBranchStart);
    DrawLdSpriteOnGrid(3, 4, "X02", Sprite.No);
    DrawSpriteOnGrid(3, 5, Sprite.OrBranchEnd);

    DrawSpriteOnGrid(2, 5, Sprite.DownWire);

    DrawLdSpriteOnGrid(1, 4, "X03", Sprite.No);
    DrawSpriteOnGrid(1, 5, Sprite.OrBranch);
    DrawSpriteOnGrid(1, 6, Sprite.Wire);
    DrawSpriteOnGrid(1, 7, Sprite.Wire);
    DrawSpriteOnGrid(1, 8, Sprite.Wire);
    DrawLdSpriteOnGrid(1, 9, "X02", Sprite.Coil);
    DrawSpriteOnGrid(1, 10, Sprite.BranchEnd);


    SetMouseOffset(0, 0);
    EndMode2D();
    if (GuiButton(new Rectangle(0, 0, 128, 24), "Some Button"))
    {
    }

    EndDrawing();
    //----------------------------------------------------------------------------------
}

// De-Initialization
//--------------------------------------------------------------------------------------
CloseWindow(); // Close window and OpenGL context
//--------------------------------------------------------------------------------------
const int gridWidthPx = 64, gridHeightPx = 64, spritePaddingPx = 1, labelFontSize=32;
return 0;


void DrawLdSpriteOnGrid(int row, int col, string lbl, Sprite spriteidx)
{
    DrawTextOnGrid(row - 1, col, lbl);
    DrawSpriteOnGrid(row, col, spriteidx);
}

void DrawTextOnGrid(int row, int col, string text)
{
    int tlPx = gridWidthPx * col;
    int tlPy = gridHeightPx * row;

    DrawText(text, tlPx, tlPy, labelFontSize, WHITE);
}

void DrawSpriteOnGrid(int row, int col, Sprite spriteIdx)
{
    int tlPx = gridWidthPx * col;
    int tlPy = gridHeightPx * row;


    var scLocation = new Rectangle((int)spriteIdx * 64 + (spritePaddingPx * (int)spriteIdx), 0, 64, 64);
    var destLocation = new Rectangle(tlPx, tlPy, 64, 64);
    DrawTexturePro(sprites, scLocation, destLocation, new(0, 0), 0, YELLOW);
}

enum Sprite
{
    Wire,
    No,
    Nc,
    Coil,
    CoilSet,
    CoilReset,
    OrBranch,
    OrBranchStart,
    OrBranchEnd,
    DownWire,
    BranchStart,
    BranchEnd
}