using Godot;

namespace DormShadowsGame.Scripts.Interface;

public partial class MainMenu : Control
{
	[Export] private TextureButton _buttonPlay;
	[Export] private TextureButton _buttonQuit;
	[Export] private PackedScene _startGameLevel;

	public override void _Ready()
	{
		_buttonPlay.Pressed += OnPlayPressed;
		_buttonQuit.Pressed += OnQuitPressed;
	}

	public override void _ExitTree()
	{
		_buttonPlay.Pressed -= OnPlayPressed;
		_buttonQuit.Pressed -= OnQuitPressed;
	}

	private void OnPlayPressed() => GetTree().ChangeSceneToPacked(_startGameLevel);
	private void OnQuitPressed() => GetTree().Quit();
}
