using Godot;
using GroundPainter.Data;

namespace GroundPainter.UI;

[Tool]
public partial class PaintControl : HBoxContainer
{
    [Signal] public delegate void MenuActionEventHandler(PaintMenuActionType action);

    [Export] private OptionButton ConstraintButton { get; set; }
    [Export] private BaseButton SelectButton { get; set; }
    [Export] private BaseButton RemoveButton { get; set; }
    [Export] private BaseButton AddButton { get; set; }


    public override void _Ready()
    {
        //ConstraintButton.ItemSelected += OnConstraintSelected;
        //LocalModeButton.Toggled += OnLocalModeToggled;
        //RiverMenuButton.GetPopup().IdPressed += OnMenuButtonPressed;

        //SelectButton.Pressed += () => OnSelectModeChange(SelectButton, RiverEditMode.Select, true);
        //RemoveButton.Pressed += () => OnSelectModeChange(RemoveButton, RiverEditMode.Remove, false);
        //AddButton.Pressed += () => OnSelectModeChange(AddButton, RiverEditMode.Add, true);
    }
}
