using Godot;

public partial class Dirtpatch : Node3D
{
    [Signal]
    public delegate void PlayerInRangeEventHandler(bool inRange);

    [Signal]
    public delegate void PlantFullyGrownEventHandler();

    enum DirtPatchState
    {
        Dry,
        Watered,
        YoungCactus,
        AgedCactus,
        CactusWithFlowers
    }

    [Export]
    MeshInstance3D groundDirtPatch;

    [Export]
    Area3D area3D;

    DirtPatchState dirtPatchState = DirtPatchState.Dry;

    MeshInstance3D cactusModel;

    bool playerInRange;

    public override void _Ready()
    {
        area3D.BodyEntered += OnBodyEntered;
        area3D.BodyExited += OnBodyExit;
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        GD.Print($"body has entered dirtpatch range! {body}");
        playerInRange = true;
        UiManager.Instance.InteractLabel.Visible = true;
        UiManager.Instance.InteractLabel.Text = GetTextForInteractLabel(dirtPatchState);
    }

    private void OnBodyExit(Node3D body)
    {
        if (!body.IsInGroup("player")) return;
        GD.Print("player has exited dirtpatch range!");
        playerInRange = false;
        UiManager.Instance.InteractLabel.Visible = false;
    }

    public override void _Process(double delta)
    {
        if (!playerInRange) return;

        if (Input.IsActionJustPressed("interact"))
        {
            switch (dirtPatchState)
            {
                case DirtPatchState.Dry:
                    dirtPatchState = DirtPatchState.Watered;
                    UiManager.Instance.InteractLabel.Text = "Press (F) to make it YoungCactus";

                    StandardMaterial3D newMaterial = new StandardMaterial3D();
                    newMaterial.AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/wet_dirt.png");

                    groundDirtPatch.Visible = true;
                    groundDirtPatch.SetSurfaceOverrideMaterial(0, newMaterial);
                    break;
                case DirtPatchState.Watered:
                    dirtPatchState = DirtPatchState.YoungCactus;
                    if (cactusModel == null)
                    {
                        PackedScene scene = GD.Load<PackedScene>("res://assets/scenes/cactus.tscn");
                        MeshInstance3D instance = scene.Instantiate<MeshInstance3D>();
                        instance.Position = new Vector3(0.0f, 0.5f, 0.0f);
                        AddChild(instance);
                        cactusModel = instance;
                    }
                    UiManager.Instance.InteractLabel.Text = "Press (F) to make it AgedCactus";
                    break;
                case DirtPatchState.YoungCactus:
                    dirtPatchState = DirtPatchState.AgedCactus;
                    string cactusModelPath = GetPathForCactus(dirtPatchState);
                    cactusModel.Mesh = (Mesh)GD.Load(cactusModelPath);
                    UiManager.Instance.InteractLabel.Text = "Press (F) to make it Cactus Flower";
                    break;
                case DirtPatchState.AgedCactus:
                    dirtPatchState = DirtPatchState.CactusWithFlowers;
                    cactusModelPath = GetPathForCactus(dirtPatchState);
                    cactusModel.Mesh = GD.Load<Mesh>(cactusModelPath);
                    UiManager.Instance.InteractLabel.Text = "Congrats on your first grown cactus! Now sell it to the NPC";
                    EmitSignal(SignalName.PlantFullyGrown);
                    GameManager.Instance.UpdateObjective(GameManager.GameObjective.SellFirstPlant);

                    break;
            }
        }
    }

    private string GetPathForCactus(DirtPatchState cactusState)
    {
        switch (cactusState)
        {
            case DirtPatchState.YoungCactus:
                return "res://assets/models/nature/cactus/Cactus_3.obj";
            case DirtPatchState.AgedCactus:
                return "res://assets/models/nature/cactus/Cactus_2.obj";
            case DirtPatchState.CactusWithFlowers:
                return "res://assets/models/nature/cactus/CactusFlowers_2.obj";
            default: return "";
        }
    }

    private string GetTextForInteractLabel(DirtPatchState dirtPatchState)
    {
        switch (dirtPatchState)
        {
            case DirtPatchState.Dry: return "Press (F) to water this dirt patch";
            case DirtPatchState.Watered: return "Press (F) to make it YoungCactus";
            case DirtPatchState.YoungCactus: return "Press (F) to make it AgedCactus";
            case DirtPatchState.AgedCactus: return "Press (F) to make it CactusFlowers";
            case DirtPatchState.CactusWithFlowers: return "Congrats on your first grown cactus! Now sell it to the NPC";
        }
        return "";
    }

}
