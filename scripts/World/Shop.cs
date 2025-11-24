using Godot;

public partial class Shop : Node3D {
    bool playerInRange;

    [Export]
    Area3D area3D;

    Player player;

    public override void _Ready() {
        // hmm i dont know how i feel about this, maybe better way to access player inventory?
        player = GetNode<Player>("/root/World/Player");

        // TODO: only connect this once we have reached GameObjective.SellFirstPlant
        area3D.BodyEntered += OnBodyEntered;
        area3D.BodyExited += OnBodyExit;
    }

    private void OnBodyEntered(Node3D body) {
        if (GameManager.Instance.CurrentObjective != GameManager.GameObjective.SellFirstPlant) {
            return;
        }

        if (!body.IsInGroup("player")) return;

        UiManager.Instance.InteractLabel.Text = $"Press (F) to sell your Cactus for {GameConstants.CactusSellPrize} coins!";
        GD.Print("player has entered shop range!");
        playerInRange = true;
        UiManager.Instance.InteractLabel.Visible = true;
    }

    private void OnBodyExit(Node3D body) {
        if (!body.IsInGroup("player")) return;

        GD.Print("player has exited shop range!");
        playerInRange = false;
        UiManager.Instance.InteractLabel.Visible = false;
    }

    public override void _Process(double delta) {
        HandleInteractionWithShop();
    }

    private void HandleInteractionWithShop() {
        bool interactActionJustPressed = Input.IsActionJustPressed("interact");

        if (!(playerInRange && interactActionJustPressed)) return;

        switch (GameManager.Instance.CurrentObjective) {
            case GameManager.GameObjective.SellFirstPlant:
                // TODO: use prize depending on plant type being sold
                Player.Instance.AddCoin(GameConstants.CactusSellPrize);
                GameManager.Instance.UpdateObjective(GameManager.GameObjective.BuyFirstPlot);
                GD.Print("Player has sold cactus");
                break;
            case GameManager.GameObjective.BuyFirstPlot:
                if (Player.Instance.CoinCount < GameConstants.PlotPrize) {
                    GD.Print("player doesnt have enough money to buy their first plot!");
                }
                break;
        }
    }
}
