using Godot;
using System;

namespace DormShadowsGame.Scripts;

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	public event Action<int> HealthChanged;

	public float Gravity { get; } = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	public bool IsTransitioning { get; set; }
	public Vector2 PlayerSpawnPosition { get; set; }

	private int _health = 3;
	public int Health
	{
		get => _health;
		set
		{
			_health = value;
			HealthChanged?.Invoke(_health);
		}
	}

	public override void _EnterTree()
	{
		if (Instance == null) Instance = this;
		else QueueFree();
	}

	public void TakeDamage(int amount)
	{
		Health -= amount;

		if (Health <= 0)
		{
			Health = 3;
			GetTree().ChangeSceneToFile("res://Scenes/Levels/entrance.tscn");
		}
	}
}
