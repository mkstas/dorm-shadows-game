using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class Cockroach : CharacterBody2D
{
	private enum State { Walking, Waiting }

	[Export] private AnimatedSprite2D _animatedSprite;
	[Export] private Area2D _hitbox;
	[Export] public int Damage = 1;

	[ExportGroup("Patrol Settings")]
	[Export] public float Speed = 40.0f;
	[Export] public float PatrolDistance = 100.0f;
	[Export] public float WaitTime = 1.5f;

	private Vector2 _startPosition;
	private int _direction = 1;
	private State _currentState = State.Walking;
	private float _waitTimer = 0f;

	public override void _Ready()
	{
		AddToGroup("enemy");

		_startPosition = GlobalPosition + new Vector2(PatrolDistance, 0);

		_direction = 1;

		_animatedSprite.Play("walk");

		if (_hitbox != null)
			_hitbox.BodyEntered += OnBodyEntered;
	}

	public override void _PhysicsProcess(double delta)
	{
		float fDelta = (float)delta;

		switch (_currentState)
		{
			case State.Walking:
				HandleWalking(fDelta);
				break;
			case State.Waiting:
				HandleWaiting(fDelta);
				break;
		}

		HandleDamageOverlap();
	}

	private void HandleWalking(float delta)
	{
		Vector2 velocity = Velocity;
		float currentOffset = GlobalPosition.X - _startPosition.X;

		if (_direction == 1 && currentOffset >= 0)
		{
			GlobalPosition = new Vector2(_startPosition.X, GlobalPosition.Y);
			SwitchToWaiting();
			return;
		}
		else if (_direction == -1 && currentOffset <= -PatrolDistance)
		{
			GlobalPosition = new Vector2(_startPosition.X - PatrolDistance, GlobalPosition.Y);
			SwitchToWaiting();
			return;
		}

		velocity.X = _direction * Speed;

		if (!IsOnFloor())
			velocity.Y += GameManager.Instance.Gravity * delta;

		Velocity = velocity;
		MoveAndSlide();

		_animatedSprite.Play("walk");
		_animatedSprite.FlipH = _direction < 0;
	}

	private void SwitchToWaiting()
	{
		_currentState = State.Waiting;
		_waitTimer = WaitTime;

		Velocity = new Vector2(0, Velocity.Y);
		_animatedSprite.Play("idle");

		_direction *= -1;
	}

	private void HandleWaiting(float delta)
	{
		_waitTimer -= delta;

		if (_waitTimer <= 0)
		{
			_currentState = State.Walking;
			_animatedSprite.FlipH = _direction < 0;
		}

		if (!IsOnFloor())
		{
			Velocity = new Vector2(0, Velocity.Y + GameManager.Instance.Gravity * delta);
			MoveAndSlide();
		}
	}

	private void HandleDamageOverlap()
	{
		if (_hitbox == null) return;
		foreach (Node2D body in _hitbox.GetOverlappingBodies())
		{
			if (body is Player player)
				player.TakeDamage(Damage, GlobalPosition);
		}
	}

	private void OnBodyEntered(Node2D body)
	{
		if (body is Player player)
			player.TakeDamage(Damage, GlobalPosition);
	}
}
