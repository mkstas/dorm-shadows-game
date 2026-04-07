using Godot;

namespace DormShadowsGame.Scripts.Interface;

public partial class GameHud : CanvasLayer
{
	[Export] private HBoxContainer _healthBar;

	public override void _Ready()
	{
		GameManager.Instance.HealthChanged += UpdateHealthDisplay;
		UpdateHealthDisplay(GameManager.Instance.Health);
	}

	private void UpdateHealthDisplay(int currentHealth)
	{
		var hearts = _healthBar.GetChildren();
		for (int i = 0; i < hearts.Count; i++)
		{
			if (hearts[i] is CanvasItem heart)
			{
				heart.Visible = i < currentHealth;
			}
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (GameManager.Instance != null)
			GameManager.Instance.HealthChanged -= UpdateHealthDisplay;
		base.Dispose(disposing);
	}
}
