using DormShadowsGame.Scripts.Managers;
using Godot;

namespace DormShadowsGame.Scripts;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }
	public float Gravity { get; } = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public bool IsSceneTransitioning { get; set; }

	public override void _EnterTree() => Instance = this;

	[Signal] public delegate void HealthChangedEventHandler(int health);

	public Vector2 PlayerSpawnPosition { get; set; }

	private int _health = 3;
	public int Health
	{
		get => _health;
		set
		{
			_health = Mathf.Max(0, value);
			EmitSignal(SignalName.HealthChanged, _health);

			if (_health <= 0) Respawn();
		}
	}

	public bool PlayerMovementEnabled { get; set; } = true;

	private void Respawn()
	{
		Health = 3;
		SceneTransition.Instance.FadeToScene("res://Scenes/Levels/entrance.tscn");
	}
}
