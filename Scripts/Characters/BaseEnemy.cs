using Godot;

namespace DormShadowsGame.Scripts.Characters;

public abstract partial class BaseEnemy : CharacterBody2D
{
	[Export] public int Damage { get; set; } = 1;
	[Export] protected Area2D Hitbox;

	public override void _Ready()
	{
		AddToGroup("enemy");
		if (Hitbox != null) Hitbox.BodyEntered += OnHitboxEntered;
	}

	protected virtual void OnHitboxEntered(Node2D body)
	{
		if (body is Player player) player.TakeDamage(Damage, GlobalPosition);
	}
}
