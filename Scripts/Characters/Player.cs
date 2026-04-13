using Godot;

namespace DormShadowsGame.Scripts.Characters;

public partial class Player : CharacterBody2D
{
	[Export] private AnimatedSprite2D _sprite;
	[Export] private RayCast2D _stompDetector;

	[ExportGroup("Movement")]
	[Export] public float Speed { get; set; } = 80.0f;
	[Export] public float Acceleration { get; set; } = 1600.0f;
	[Export] public float Friction { get; set; } = 1800.0f;

	[ExportGroup("Jump Settings")]
	[Export] public float JumpVelocity { get; set; } = -140.0f;
	[Export] public float MaxJumpVelocity { get; set; } = -150.0f;
	[Export] public float JumpHoldTime { get; set; } = 0.2f;
	[Export] public float JumpCutMultiplier { get; set; } = 0.4f;

	[ExportCategory("Bounce Settings")]
	[Export] public float KillsBounceMultiplier { get; set; } = 1.4f;
	[Export] public float HitBounceMultiplier { get; set; } = 0.8f;

	private bool _isInvulnerable;
	private float _jumpHoldTimer;
	private bool _isJumping;
	private float _lastFacingDirection = 1.0f;

	public override void _Ready()
	{
		AddToGroup("player");
		_stompDetector ??= GetNode<RayCast2D>("StompDetector");

		if (GameManager.Instance.PlayerSpawnPosition != Vector2.Zero)
		{
			GlobalPosition = GameManager.Instance.PlayerSpawnPosition;
			GameManager.Instance.PlayerSpawnPosition = Vector2.Zero;
			_sprite.Play("idle");
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (!GameManager.Instance.PlayerMovementEnabled) return;

		float fDelta = (float)delta;

		ApplyGravity(fDelta);
		HandleJump(fDelta);
		HandleHorizontalMovement(fDelta);

		CheckStomp();
		MoveAndSlide();
		HandleCeilingCollision();
		UpdateAnimation();
	}

	private void ApplyGravity(float delta)
	{
		if (IsOnFloor()) return;

		float gravityMult = (_isJumping && Input.IsActionPressed("move_jump") && Velocity.Y < 0) ? 0.4f : 1.0f;
		Velocity += Vector2.Down * GameManager.Instance.Gravity * gravityMult * delta;
	}

	private void HandleJump(float delta)
	{
		if (Input.IsActionJustPressed("move_jump") && IsOnFloor())
		{
			Velocity = new Vector2(Velocity.X, JumpVelocity);
			_isJumping = true;
			_jumpHoldTimer = 0f;
		}

		if (!_isJumping) return;

		if (Input.IsActionPressed("move_jump") && _jumpHoldTimer < JumpHoldTime)
		{
			_jumpHoldTimer += delta;
			float t = Mathf.Min(_jumpHoldTimer / JumpHoldTime, 1.0f);
			float smoothT = t * t * (3 - 2 * t);
			Velocity = new Vector2(Velocity.X, Mathf.Lerp(JumpVelocity, MaxJumpVelocity, smoothT));
		}
		else
		{
			_isJumping = false;
		}

		if (Input.IsActionJustReleased("move_jump") && Velocity.Y < 0)
		{
			Velocity = new Vector2(Velocity.X, Velocity.Y * JumpCutMultiplier);
			_isJumping = false;
		}
	}

	private void HandleHorizontalMovement(float delta)
	{
		float axis = Input.GetAxis("move_left", "move_right");
		float targetSpeed = axis * Speed;
		float accel = axis != 0 ? Acceleration : Friction;

		Velocity = new Vector2(Mathf.MoveToward(Velocity.X, targetSpeed, accel * delta), Velocity.Y);

		if (axis != 0) _lastFacingDirection = axis;
	}

	private void CheckStomp()
	{
		if (Velocity.Y <= 0 || !_stompDetector.IsColliding()) return;

		if (_stompDetector.GetCollider() is BaseEnemy enemy)
		{
			if (enemy.TakeDamage(1))
			{
				Bounce(JumpVelocity * 1.2f);
			}
			else
			{
				Bounce(JumpVelocity * 0.7f);
				StartBriefInvulnerability(0.2f);
			}
		}
	}

	public void Bounce(float force)
	{
		Velocity = new Vector2(Velocity.X, force);
		_isJumping = false;
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

	private async void StartBriefInvulnerability(float duration)
	{
		_isInvulnerable = true;
		await ToSignal(GetTree().CreateTimer(duration), SceneTreeTimer.SignalName.Timeout);
		_isInvulnerable = false;
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

	private void HandleCeilingCollision()
	{
		if (IsOnCeiling() && Velocity.Y < 0)
		{
			Velocity = new Vector2(Velocity.X, 0);
			_isJumping = false;
		}
	}

	private void UpdateAnimation()
	{
		_sprite.FlipH = _lastFacingDirection < 0;
		string anim = IsOnFloor() ? (Mathf.Abs(Velocity.X) > 0.1f ? "walk" : "idle") : "jump";

		if (_sprite.Animation != anim) _sprite.Play(anim);
	}
}
