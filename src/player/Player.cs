using Godot;
using Godot.Collections;
using agame.Items;
using static agame.World.GrowPlot;
using static agame.Items.BuildItem;

namespace agame.Player;

public partial class Player : CharacterBody3D {
    private PlayerCamera _playerCamera;
    private int _speed = 15;
    private float _jumpVelocity = 10.0f;

    public static Player Instance { get; private set; }

    public float CoinCount { get; private set; } = 0f;

    public const int HotbarSize = 8;
    // maybe have a better data structure, we want fixed size of HotbarSize. for now we just ensure this at developer side that it wont be bigger than this
    private Array<GameItem> _hotbar = [];

    public int CurrentHotbarSlotSelected = 0;

    public override void _Ready() {
        Instance = this;
        _playerCamera = (PlayerCamera)GetNode("PlayerCamera");
        for (int i = 0; i < HotbarSize; i++) {
            _hotbar.Add(new GameItem { IsPlaceHolder = true });
        }
    }

    public override void _PhysicsProcess(double delta) {
        ApplyGravity(delta);
        HandleMovementInput();
        MoveAndSlide();
    }

    public override void _Process(double delta) {
        if (Input.IsActionJustPressed("hotbar_1")) {
            UpdateCurrentHotbarSlotSelected(0);
        }
        else if (Input.IsActionJustPressed("hotbar_2")) {
            UpdateCurrentHotbarSlotSelected(1);
        }
        else if (Input.IsActionJustPressed("hotbar_3")) {
            UpdateCurrentHotbarSlotSelected(2);
        }
        else if (Input.IsActionJustPressed("hotbar_4")) {
            UpdateCurrentHotbarSlotSelected(3);
        }
        else if (Input.IsActionJustPressed("hotbar_5")) {
            UpdateCurrentHotbarSlotSelected(4);
        }
        else if (Input.IsActionJustPressed("hotbar_6")) {
            UpdateCurrentHotbarSlotSelected(5);
        }
        else if (Input.IsActionJustPressed("hotbar_7")) {
            UpdateCurrentHotbarSlotSelected(6);
        }
        else if (Input.IsActionJustPressed("hotbar_8")) {
            UpdateCurrentHotbarSlotSelected(7);
        }
    }

    private void UpdateCurrentHotbarSlotSelected(int newHotbarSlotSelected) {
        CurrentHotbarSlotSelected = newHotbarSlotSelected;
        UiManager.Instance.UpdateSelectedHotbarSlot(newHotbarSlotSelected);
    }

    private void HandleMovementInput() {
        if (_playerCamera.freeCamEnabled) return;
        Vector3 localVelocity = new();

        if (Input.IsActionPressed("move_left")) {
            localVelocity.X -= 1.0f;
        }
        if (Input.IsActionPressed("move_right")) {
            localVelocity.X += 1.0f;
        }
        if (Input.IsActionPressed("move_forward")) {
            localVelocity.Z -= 1.0f;
        }
        if (Input.IsActionPressed("move_backwards")) {
            localVelocity.Z += 1.0f;
        }

        localVelocity = localVelocity.Normalized();

        Basis basis = _playerCamera.GlobalBasis;
        Vector3 movement = basis.X * localVelocity.X + basis.Z * localVelocity.Z;

        Velocity = new Vector3(movement.X * _speed, Velocity.Y, movement.Z * _speed);

        if (IsOnFloor() && Input.IsActionJustPressed("jump")) {
            Velocity = new Vector3(Velocity.X, _jumpVelocity, Velocity.Z);
        }
    }

    private void ApplyGravity(double delta) {
        if (!IsOnFloor()) {
            Velocity += new Vector3(0.0f, -30.0f * (float)delta, 0.0f);
        }
        else if (Velocity.Y < 0) {
            Velocity = new Vector3(Velocity.X, 0f, Velocity.Z);
        }
    }

    /// To substract coins, use a negative number
    public void UpdateCoin(float toAdd) {
        CoinCount += toAdd;
        UiManager.Instance.CoinsLabel.Text = CoinCount.ToString();
    }

    /// <summary>
    /// Attempts to place the given item into the currently selected hotbar slot.
    /// If the selected slot is occupied, tries to place the item into the next
    /// available empty slot from left to right.
    /// </summary>
    /// <param name="itemToAdd">The item to add to the hotbar.</param>
    /// <returns>
    /// True if the item is placed successfully; otherwise false.
    /// </returns>
    public bool TryPlaceItemInHotbar(GameItem itemToAdd) {
        if (_hotbar[CurrentHotbarSlotSelected].IsPlaceHolder) {
            _hotbar[CurrentHotbarSlotSelected] = itemToAdd;
            UiManager.Instance.UpdateItemPreviewSlotTexture(CurrentHotbarSlotSelected, itemToAdd.PathToTexture);
            return true;
        }
        for (int index = 0; index < _hotbar.Count; index++) {
            GameItem currentGameItem = _hotbar[index];

            if (!currentGameItem.IsPlaceHolder) continue;

            _hotbar[index] = itemToAdd;
            UiManager.Instance.UpdateItemPreviewSlotTexture(index, itemToAdd.PathToTexture);
            return true;
        }
        return false;
    }

    /// Returns a boolean indicating whether the removal of the item at the given index was succesful
    public bool RemoveItemFromHotbar(int index) {
        if (index + 1 > HotbarSize) {
            GD.PrintErr($"The inventory has a size of {HotbarSize} so the given index {index} is invalid");
            return false;
        }

        _hotbar[index] = new GameItem { IsPlaceHolder = true };
        UiManager.Instance.UpdateItemPreviewSlotTexture(index, null);
        return true;
    }

    /// Returns the game item and index for the given GameItemType
    public (PlantItem, int)? GetPlantItemByType(PlantType plantType) {
        for (int index = 0; index < HotbarSize; index++) {
            GameItem currentItem = _hotbar[index];
            if (currentItem is PlantItem plantItem && plantItem.Type == plantType) {
                return (plantItem, index);
            }
        }
        return null;
    }

    public int GetPlayerOwnCountForPlantItemByType(PlantType plantType) {
        int count = 0;
        for (int index = 0; index < HotbarSize; index++) {
            GameItem currentItem = _hotbar[index];
            if (currentItem is PlantItem plantItem && plantItem.Type == plantType) {
                count++;
            }
        }
        return count;
    }

    public int GetPlayerOwnCountnForBuildItemByType(BuildItemType buildItemType) {
        int count = 0;
        for (int index = 0; index < HotbarSize; index++) {
            GameItem currentItem = _hotbar[index];
            if (currentItem is BuildItem buildItem && buildItem.Type == buildItemType) {
                count++;
            }
        }
        return count;

    }
}