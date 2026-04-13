using Godot;
using DormShadowsGame.Scripts.Characters;

namespace DormShadowsGame.Scripts.Managers;

public partial class SceneTransition : CanvasLayer
{
	public static SceneTransition Instance { get; private set; }
	private ColorRect _fadeRect;

	public override void _Ready()
	{
		Instance = this;
		Layer = 100;

		_fadeRect = new ColorRect
		{
			Color = new Color(0, 0, 0, 0),
			MouseFilter = Control.MouseFilterEnum.Ignore
		};
		_fadeRect.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		AddChild(_fadeRect);
	}

	public async void FadeToScene(string scenePath)
	{
		GameManager.Instance.IsSceneTransitioning = true;
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Stop;

		SetPlayerInput(false);

		var tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(_fadeRect, "color:a", 1.0f, 0.2f);

		await ToSignal(tween, Tween.SignalName.Finished);

		GetTree().ChangeSceneToFile(scenePath);

		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

		FadeIn();
	}

	private async void FadeIn()
	{
		var tween = CreateTween().SetTrans(Tween.TransitionType.Linear);
		tween.TweenProperty(_fadeRect, "color:a", 0.0f, 0.2f);

		await ToSignal(tween, Tween.SignalName.Finished);

		SetPlayerInput(true);
		_fadeRect.MouseFilter = Control.MouseFilterEnum.Ignore;
		GameManager.Instance.IsSceneTransitioning = false;
	}

	private void SetPlayerInput(bool enabled)
	{
		var playerNode = GetTree().GetFirstNodeInGroup("player");

		if (playerNode is Player player)
		{
			GameManager.Instance.PlayerMovementEnabled = enabled;
		}
	}
}
