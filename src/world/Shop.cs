using Godot;
using Godot.Collections;
using agame.Items;

namespace agame.World;

public partial class Shop : Node3D {
    private bool _playerInRange;

    [Export]
    private Area3D _area3D;

    private Control _shopMenu;

    [Export]
    private ItemList _shopMenuList;
    [Export]
    private Label _descriptionLabel;
    [Export]
    private Label _playerOwningCountLabel;
    [Export]
    private Button _buyButton;
    [Export]
    private Button _sellButton;

    private CanvasLayer _rootCanvasLayer;

    // we shouldnt have to save this again, we already have _shopMenuList, find out if we can save our ownn data in a `ItemList`
    private Array<GameItem> _shopItems = [];

#nullable enable
    private GameItem? _selectedGameItem;

    public override void _Ready() {
        _area3D = GetNode<Area3D>("Area3D");
        _shopMenu = GetNode<Control>("ShopMenu");
        _shopMenu.Visible = false;

        _rootCanvasLayer = GetNode<CanvasLayer>("/root/World/CanvasLayer");

        _area3D.BodyEntered += OnBodyEntered;
        _area3D.BodyExited += OnBodyExit;

        _shopMenuList.ItemSelected += OnItemSelected;

        AddItemToShopList(PlantItem.Cactus);

        _buyButton.Pressed += OnBuyButtonClick;
        _sellButton.Pressed += OnSellButtonClick;

    }

    public override void _Process(double delta) {
        HandleInteractionWithShop();
    }

    private void OnItemSelected(long index) {
        GD.Print($"new item selected: {_selectedGameItem} index {index}");
        _selectedGameItem = _shopItems[(int)index];
        UpdateBuyAndSellButtonStates();
    }

    private void UpdateBuyAndSellButtonStates() {
        if (_selectedGameItem is null) {
            return;
        }

        _descriptionLabel.Text = _selectedGameItem.Description;
        int count = UpdateUserOwnLabelOfSelectedItem();
        _buyButton.Text = $"Buy ({_selectedGameItem.BuyPrice})";
        _sellButton.Text = $"Sell ({_selectedGameItem.SellPrice})";

        _sellButton.Disabled = count == 0;

        bool playerHasEnoughCoinsToBuy = Player.Player.Instance.CoinCount >= _selectedGameItem.BuyPrice;
        _buyButton.Disabled = !playerHasEnoughCoinsToBuy;
    }

    private void OnBuyButtonClick() {
        if (_selectedGameItem is not GameItem gameItem) {
            return;
        }

        float price = gameItem.BuyPrice;
        if (Player.Player.Instance.CoinCount < price) {
            GD.Print("player doesnt have enough money to buy");
            return;
        }
        Player.Player.Instance.UpdateCoin(-price);
        Player.Player.Instance.AppendItemToInventory(gameItem);
        UpdateUserOwnLabelOfSelectedItem();

        if (gameItem is BuildItem buildItem && buildItem.Type == BuildItem.BuildItemType.GrowPlot && GameManager.Instance.CurrentObjective == GameManager.GameObjective.BuyFirstPlot) {
            GameManager.Instance.UpdateObjective(GameManager.GameObjective.PlaceFirstPlot);
        }

        UpdateBuyAndSellButtonStates();
    }

    private void OnSellButtonClick() {
        if (_selectedGameItem is PlantItem plantItem) {
            GD.Print("Player trying to sell a plant");
            (PlantItem, int)? itemFromInventory = Player.Player.Instance.GetPlantItemByType(plantItem.Type);
            if (itemFromInventory is (PlantItem item, int index)) {
                float sellPrice = item.SellPrice;
                Player.Player.Instance.UpdateCoin(sellPrice);
                Player.Player.Instance.RemoveItemFromInventory(index);

                UpdateUserOwnLabelOfSelectedItem();

                if (GameManager.Instance.CurrentObjective == GameManager.GameObjective.SellFirstPlant) {
                    GameManager.Instance.UpdateObjective(GameManager.GameObjective.BuyFirstPlot);
                    AddItemToShopList(BuildItem.GrowPlot);
                }
                UpdateBuyAndSellButtonStates();
            }
        }
        GD.Print("Player trying to sell something else than a plant");
    }

    private void OnBodyEntered(Node3D body) {
        if (!body.IsInGroup("player")) return;

        GD.Print("player has entered shop range!");
        if (GameManager.Instance.CurrentObjective == GameManager.GameObjective.GrowFirstPlant) {
            GD.Print("current objective is growfirstplant, ignoring player entered shop range");
            return;
        }

        UiManager.Instance.InteractLabel.Text = $"(F) Interact";
        _playerInRange = true;
        UiManager.Instance.InteractLabel.Visible = true;
    }

    public void OnBodyExit(Node3D body) {
        if (!body.IsInGroup("player")) return;

        GD.Print("player has exited shop range!");
        _playerInRange = false;
        UiManager.Instance.InteractLabel.Visible = false;

        // or we disallow movement when shop menu is open
        _shopMenu.Visible = false;
        _rootCanvasLayer.Visible = true;
    }

    private void HandleInteractionWithShop() {
        if (!_playerInRange) {
            return;
        }

        var interactActionJustPressed = Input.IsActionJustPressed("interact");
        if (interactActionJustPressed) {
            _shopMenu.Visible = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
            UpdateUserOwnLabelOfSelectedItem();
            UpdateBuyAndSellButtonStates();
            _rootCanvasLayer.Visible = false;
        }
        else if (Input.IsActionJustPressed("escape") && _shopMenu.Visible) {
            _shopMenu.Visible = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
            _rootCanvasLayer.Visible = true;
        }
    }

    /// Updates the player own count label for the currently selected item and returns the count
    private int UpdateUserOwnLabelOfSelectedItem() {
        // TODO: if we add a new item type, we should get an error here so we automatically know we have to update this method
        if (_selectedGameItem is PlantItem plantItem) {
            int playerOwnCount = Player.Player.Instance.GetPlayerOwnCountForPlantItemByType(plantItem.Type);
            _playerOwningCountLabel.Text = $"You have: {playerOwnCount}";
            return playerOwnCount;
        }
        else if (_selectedGameItem is BuildItem buildItem) {
            int playerOwnCount = Player.Player.Instance.GetPlayerOwnCountnForBuildItemByType(buildItem.Type);
            _playerOwningCountLabel.Text = $"You have: {playerOwnCount}";
            return playerOwnCount;
        }
        return 0;
    }

    private void AddItemToShopList(GameItem gameItem) {
        _shopItems.Add(gameItem);
        _shopMenuList.AddItem(gameItem.ItemName, GD.Load<Texture2D>(gameItem.PathToTexture));
    }
}