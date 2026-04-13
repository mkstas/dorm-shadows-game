using Godot;

namespace DormShadowsGame.Scripts.Characters;

public abstract partial class BaseEnemy : CharacterBody2D
{
	[Export] public int MaxHealth { get; set; } = 1;
	[Export] public int Damage { get; set; } = 1;
	[Export] protected Area2D Hitbox;

	protected int CurrentHealth;
	protected bool IsDead = false;

	public override void _Ready()
	{
		AddToGroup("enemy");
		CurrentHealth = MaxHealth;
		if (Hitbox != null) Hitbox.BodyEntered += OnHitboxEntered;
	}

	protected virtual void OnHitboxEntered(Node2D body)
	{
		if (IsDead) return;
		if (body is Player player && player.Velocity.Y <= 0)
		{
			player.TakeDamage(Damage, GlobalPosition);
		}
	}

	public virtual bool TakeDamage(int amount)
	{
		if (IsDead) return false;
		CurrentHealth -= amount;
		if (CurrentHealth <= 0) { Die(); return true; }
		PlayHitEffect();
		return false;
	}

	protected virtual void PlayHitEffect()
	{
		var tween = CreateTween();
		tween.TweenProperty(this, "modulate", Colors.Red, 0.1f);
		tween.TweenProperty(this, "modulate", Colors.White, 0.1f);
	}

	public virtual void Die()
	{
		if (IsDead) return;
		IsDead = true;

		SetPhysicsProcess(false);
		SetProcess(false);

		if (Hitbox != null) Hitbox.SetDeferred("monitoring", false);
		GetNodeOrNull<CollisionShape2D>("CollisionShape2D")?.SetDeferred("disabled", true);

		AnimatedSprite2D sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D");
		if (sprite == null)
		{
			foreach (var child in GetChildren())
			{
				if (child is AnimatedSprite2D s) { sprite = s; break; }
			}
		}

		if (sprite != null && sprite.SpriteFrames.HasAnimation("die"))
		{
			sprite.Play("die");
			sprite.AnimationFinished += OnDieAnimationFinished;
		}
		else
		{
			StartFadeOut(sprite);
		}
	}

	private void OnDieAnimationFinished()
	{
		AnimatedSprite2D sprite = GetNodeOrNull<AnimatedSprite2D>("AnimatedSprite2D") ??
								 (AnimatedSprite2D)FindChild("*AnimatedSprite2D*");
		StartFadeOut(sprite);
	}

	private void StartFadeOut(Node2D target)
	{
		if (target == null) { QueueFree(); return; }

		var tween = CreateTween();
		tween.TweenProperty(target, "modulate:a", 0.0f, 1.0f);
		tween.Finished += () => QueueFree();
	}
}
