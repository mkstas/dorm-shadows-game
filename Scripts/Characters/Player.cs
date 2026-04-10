using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class Player : CharacterBody2D
{
	[Export] private AnimatedSprite2D _sprite;

	[ExportGroup("Movement")]
	[Export] public float Speed { get; set; } = 80.0f;
	[Export] public float Acceleration { get; set; } = 1600.0f;
	[Export] public float Friction { get; set; } = 1800.0f;

	[ExportGroup("Jump Settings")]
	[Export] public float JumpVelocity { get; set; } = -140.0f;
	[Export] public float MaxJumpVelocity { get; set; } = -150.0f;
	[Export] public float JumpHoldTime { get; set; } = 0.2f;
	[Export] public float JumpCutMultiplier { get; set; } = 0.4f;

	private bool _isInvulnerable;
	private bool _movementEnabled = true;
	private float _jumpHoldTimer;
	private bool _isJumping;
	private float _lastFacingDirection = 1.0f;

	public override void _Ready()
	{
		AddToGroup("player");
		if (GameManager.Instance.PlayerSpawnPosition != Vector2.Zero)
		{
			GlobalPosition = GameManager.Instance.PlayerSpawnPosition;
			GameManager.Instance.PlayerSpawnPosition = Vector2.Zero;
			_sprite.Play("idle");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_movementEnabled) return;

		float fDelta = (float)delta;
		Vector2 velocity = Velocity;

		if (!IsOnFloor())
		{
			float gravityMult = (_isJumping && Input.IsActionPressed("move_jump") && velocity.Y < 0) ? 0.4f : 1.0f;
			velocity.Y += GameManager.Instance.Gravity * gravityMult * fDelta;
		}

		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			_isJumping = true;
			_jumpHoldTimer = 0f;
		}

		if (_isJumping)
		{
			if (Input.IsActionPressed("move_jump") && _jumpHoldTimer < JumpHoldTime)
			{
				_jumpHoldTimer += fDelta;
				float t = Mathf.Min(_jumpHoldTimer / JumpHoldTime, 1.0f);
				float smoothT = t * t * (3 - 2 * t);
				velocity.Y = Mathf.Lerp(JumpVelocity, MaxJumpVelocity, smoothT);
			}
			else
			{
				_isJumping = false;
			}

			if (Input.IsActionJustReleased("move_jump") && velocity.Y < 0)
			{
				velocity.Y *= JumpCutMultiplier;
				_isJumping = false;
			}
		}

		float axis = Input.GetAxis("move_left", "move_right");
		float targetSpeed = axis * Speed;
		velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, (axis != 0 ? Acceleration : Friction) * fDelta);

		Velocity = velocity;

		MoveAndSlide();

		if (IsOnCeiling())
		{
			if (Velocity.Y < 0)
			{
				Velocity = new Vector2(Velocity.X, 0);
			}
			_isJumping = false;
		}

		UpdateAnimation(axis);
	}

	private void UpdateAnimation(float axis)
	{
		if (axis != 0) _lastFacingDirection = axis;
		_sprite.FlipH = _lastFacingDirection < 0;

		string anim = IsOnFloor() ? (Mathf.Abs(Velocity.X) > 0.1f ? "walk" : "idle") : "jump";
		if (_sprite.Animation != anim) _sprite.Play(anim);
	}

	public void TakeDamage(int amount, Vector2 sourcePos)
	{
		if (_isInvulnerable) return;

		GameManager.Instance.Health -= amount;
		ApplyKnockback(sourcePos);
		FlashEffect();
	}

	private void ApplyKnockback(Vector2 sourcePos)
	{
		float dir = GlobalPosition.X > sourcePos.X ? 1 : -1;
		Velocity = new Vector2(dir * 250f, -150f);
	}

	private async void FlashEffect()
	{
		_isInvulnerable = true;
		using var tween = CreateTween();
		tween.SetLoops(6);
		tween.TweenProperty(_sprite, "modulate:a", 0f, 0.1f);
		tween.TweenProperty(_sprite, "modulate:a", 1f, 0.1f);

		await ToSignal(GetTree().CreateTimer(1.2f), SceneTreeTimer.SignalName.Timeout);
		_isInvulnerable = false;
	}

	public void SetMovementEnabled(bool enabled) => _movementEnabled = enabled;
}
