using Godot;

public partial class CardComponent : PanelContainer
{
    private Label _selectionName = null!;
    private ProgressBar _tiredness = null!;
    private ProgressBar _satiation = null!;
    private Label _selectionInventory = null!;

    public override void _Ready()
    {
        _selectionName = GetNode<Label>("%SelectionName");
        _tiredness = GetNode<ProgressBar>("%Tiredness");
        _satiation = GetNode<ProgressBar>("%Satiation");
        _selectionInventory = GetNode<Label>("%SelectionInventory");
    }

    public void ShowSelection(string name, double tiredness, double satiation, string? inventory)
    {
        _selectionName.Text = name;
        _tiredness.Value = tiredness;
        _satiation.Value = satiation;
        _selectionInventory.Text = inventory ?? string.Empty;
        _selectionInventory.Visible = !string.IsNullOrWhiteSpace(inventory);
        Visible = true;
    }

    public void ClearSelection() => Visible = false;
}
