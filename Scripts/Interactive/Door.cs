using Godot;
using DormShadowsGame.Scripts.Managers;

namespace DormShadowsGame.Scripts.Interactive;

public partial class Door : Node2D
{
	[Export] private string _nextLevelPath = "res://Scenes/Levels/entrance.tscn";
	[Export] private Marker2D _spawnMarker;
	[Export] private Area2D _interactArea;
	[Export] private Label _keyHint;

	public override void _Ready()
	{
		_keyHint.Hide();
		_interactArea.BodyEntered += (body) => ToggleHint(body, true);
		_interactArea.BodyExited += (body) => ToggleHint(body, false);
	}

	public override void _EnterTree()
	{
		if (GameManager.Instance != null)
			GameManager.Instance.PlayerSpawnPosition = _spawnMarker.GlobalPosition;
	}

	public override void _Input(InputEvent @event)
	{
		if (_keyHint.Visible && @event.IsActionPressed("interact") && !GameManager.Instance.IsSceneTransitioning)
		{
			SceneTransition.Instance.FadeToScene(_nextLevelPath);
		}
	}

	private void ToggleHint(Node2D body, bool active)
	{
		if (body.IsInGroup("player")) _keyHint.Visible = active;
	}
}
