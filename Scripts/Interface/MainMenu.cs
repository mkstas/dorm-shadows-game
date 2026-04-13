using DormShadowsGame.Scripts.Managers;
using Godot;

namespace DormShadowsGame.Scripts.Interface;

public partial class MainMenu : Control
{
	[Export] private TextureButton _buttonPlay;
	[Export] private TextureButton _buttonQuit;
	[Export] private string _entryLevelPath = "res://Scenes/Levels/entrance.tscn";

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

	private void OnPlayPressed() => SceneTransition.Instance.FadeToScene(_entryLevelPath);
	private void OnQuitPressed() => GetTree().Quit();
}
