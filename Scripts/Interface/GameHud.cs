using DormShadowsGame.Scripts.Managers;
using Godot;
using System;

namespace DormShadowsGame.Scripts.Interface;

public partial class GameHud : CanvasLayer
{
	[ExportCategory("Pause menu")]
	[Export] private Control _pauseMenu;
	[Export] private string _mainManuPath = "res://Scenes/main_menu.tscn";
	[Export] private TextureButton _buttonContinue;
	[Export] private TextureButton _buttonMainMenu;
	[Export] private TextureButton _buttonQuit;

	[ExportCategory("Effects")]
	[Export] private ColorRect _blurOverlay;
	[Export] private float _blurIntensity = 2.5f;
	[Export] private float _animationDuration = 0.3f;

	[ExportCategory("InGameHud")]
	[Export] private HBoxContainer _healthBar;

	private Tween _fadeTween;

	public override void _Ready()
	{
		_pauseMenu.Hide();
		_pauseMenu.Modulate = new Color(1, 1, 1, 0);
		SetBlurAmount(0.0f);

		GameManager.Instance.Connect(GameManager.SignalName.HealthChanged, Callable.From<int>(UpdateHealthDisplay));
		UpdateHealthDisplay(GameManager.Instance.Health);

		ProcessMode = ProcessModeEnum.Always;

		_buttonContinue.Pressed += OnContinuePressed;
		_buttonMainMenu.Pressed += OnMainMenuPressed;
		_buttonQuit.Pressed += OnQuitPressed;
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			TogglePause();
		}
	}

	private void TogglePause()
	{
		bool isPausing = !GetTree().Paused;
		GetTree().Paused = isPausing;

		if (isPausing)
		{
			_pauseMenu.Show();
			AnimatePauseMenu(1.0f, _blurIntensity);
		}
		else
		{
			AnimatePauseMenu(0.0f, 0.0f, () => _pauseMenu.Hide());
		}
	}

	private void AnimatePauseMenu(float targetOpacity, float targetBlur, Action onComplete = null)
	{
		if (_fadeTween != null && _fadeTween.IsRunning())
		{
			_fadeTween.Kill();
		}

		_fadeTween = CreateTween();
		_fadeTween.SetPauseMode(Tween.TweenPauseMode.Process);
		_fadeTween.SetParallel(true);

		_fadeTween.TweenProperty(_pauseMenu, "modulate:a", targetOpacity, _animationDuration)
			.SetTrans(Tween.TransitionType.Cubic)
			.SetEase(Tween.EaseType.Out);

		if (_blurOverlay?.Material is ShaderMaterial sm)
		{
			_fadeTween.TweenProperty(sm, "shader_parameter/blur_amount", targetBlur, _animationDuration)
				.SetTrans(Tween.TransitionType.Cubic)
				.SetEase(Tween.EaseType.Out);
		}

		if (onComplete != null)
		{
			_fadeTween.Chain().Finished += () => onComplete();
		}
	}

	private void SetBlurAmount(float amount)
	{
		if (_blurOverlay?.Material is ShaderMaterial sm)
		{
			sm.SetShaderParameter("blur_amount", amount);
		}
	}

	private void UpdateHealthDisplay(int currentHealth)
	{
		int i = 0;
		foreach (var child in _healthBar.GetChildren())
		{
			if (child is CanvasItem heart)
			{
				heart.Visible = i < currentHealth;
				i++;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (GameManager.Instance != null)
			GameManager.Instance.Disconnect(GameManager.SignalName.HealthChanged, Callable.From<int>(UpdateHealthDisplay));
		base.Dispose(disposing);
	}

	private void OnContinuePressed() => TogglePause();

	private void OnMainMenuPressed()
	{
		GetTree().Paused = false;
		SceneTransition.Instance.FadeToScene(_mainManuPath);
	}

	private void OnQuitPressed() => GetTree().Quit();
}
