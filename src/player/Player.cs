using Godot;
using Godot.Collections;
using agame.Items;
using static agame.World.GrowPlot;
using static agame.Items.BuildItem;
using agame.scripts.Player;

namespace agame.Player;

public partial class Player : CharacterBody3D {

    // --- Player camera ---
    private Camera3D _playerCamera;
    private float _cameraJiggleAmmount = 0.3f;
    private Vector3 _cameraInitalPos;
    private Vector3 _cameraTargetPos;
    private bool _wasOnFloor = false;

    // --- Grapple gun --- (TODO: move somewhere else)
    private Vector3? _grappleHit = null;
    private float _grappleStren = 80.0f;
    private Rope _rope = null;

    // --- Movement related ---
    private PlayerMovement _playerMovement;

    public static Player Instance { get; private set; }

    public const int HotbarSize = 8;
    private Array<GameItem> _hotbar = [];

    public float CoinCount { get; private set; } = 0f;

    public int CurrentHotbarSlotSelected = 0;

    private Node3D _growPlotBlueprint = null;

    public override void _Ready() {
        Instance = this;
        _playerMovement = new PlayerMovement();
        _playerCamera = (Camera3D)GetNode("PlayerCamera");
        _cameraTargetPos = _playerCamera.Position;
        _cameraInitalPos = _playerCamera.Position;
        for (int i = 0; i < HotbarSize; i++) {
            _hotbar.Add(new GameItem { IsPlaceHolder = true });
        }
    }

    public override void _PhysicsProcess(double delta) {
        _playerMovement.HandleMovement(IsOnFloor(), (float)delta);
        Velocity = _playerMovement.finalVelocity;
        MoveAndSlide();

        // Check for change
        if (IsOnFloor() != _wasOnFloor) {
            if (_wasOnFloor)
                OnJumpStart();
            else
                OnJumpImpact();
        }
        _wasOnFloor = IsOnFloor();
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

        bool currentHotbarItemIsGrowPlot = _hotbar[CurrentHotbarSlotSelected] is BuildItem buildItem && buildItem.Type == BuildItemType.GrowPlot;
        if (Input.IsActionJustPressed("build_mode") && currentHotbarItemIsGrowPlot && _growPlotBlueprint is null) {
            var growPlotBlueprint = GD.Load<PackedScene>("res://assets/scenes/grow_plot_blueprint.tscn");
            _growPlotBlueprint = growPlotBlueprint.Instantiate<Node3D>();
            _growPlotBlueprint.Scale = new(0.5f, 0.5f, 0.5f);

            GetTree().Root.AddChild(_growPlotBlueprint);

            // instantiate GrowPlotBlueprint, move it when mouse moves with player position
            // Vector3? positionSnapped = Terrain.Instance.SnapToGround(Position);
            // if (positionSnapped is Vector3 position) {
            //     World.World.Instance.PlaceGrowPlot(position);
            //     RemoveItemFromHotbar(CurrentHotbarSlotSelected);
            // }
        }

        if (_growPlotBlueprint is not null) {
            Vector3 growPlotBluePrintPosition = Position;
            growPlotBluePrintPosition.Z -= 2f;
            _growPlotBlueprint.Position = growPlotBluePrintPosition;
            _growPlotBlueprint.Rotation = _playerCamera.Rotation;
        }

        // --- Read input from player ---
        _playerMovement.HandleInput(_playerCamera.Basis);

        // --- Camera control ---
        _playerCamera.Position = _playerCamera.Position.Lerp(_cameraTargetPos, (float)delta * 10f);

        // --- Grapple gun ---
        if (Input.IsActionJustPressed("mouse_click_left")) {
            _grappleHit = GetCamRaycast(_playerCamera, 3000.0f);
            if (_grappleHit != null) {
                PackedScene ropeScene = GD.Load<PackedScene>("res://assets/scenes/rope.tscn");
                _rope = ropeScene.Instantiate<Rope>();
                GetTree().CurrentScene.AddChild(_rope);
            }
        }
        if (Input.IsActionJustReleased("mouse_click_left")) {
            _grappleHit = null;
            if (_rope != null)
                _rope.QueueFree();
        }

        if (_grappleHit.HasValue) {
            _rope.startPoint = _playerCamera.GlobalPosition - new Vector3(0f, 0.3f, 0f);
            _rope.endPoint = _grappleHit.Value;

            Vector3 toGrapple = _grappleHit.Value - _playerCamera.GlobalPosition;
            toGrapple = toGrapple.Normalized();
            _playerMovement.finalVelocity += toGrapple * (float)delta * _grappleStren * _playerCamera.Position.DistanceTo(_grappleHit.Value) / 10.0f;
        }
    }

    private void UpdateCurrentHotbarSlotSelected(int newHotbarSlotSelected) {
        CurrentHotbarSlotSelected = newHotbarSlotSelected;
        UiManager.Instance.UpdateSelectedHotbarSlot(newHotbarSlotSelected);
        UpdateInteractLabel();
    }

    private void UpdateInteractLabel() {
        bool currentHotbarItemIsGrowPlot = _hotbar[CurrentHotbarSlotSelected] is BuildItem buildItem && buildItem.Type == BuildItemType.GrowPlot;
        if (currentHotbarItemIsGrowPlot) {
            UiManager.Instance.InteractLabel.Visible = true;
            UiManager.Instance.InteractLabel.Text = "Press (E) to enter Build Mode";
        }
        else {
            UiManager.Instance.InteractLabel.Visible = false;
            UiManager.Instance.InteractLabel.Text = "";
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

    // --- Jumping callbacks ---
    private void OnJumpImpact() {
        DoCameraImpact();
    }

    private void OnJumpStart() {

    }

    private async void DoCameraImpact() {
        _cameraTargetPos = _cameraInitalPos - new Vector3(0.0f, 0.5f, 0.0f);
        await ToSignal(GetTree().CreateTimer(0.1f), SceneTreeTimer.SignalName.Timeout);
        _cameraTargetPos = _cameraInitalPos;
    }

    private Vector3? GetCamRaycast(Camera3D camera, float maxDistance) {
        PhysicsRayQueryParameters3D parameters = new PhysicsRayQueryParameters3D {
            From = camera.GlobalPosition,
            To = camera.GlobalPosition + -camera.GlobalTransform.Basis.Z * maxDistance,
            CollideWithAreas = true,
            CollideWithBodies = true
        };
        parameters.Exclude = new Godot.Collections.Array<Rid> { camera.GetCameraRid() };
        var spaceState = GetWorld3D().DirectSpaceState;
        Godot.Collections.Dictionary result = spaceState.IntersectRay(parameters);
        if (result.Count > 0)
            return (Vector3)result["position"];
        return null;
    }
}