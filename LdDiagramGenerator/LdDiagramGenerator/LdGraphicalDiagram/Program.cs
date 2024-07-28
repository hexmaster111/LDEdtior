using System.Collections.ObjectModel;
using System.Drawing;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;
using static Raylib_CsLo.RayMath;
using static Raylib_CsLo.RlGl;
using System.Numerics;
using Rectangle = Raylib_CsLo.Rectangle;


Node L0X7NO = new()
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

Node L0X2NO = new()
{
    Attached =
    [
        new Node()
        {
            Attached = [L0X7NO],
            Kind = Node.NodeKind.No,
            Label = "X4"
        },
        new Node()
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
        new Node()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X1"
        },
        new Node()
        {
            Attached = [L0X2NO],
            Kind = Node.NodeKind.No,
            Label = "X3"
        },
        new Node()
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
InteractiveLdBuilder interact = new();
SetTargetFPS(60);
//HideCursor();
// Main game loop
while (!WindowShouldClose())
{
    if (interact.IsPopupOpen) goto Draw;
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
    if (IsKeyPressed(KeyboardKey.KEY_ONE)) interact.PlaceItem(Sprite.Wire, "");
    if (IsKeyPressed(KeyboardKey.KEY_TWO)) interact.PlaceItem(Sprite.No, "LBL");
    if (IsKeyPressed(KeyboardKey.KEY_P)) interact.EditItemProperties();

    //----------------------------------------------------------------------------------

    // Draw
    //----------------------------------------------------------------------------------
    Draw:
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
    //DrawCircle((int)mp.X, (int)mp.Y, 5, RED);
    //GuiDrawIcon((int)GuiIconName.ICON_CURSOR_HAND, (int)mp.X, (int)mp.Y, 1, WHITE);
    DrawPointerOnGrid(interact.Selected);

    for (var i = 1; i < 15; i++)
    {
        DrawSpriteOnGrid(i, 0, Sprite.DownWire);
        DrawSpriteOnGrid(i, 10, Sprite.DownWire);
    }

    foreach (var e in interact.LdElems)
    {
        DrawLdSpriteOnGrid(e.Key.Y, e.Key.X, e.Value.Label, e.Value.Kind);
    }

    //
    // DrawSpriteOnGrid(1, 0, Sprite.BranchStart);
    // DrawSpriteOnGrid(1, 1, Sprite.Wire);
    //
    // DrawLdSpriteOnGrid(1, 2, "X01", Sprite.No);
    //
    // DrawSpriteOnGrid(1, 3, Sprite.OrBranch);
    // DrawSpriteOnGrid(2, 3, Sprite.DownWire);
    //
    // DrawSpriteOnGrid(3, 3, Sprite.OrBranchStart);
    // DrawLdSpriteOnGrid(3, 4, "X02", Sprite.No);
    // DrawSpriteOnGrid(3, 5, Sprite.OrBranchEnd);
    //
    // DrawSpriteOnGrid(2, 5, Sprite.DownWire);
    //
    // DrawLdSpriteOnGrid(1, 4, "X03", Sprite.No);
    // DrawSpriteOnGrid(1, 5, Sprite.OrBranch);
    // DrawSpriteOnGrid(1, 6, Sprite.Wire);
    // DrawSpriteOnGrid(1, 7, Sprite.Wire);
    // DrawSpriteOnGrid(1, 8, Sprite.Wire);
    // DrawLdSpriteOnGrid(1, 9, "X02", Sprite.Coil);
    // DrawSpriteOnGrid(1, 10, Sprite.BranchEnd);

    SetMouseOffset(0, 0);
    EndMode2D();
    if (GuiButton(new Rectangle(0, 0, 128, 24), "Some Button"))
    {
    }

    if (interact.IsPopupOpen) interact.DrawPopup();

    EndDrawing();
    //----------------------------------------------------------------------------------
}

// De-Initialization
//--------------------------------------------------------------------------------------
CloseWindow(); // Close window and OpenGL context
//--------------------------------------------------------------------------------------
const int gridWidthPx = 64, gridHeightPx = 64, spritePaddingPx = 1, labelFontSize = 32;

return 0;

void DrawPointerOnGrid(Point gp)
{
    DrawRectangle(gp.X * gridWidthPx, gp.Y * gridHeightPx, gridWidthPx, gridHeightPx, BLUE);
}


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

public class InteractiveLdBuilder
{
    public Point Selected { get; private set; }
    public bool IsPopupOpen => _openPopup != PopupKind.Nothing;

    public readonly Dictionary<Point, LdElem> LdElems = new();
    private PopupKind _openPopup = PopupKind.Nothing;

    public struct LdElem
    {
        //I think here is where the node would end up going
        public Sprite Kind;
        public string Label;
    }

    private void ContactPropertiesUpdate()
    {
    }

    private void DrawContactProperties()
    {
        DrawRectangle(50, 50, 512, 128, RAYWHITE);
    }

    public void DrawPopup()
    {
        switch (_openPopup)
        {
            default:
            case PopupKind.Nothing:
                break;
            case PopupKind.NoContactProperties:
                ContactPropertiesUpdate();
                DrawContactProperties();
                break;
        }
    }

    public void PlaceItem(Sprite element, string label)
    {
        LdElems[Selected] = new LdElem()
        {
            Kind = element,
            Label = label
        };
    }

    public void DeleteItem()
    {
        LdElems.Remove(Selected);
    }

    public void EditItemProperties()
    {
        if (!LdElems.TryGetValue(Selected, out var item)) return;
        _openPopup = item.Kind switch
        {
            Sprite.No => PopupKind.NoContactProperties,
            _ => PopupKind.Nothing
        };
    }

    public void SelectLeft()
    {
        Selected = Selected with
        {
            X = Selected.X - 1
        };
    }

    public void SelectRight()
    {
        Selected = Selected with
        {
            X = Selected.X + 1
        };
    }

    public void SelectUp()
    {
        Selected = Selected with
        {
            Y = Selected.Y - 1
        };
    }

    public void SelectDown()
    {
        Selected = Selected with
        {
            Y = Selected.Y + 1
        };
    }

    private enum PopupKind
    {
        Nothing,
        NoContactProperties
    }
}

public enum Sprite
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