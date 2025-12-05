using Godot;

namespace agame.World;

public partial class World : Node3D {
    public static World Instance { get; private set; }

    public override void _Ready() {
        Instance = this;
    }

    public override void _Process(double delta) {
        // TODO: move this somewhere else
        if (Input.IsActionJustPressed("escape")) {
            if (Input.MouseMode == Input.MouseModeEnum.Captured) {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
        }
    }

    public void PlaceGrowPlot(Vector3 buildPosition) {
        var scene = GD.Load<PackedScene>("res://assets/scenes/grow_plot.tscn");
        var node = scene.Instantiate<Node3D>();
        node.Position = buildPosition;
        AddChild(node);
    }
}