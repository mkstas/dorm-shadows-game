using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class PropellerRat : Node2D
{
	[Export] private AnimatedSprite2D _animatedSprite;
	[Export] private CharacterBody2D _player;
	[Export] private Area2D _interactArea;
	[Export] private Label _label;
	[Export] private string _message;

	public override void _Ready()
	{
		_animatedSprite.Play("idle");
		_label.Text = _message;
		_label.Hide();

		_interactArea.BodyEntered += (body) => { if (body.IsInGroup("player")) _label.Show(); };
		_interactArea.BodyExited += (body) => { if (body.IsInGroup("player")) _label.Hide(); };
	}

	public override void _Process(double delta)
	{
		_animatedSprite.FlipH = _player.GlobalPosition.X > GlobalPosition.X;
	}
}
