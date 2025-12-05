using Godot;
using Godot.Collections;

namespace agame;

public partial class UiManager : Control {
    public static UiManager Instance { get; private set; }

    GameManager ObjectiveManager;

    [Export]
    public Label CurrentObjectiveLabel { get; set; }

    [Export]
    public Label InteractLabel { get; set; }

    [Export]
    public Label CoinsLabel { get; set; }

    private Array<TextureRect> HotbarSlotTextures = [];

    private Array<TextureRect> ItemPreviewSlotTextures = [];

    public override void _Ready() {
        Instance = this;
        CurrentObjectiveLabel.Text = GameManager.GetCurrentObjectiveDescription(GameManager.Instance.CurrentObjective);

        for (int i = 0; i < Player.Player.HotbarSize; i++) {
            HotbarSlotTextures.Add(GetNode<TextureRect>($"/root/World/CanvasLayer/UiRoot/Hotbar/Slot{i + 1}"));
            ItemPreviewSlotTextures.Add(GetNode<TextureRect>($"/root/World/CanvasLayer/UiRoot/Hotbar/Slot{i + 1}/ItemPreview"));
        }
        foreach (TextureRect inventorySlot in HotbarSlotTextures) {
            inventorySlot.Texture = GD.Load<Texture2D>("res://assets/hud/default-hotbar-slot.png");
        }
        HotbarSlotTextures[0].Texture = GD.Load<Texture2D>("res://assets/hud/selected-hotbar-slot.png");
    }

    public void UpdateSelectedHotbarSlot(int index) {
        // update all inventory slot textures to default
        foreach (TextureRect hotbarSlot in HotbarSlotTextures) {
            hotbarSlot.Texture = GD.Load<Texture2D>("res://assets/hud/default-hotbar-slot.png");
        }
        HotbarSlotTextures[index].Texture = GD.Load<Texture2D>("res://assets/hud/selected-hotbar-slot.png");
    }

#nullable enable
    public void UpdateItemPreviewSlotTexture(int hotbarIndex, string? pathToPreview) {
        if (pathToPreview is null) {
            ItemPreviewSlotTextures[hotbarIndex].Texture = null;
        }
        else {
            ItemPreviewSlotTextures[hotbarIndex].Texture = GD.Load<Texture2D>(pathToPreview);
        }
    }
}