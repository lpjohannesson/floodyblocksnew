using Godot;

public class ClickArea : Area2D
{
    [Signal]
    public delegate void AreaClicked();

    public void ClickAreaInputEvent(Node viewport, InputEvent @event, long shapeIdx)
    {
        if (@event is InputEventMouseButton eventMouseButton)
        {
            if (eventMouseButton.Pressed && eventMouseButton.ButtonIndex == (int)ButtonList.Left)
            {
                EmitSignal(nameof(AreaClicked));
            }
        }
    }

    public override void _Ready()
    {
        Connect("input_event", this, nameof(ClickAreaInputEvent));
    }

}