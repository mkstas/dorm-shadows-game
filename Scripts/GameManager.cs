using Godot;

namespace DormShadowsGame.Scripts;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Signal] public delegate void HealthChangedEventHandler(int currentHealth);

	public float Gravity { get; } = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public bool IsTransitioning { get; set; }
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

	public override void _EnterTree()
	{
		if (Instance == null) Instance = this;
		else QueueFree();
	}

	private void Respawn()
	{
		Health = 3;
		GetTree().ChangeSceneToFile("res://Scenes/Levels/entrance.tscn");
	}
}
