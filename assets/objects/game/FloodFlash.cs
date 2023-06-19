using Godot;
using System.Collections.Generic;

public partial class FloodFlash : Node2D
{
	[Export]
	public List<Texture> Textures;

	public AnimationPlayer AnimationPlayer;
	public Sprite Sprite;

	public void AnimationPlayerFinished(string animName)
	{
		QueueFree();
	}

	public void StartFlash(int tileId)
	{
		Sprite.Texture = Textures[tileId - 1];
	}

	public override void _Ready()
	{
        Sprite = GetNode<Sprite>("Sprite");

        AnimationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        AnimationPlayer.Connect("animation_finished", this, nameof(AnimationPlayerFinished));
	}
}
