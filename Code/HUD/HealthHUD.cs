using Sandbox;
using Sandbox.Rendering;
using System;

public sealed class HealthHud : Component
{
	[Property] public PlayerHealth PlayerHealth { get; set; }

	[Property] public float LeftOffset { get; set; } = 40f;
	[Property] public float BottomOffset { get; set; } = 40f;

	[Property] public float PanelWidth { get; set; } = 210f;
	[Property] public float PanelHeight { get; set; } = 92f;

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
		float y = Screen.Height - BottomOffset - PanelHeight;

		var panelRect = new Rect( x, y, PanelWidth, PanelHeight );
		HudStyle.DrawPanel( hud, panelRect );

		float health = MathF.Ceiling( PlayerHealth.CurrentHealth );
		float healthPercent = PlayerHealth.CurrentHealth / MathF.Max( PlayerHealth.MaxHealth, 1f );

		Color healthColor = HudStyle.Good;

		if ( healthPercent <= 0.30f )
		{
			healthColor = HudStyle.Danger;
		}
		else if ( healthPercent <= 0.60f )
		{
			healthColor = HudStyle.Warning;
		}

		string healthText = PlayerHealth.IsDead
			? "DOWN"
			: $"HP {health}";

		hud.DrawText(
			"HEALTH",
			18f,
			HudStyle.TextMuted,
			new Vector2( x + 18f, y + 18f ),
			TextFlag.LeftTop
		);

		hud.DrawText(
			healthText,
			34f,
			healthColor,
			new Vector2( x + 18f, y + PanelHeight - 18f ),
			TextFlag.LeftBottom
		);

		DrawHealthBar( hud, x + 18f, y + PanelHeight - 12f, PanelWidth - 36f, 5f, healthPercent, healthColor );
	}

	private void DrawHealthBar( HudPainter hud, float x, float y, float width, float height, float percent, Color fillColor )
	{
		percent = Math.Clamp( percent, 0f, 1f );

		var backgroundRect = new Rect( x, y, width, height );
		var fillRect = new Rect( x, y, width * percent, height );

		hud.DrawRect(
			backgroundRect,
			new Color( 1f, 1f, 1f, 0.12f ),
			new Vector4( 3f, 3f, 3f, 3f ),
			Vector4.Zero,
			Color.Transparent
		);

		hud.DrawRect(
			fillRect,
			fillColor,
			new Vector4( 3f, 3f, 3f, 3f ),
			Vector4.Zero,
			Color.Transparent
		);
	}
}
