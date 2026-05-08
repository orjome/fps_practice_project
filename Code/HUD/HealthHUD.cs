using Sandbox;
using System;

public sealed class HealthHud : Component
{
	[Property] public PlayerHealth PlayerHealth { get; set; }

	[Property] public float TextSize { get; set; } = 28f;
	[Property] public float LeftOffset { get; set; } = 40f;
	[Property] public float BottomOffset { get; set; } = 40f;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		if ( PlayerHealth is null )
			return;

		DrawHealth();
	}

	private void DrawHealth()
	{
		var hud = Scene.Camera.Hud;

		float x = LeftOffset;
		float y = Screen.Height - BottomOffset;

		string healthText;

		if ( PlayerHealth.IsDead )
		{
			healthText = "DEAD - RESPAWNING";
		}
		else
		{
			healthText = $"HP: {MathF.Ceiling( PlayerHealth.CurrentHealth )}";
		}

		hud.DrawText(
			healthText,
			TextSize,
			Color.White,
			new Vector2( x, y ),
			TextFlag.LeftBottom
		);
	}
}
