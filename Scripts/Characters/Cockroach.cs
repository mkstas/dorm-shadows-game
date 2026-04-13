using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class Cockroach : BaseEnemy
{
	private enum State { Walking, Waiting }

	[Export] private AnimatedSprite2D _sprite;
	[Export] public float Speed = 64.0f;
	[Export] public float PatrolDistance = 48.0f;
	[Export] public float WaitTime = 1.0f;

	private Vector2 _anchorPosition;
	private int _direction = 1;
	private State _state = State.Walking;
	private float _timer;

	public override void _Ready()
	{
		base._Ready();
		_anchorPosition = GlobalPosition;
		_sprite.Play("walk");
	}

	public override void _PhysicsProcess(double delta)
	{
		// Если враг мертв, не выполняем логику движения
		if (IsDead) return;

		float fDelta = (float)delta;

		if (_state == State.Walking)
		{
			Vector2 vel = Velocity;
			float offset = GlobalPosition.X - _anchorPosition.X;

			if ((_direction == 1 && offset >= PatrolDistance) || (_direction == -1 && offset <= 0))
			{
				StartWaiting();
				return;
			}

			vel.X = _direction * Speed;
			if (!IsOnFloor()) vel.Y += (float)ProjectSettings.GetSetting("physics/2d/default_gravity") * fDelta;

			Velocity = vel;
			MoveAndSlide();
		}
		else
		{
			_timer -= fDelta;
			if (_timer <= 0) StartWalking();
		}
	}

	private void StartWaiting()
	{
		_state = State.Waiting;
		_timer = WaitTime;
		_sprite.Play("idle");
		_direction *= -1;
	}

	private void StartWalking()
	{
		_state = State.Walking;
		_sprite.Play("walk");
		_sprite.FlipH = _direction < 0;
	}

	// Переопределяем Die, если нужно специфичное поведение для таракана,
	// но базовый класс теперь справляется сам.
}
