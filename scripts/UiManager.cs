using Godot;

public partial class UiManager : Control {
    public static UiManager Instance;

    GameManager ObjectiveManager;

    [Export]
    public Label CurrentObjectiveLabel { get; set; }

    [Export]
    public Label InteractLabel { get; set; }

    [Export]
    public Label CoinsLabel { get; set; }

    [Export]
    public TextureRect InventorySlot1;

    [Export]
    public TextureRect InventorySlot2;

    [Export]
    public TextureRect InventorySlot3;

    [Export]
    public TextureRect InventorySlot4;

    [Export]
    public TextureRect InventorySlot5;

    [Export]
    public TextureRect InventorySlot6;

    [Export]
    public TextureRect InventorySlot7;

    [Export]
    public TextureRect InventorySlot8;

    public override void _Ready() {
        Instance = this;
        CurrentObjectiveLabel.Text = GameManager.GetCurrentObjectiveDescription(GameManager.Instance.CurrentObjective);
    }
}
