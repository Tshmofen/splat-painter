using Godot;
using GroundPainter.Data.UI;

namespace GroundPainter.UI;

[Tool]
public partial class PaintControl : HBoxContainer
{
    [Signal] public delegate void MenuActionEventHandler(PaintMenuActionType action);

    [Export] private BaseButton PaintR { get; set; }
    [Export] private BaseButton PaintG { get; set; }
    [Export] private BaseButton PaintB { get; set; }
    [Export] private BaseButton PaintA { get; set; }


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
