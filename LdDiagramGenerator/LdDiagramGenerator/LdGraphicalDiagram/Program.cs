using LdGraphicalDiagram;
using Raylib_CsLo;
using static Raylib_CsLo.Raylib;
using static Raylib_CsLo.RayGui;


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
    ]
};

List<LdNode> _nodes = new();


SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
InitWindow(800, 480, "Hello World");
SetTargetFPS(30);

bool showMessageBox = false;

while (!WindowShouldClose())
{
    BeginDrawing();
    ClearBackground(WHITE);

    LdGraphics.RenderNodeGraph(Line0Root);

    var mousePosition = GetMousePosition();
    var displayWidth = GetScreenWidth();
    var displayHeight = GetScreenHeight();

    DrawText($"{mousePosition}", 5, 5, 18, BLACK);
    DrawText($"{displayWidth} {displayHeight}", 5, 23, 18, BLACK);


    EndDrawing();
}

CloseWindow();

public class LdNode : Node
{
    public int X, Y, Width, Height, InX, InY, OutX, OutY;
}