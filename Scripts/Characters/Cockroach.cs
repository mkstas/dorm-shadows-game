using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class Cockroach : BaseEnemy
{
	private enum State { Walking, Waiting }

	[Export] private AnimatedSprite2D _sprite;
	[Export] public float Speed = 64.0f;
	[Export] public float PatrolDistance = 48.0f;
	[Export] public float WaitTime = 1.0f;

	// 1 для движения вправо от старта, -1 для движения влево от старта
	[Export] public int Direction = 1;

	private Vector2 _startPosition;
	private Vector2 _targetPosition;
	private Vector2 _currentTarget; // Точка, к которой идем сейчас

	private State _state = State.Walking;
	private float _timer;

	public override void _Ready()
	{
		base._Ready();
		_startPosition = GlobalPosition;

		// Рассчитываем конечную точку патруля
		_targetPosition = _startPosition + new Vector2(Direction * PatrolDistance, 0);

		// Начинаем идти к дальней точке
		_currentTarget = _targetPosition;

		UpdateVisuals();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (IsDead) return;

		float fDelta = (float)delta;

		if (_state == State.Walking)
		{
			Vector2 vel = Velocity;

			// Считаем расстояние до текущей цели по X
			float diff = _currentTarget.X - GlobalPosition.X;

			// Если мы дошли до цели (или проскочили её)
			if ((Direction == 1 && diff <= 0) || (Direction == -1 && diff >= 0))
			{
				StartWaiting();
				return;
			}

			vel.X = Direction * Speed;

			if (!IsOnFloor())
				vel.Y += (float)ProjectSettings.GetSetting("physics/2d/default_gravity") * fDelta;

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

		// Разворачиваем направление
		Direction *= -1;

		// Меняем текущую цель: если были в точке патруля — идем домой, и наоборот
		_currentTarget = (_currentTarget == _targetPosition) ? _startPosition : _targetPosition;
	}

	private void StartWalking()
	{
		_state = State.Walking;
		UpdateVisuals();
	}

	private void UpdateVisuals()
	{
		_sprite.Play("walk");
		_sprite.FlipH = Direction < 0;
	}
}
