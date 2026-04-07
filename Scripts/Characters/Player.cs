using Godot;
using System;

namespace DormShadowsGame.Scripts.Characters;

public partial class Player : CharacterBody2D
{
	[Export] private AnimatedSprite2D _animatedSprite;

	[ExportGroup("Movement")]
	[Export] public float Speed = 80.0f;
	[Export] public float Acceleration = 1600.0f;
	[Export] public float Friction = 1800.0f;

	[ExportGroup("Jump")]
	[Export] public float JumpVelocity = -250.0f;
	[Export] public float MaxJumpVelocity = -400.0f;
	[Export] public float JumpHoldTime = 0.2f;
	[Export] public float JumpCutMultiplier = 0.2f;

	[ExportGroup("Combat")]
	[Export] public float InvulnerabilityDuration = 1.5f;

	private bool _holdingJump;
	private float _jumpHoldTimer;
	private float _lastDirection = 1f;
	private bool _movementEnabled = true;
	private bool _isInvulnerable = false;

	public override void _Ready()
	{
		AddToGroup("player");
		ApplySpawnPosition();
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!_movementEnabled)
		{
			Velocity = Vector2.Zero;
			MoveAndSlide();
			return;
		}

		float fDelta = (float)delta;
		Vector2 velocity = Velocity;
		float direction = Input.GetAxis("move_left", "move_right");

		ApplyGravity(ref velocity, fDelta);
		HandleJump(ref velocity, fDelta);
		HandleHorizontalMovement(ref velocity, direction, fDelta);

		Velocity = velocity;
		MoveAndSlide();

		UpdateAnimation(direction);
	}

	public void SetMovementEnabled(bool enabled)
	{
		_movementEnabled = enabled;
	}

	public void TakeDamage(int amount, Vector2 sourcePosition)
	{
		if (_isInvulnerable) return;

		GameManager.Instance.TakeDamage(amount);

		ApplyKnockback(sourcePosition);
		BecomeInvulnerable(InvulnerabilityDuration);
	}

	private void ApplyKnockback(Vector2 sourcePosition)
	{
		float pushDir = GlobalPosition.X > sourcePosition.X ? 1 : -1;
		Velocity = new Vector2(pushDir * 250f, -150f);
	}

	private async void BecomeInvulnerable(float duration)
	{
		_isInvulnerable = true;

		int cycles = Mathf.Max(1, (int)(duration * 10));
		Tween tween = CreateTween().SetLoops(cycles);
		tween.TweenProperty(_animatedSprite, "modulate:a", 0.0f, 0.025f);
		tween.TweenProperty(_animatedSprite, "modulate:a", 1.0f, 0.025f);

		await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);

		_isInvulnerable = false;
		_animatedSprite.Modulate = new Color(1, 1, 1, 1);
	}

	private void ApplySpawnPosition()
	{
		if (GameManager.Instance.PlayerSpawnPosition != Vector2.Zero)
		{
			GlobalPosition = GameManager.Instance.PlayerSpawnPosition;
			GameManager.Instance.PlayerSpawnPosition = Vector2.Zero;
		}
	}

	private void ApplyGravity(ref Vector2 velocity, float delta)
	{
		if (IsOnFloor()) return;
		float gravityScale = (_holdingJump && velocity.Y < 0 && _jumpHoldTimer < JumpHoldTime) ? 0.45f : 1.0f;
		velocity.Y += GameManager.Instance.Gravity * gravityScale * delta;
		if (_holdingJump && velocity.Y < 0) _jumpHoldTimer += delta;
	}

	private void HandleJump(ref Vector2 velocity, float delta)
	{
		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
		{
			velocity.Y = JumpVelocity;
			_holdingJump = true;
			_jumpHoldTimer = 0f;
		}
		if (_holdingJump && Input.IsActionPressed("move_jump") && velocity.Y < 0)
		{
			float t = Mathf.Clamp(_jumpHoldTimer / JumpHoldTime, 0, 1);
			velocity.Y = Mathf.Lerp(JumpVelocity, MaxJumpVelocity, t);
		}
		if (Input.IsActionJustReleased("move_jump") && velocity.Y < 0)
		{
			velocity.Y *= JumpCutMultiplier;
			_holdingJump = false;
		}
		if (IsOnFloor()) _holdingJump = false;
	}

	private void HandleHorizontalMovement(ref Vector2 velocity, float direction, float delta)
	{
		float targetSpeed = direction * Speed;
		float weight = (direction != 0) ? Acceleration : Friction;
		velocity.X = Mathf.MoveToward(velocity.X, targetSpeed, weight * delta);
		if (direction != 0) _lastDirection = direction;
	}

	private void UpdateAnimation(float direction)
	{
		string anim = IsOnFloor() ? (Mathf.Abs(Velocity.X) > 1f ? "walk" : "idle") : "jump";
		_animatedSprite.Play(anim);
		float lookDir = direction != 0 ? direction : _lastDirection;
		_animatedSprite.FlipH = lookDir < 0;
	}
}
