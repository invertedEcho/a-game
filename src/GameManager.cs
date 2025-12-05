using Godot;

namespace agame;

public partial class GameManager : Node {
    public static GameManager Instance { get; private set; }

    public enum GameObjective {
        GrowFirstPlant,
        SellFirstPlant,
        BuyFirstPlot,
        PlaceFirstPlot
    }

    public GameObjective CurrentObjective { get; private set; } = GameObjective.GrowFirstPlant;

    public override void _Ready() {
        Instance = this;
    }

    public void UpdateObjective(GameObjective newObjective) {
        CurrentObjective = newObjective;
        string newObjectiveText = GetCurrentObjectiveDescription(newObjective);

        UiManager.Instance.CurrentObjectiveLabel.Text = newObjectiveText;
    }

    public static string GetCurrentObjectiveDescription(GameObjective currentGameObjective) {
        string currentObjectiveText = "Current Objective: ";
        return currentGameObjective switch {
            GameObjective.GrowFirstPlant => currentObjectiveText += "Grow your first cactus!",
            GameObjective.SellFirstPlant => currentObjectiveText += "Sell your first cactus!",
            GameObjective.BuyFirstPlot => currentObjectiveText += "Buy your first plot!",
            GameObjective.PlaceFirstPlot => currentObjectiveText += "Place your first plot!",
            _ => "No current objective",
        };
    }
}