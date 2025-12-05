using agame.Items;
using Godot;

namespace agame.World;

public partial class GrowPlot : Node3D {
    [Signal]
    public delegate void PlayerInRangeEventHandler(bool inRange);

    [Signal]
    public delegate void PlantFullyGrownEventHandler();

    enum GrowPlotState {
        Dry,
        Watered,
        HasPlant
    }

    public enum PlantType {
        Cactus,
    }

    enum PlantState {
        YoungPlant,
        AgedPlant,
        ReadyToHarvest,
    }

    [Export]
    MeshInstance3D _dirtPatchMesh;
    [Export]
    Area3D area3D;

    Node3D plantModel;

    PlantState plantState;
    GrowPlotState growPlotState = GrowPlotState.Dry;

    bool playerInRange;

    public override void _Ready() {
        area3D.BodyEntered += OnBodyEntered;
        area3D.BodyExited += OnBodyExit;
        _dirtPatchMesh = GetNode<MeshInstance3D>("Ground");
    }

    public override void _Process(double delta) {
        if (!playerInRange) return;
        HandleInteractionWithGrowPlot();
    }

    private void OnBodyEntered(Node3D body) {
        if (!body.IsInGroup("player")) return;
        GD.Print("player has entered dirtpatch range!");
        playerInRange = true;
        UiManager.Instance.InteractLabel.Visible = true;
        GD.Print("made interact label visible");
        UiManager.Instance.InteractLabel.Text = GetTextForInteractLabel(growPlotState);
    }

    private void OnBodyExit(Node3D body) {
        if (!body.IsInGroup("player")) return;
        GD.Print("player has exited dirtpatch range!");
        playerInRange = false;
        UiManager.Instance.InteractLabel.Visible = false;
    }

    private void HandleInteractionWithGrowPlot() {
        if (!Input.IsActionJustPressed("interact")) return;

        // for now we always plant a cactus
        switch (growPlotState) {
            case GrowPlotState.Dry:
                growPlotState = GrowPlotState.Watered;

                StandardMaterial3D newMaterial = new() {
                    AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/wet_dirt.png")
                };
                _dirtPatchMesh.SetSurfaceOverrideMaterial(0, newMaterial);

                UiManager.Instance.InteractLabel.Text = "Press (F) to plant a Young Cactus";

                return;
            case GrowPlotState.Watered:
                growPlotState = GrowPlotState.HasPlant;
                plantState = PlantState.YoungPlant;

                string plantModelPath = GetModelPathForCactusByPlantState(plantState);
                UpdatePlantModel(plantModelPath);

                string interactLabelText = "Press (F) to make this Young Plant an Aged Plant";
                UiManager.Instance.InteractLabel.Text = interactLabelText;

                return;
        }

        if (growPlotState != GrowPlotState.HasPlant) return;

        switch (plantState) {
            case PlantState.YoungPlant: {
                    plantState = PlantState.AgedPlant;

                    string plantModelPath = GetModelPathForCactusByPlantState(plantState);
                    UpdatePlantModel(plantModelPath);

                    UiManager.Instance.InteractLabel.Text = "Press (F) to make this plant ready to harvest";
                    break;
                }
            case PlantState.AgedPlant: {
                    plantState = PlantState.ReadyToHarvest;

                    string plantModelPath = GetModelPathForCactusByPlantState(plantState);
                    UpdatePlantModel(plantModelPath);

                    UiManager.Instance.InteractLabel.Text = "Press (F) to harvest this plant";
                    break;
                }
            case PlantState.ReadyToHarvest: {
                    GD.Print("harvesting plant and freeing model");
                    plantModel?.Free();
                    plantModel = null;

                    growPlotState = GrowPlotState.Dry;
                    plantState = PlantState.YoungPlant;

                    bool result = Player.Player.Instance.TryPlaceItemInHotbar(PlantItem.Cactus);
                    if (!result) {
                        GD.Print("couldnt add item to inventory, inventory already full. what to do now?");
                        break;
                    }
                    if (GameManager.Instance.CurrentObjective == GameManager.GameObjective.GrowFirstPlant) {
                        GameManager.Instance.UpdateObjective(GameManager.GameObjective.SellFirstPlant);
                    }
                    StandardMaterial3D newMaterial = new() {
                        AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/textures_2/Dirt/Dirt_02/Dirt_02_basecolor.png")
                    };
                    _dirtPatchMesh.SetSurfaceOverrideMaterial(0, newMaterial);
                    break;
                }
        }
    }

    private static string GetModelPathForCactusByPlantState(PlantState plantState) {
        return plantState switch {
            PlantState.YoungPlant => "res://assets/models/nature/cactus/Cactus_3.glb",
            PlantState.AgedPlant => "res://assets/models/nature/cactus/Cactus_2.glb",
            PlantState.ReadyToHarvest => "res://assets/models/nature/cactus/CactusFlowers_2.glb",
            _ => "",
        };
    }

    private static string GetTextForInteractLabel(GrowPlotState growPlotState) {
        // for now we always plant a cactus
        return growPlotState switch {
            GrowPlotState.Dry => "Press (F) to water this grow plot",
            GrowPlotState.Watered => "Press (F) to plant a young cactus",
            GrowPlotState.HasPlant => "Press (F) to harvest the plant",
            _ => "",
        };
    }

    // in the future, we probably want one model that contains the different stages, because this is overhead
    private void UpdatePlantModel(string pathToNewGlbModel) {
        plantModel?.Free();
        plantModel = null;

        PackedScene scene = GD.Load<PackedScene>(pathToNewGlbModel);
        Node3D node = scene.Instantiate<Node3D>();
        plantModel = node;
        AddChild(node);
    }
}