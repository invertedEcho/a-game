using Godot;

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

    enum PlantType {
        Cactus,
    }

    enum PlantState {
        YoungPlant,
        AgedPlant,
        ReadyToHarvest,
    }

    [Export]
    MeshInstance3D groundMesh;
    [Export]
    Area3D area3D;

    Node3D plantModel;

    PlantState plantState;
    GrowPlotState growPlotState = GrowPlotState.Dry;

    bool playerInRange;

    public override void _Ready() {
        area3D.BodyEntered += OnBodyEntered;
        area3D.BodyExited += OnBodyExit;
    }

    public override void _Process(double delta) {
        if (!playerInRange) return;
        HandleInteractionWithGrowPlot();
    }

    private void OnBodyEntered(Node3D body) {
        if (!body.IsInGroup("player")) return;
        GD.Print($"body has entered dirtpatch range! {body}");
        playerInRange = true;
        UiManager.Instance.InteractLabel.Visible = true;
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

                StandardMaterial3D newMaterial = new StandardMaterial3D();
                newMaterial.AlbedoTexture = (Texture2D)GD.Load("res://assets/textures/wet_dirt.png");
                groundMesh.SetSurfaceOverrideMaterial(0, newMaterial);

                UiManager.Instance.InteractLabel.Text = "Press (F) to plant a Young Cactus";

                return;
            case GrowPlotState.Watered:
                growPlotState = GrowPlotState.HasPlant;
                plantState = PlantState.YoungPlant;

                string cactusModelPath = GetModelPathForCactusByPlantState(plantState);
                PackedScene plantScene = GD.Load<PackedScene>(cactusModelPath);
                Node3D plantModel = plantScene.Instantiate<Node3D>();
                AddChild(plantModel);

                string interactLabelText = "Press (F) to make this Young Plant an Aged Plant";
                UiManager.Instance.InteractLabel.Text = interactLabelText;

                return;
        }

        if (growPlotState != GrowPlotState.HasPlant) return;

        // free the model so we can add new plant model
        if (plantModel != null) {
            GD.Print("freeing plant model");
            this.plantModel.Free();
        }

        switch (plantState) {
            case PlantState.YoungPlant: {
                    plantState = PlantState.AgedPlant;

                    string plantModelPath = GetModelPathForCactusByPlantState(plantState);
                    PackedScene plantScene = (PackedScene)GD.Load(plantModelPath);
                    this.plantModel = plantScene.Instantiate<Node3D>();
                    GD.Print("adding new plant model");
                    AddChild(this.plantModel);

                    UiManager.Instance.InteractLabel.Text = "Press (F) to make this plant ready to harvest";
                    break;
                }
            case PlantState.AgedPlant: {
                    plantState = PlantState.ReadyToHarvest;

                    string plantModelPath = GetModelPathForCactusByPlantState(plantState);
                    PackedScene plantScene = (PackedScene)GD.Load(plantModelPath);
                    this.plantModel = plantScene.Instantiate<Node3D>();
                    GD.Print("adding new plant model");
                    AddChild(plantModel);

                    UiManager.Instance.InteractLabel.Text = "Press (F) to harvest this plant";
                    break;
                }
            case PlantState.ReadyToHarvest: {
                    growPlotState = GrowPlotState.Dry;
                    plantState = PlantState.YoungPlant;

                    GameItem gameItem = new GameItem {
                        BuyPrice = GameConstants.CactusBuyPrize,
                        DescriptionName = "Cactus",
                        PathToTexture = "res://assets/models/nature/cactus/grown_cactus.png",
                        SellPrice = GameConstants.CactusSellPrize,
                        IsPlaceHolder = false
                    };

                    bool result = Player.Instance.AppendItemToInventory(gameItem);
                    GameManager.Instance.UpdateObjective(GameManager.GameObjective.SellFirstPlant);
                    if (!result) {
                        GD.Print("couldnt add item to inventory, inventory already full. what to do now?");
                    }
                    break;
                }
        }
    }

    private static string GetModelPathForCactusByPlantState(PlantState plantState) {
        switch (plantState) {
            case PlantState.YoungPlant:
                return "res://assets/models/nature/cactus/Cactus_3.glb";
            case PlantState.AgedPlant:
                return "res://assets/models/nature/cactus/Cactus_2.glb";
            case PlantState.ReadyToHarvest:
                return "res://assets/models/nature/cactus/CactusFlowers_2.glb";
            default: return "";
        }
    }

    private static string GetTextForInteractLabel(GrowPlotState growPlotState) {
        // for now we always plant a cactus
        switch (growPlotState) {
            case GrowPlotState.Dry: return "Press (F) to water this grow plot";
            case GrowPlotState.Watered: return "Press (F) to plant a young cactus";
            case GrowPlotState.HasPlant: return "Press (F) to harvest the plant";
            default: return "";
        }
    }

}
