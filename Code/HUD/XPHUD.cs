using Sandbox;
using Sandbox.Rendering;
using System;

public sealed class XpHud : Component
{
	[Property] public XpManager XpManager { get; set; }

	[Property] public float LeftOffset { get; set; } = 40f;
	[Property] public float TopOffset { get; set; } = 40f;

	[Property] public float PanelWidth { get; set; } = 230f;
	[Property] public float PanelHeight { get; set; } = 86f;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		if ( XpManager is null )
			return;

		DrawXp();
	}

	private void DrawXp()
	{
		var hud = Scene.Camera.Hud;

		float x = LeftOffset;
		float y = TopOffset;

		var panelRect = new Rect( x, y, PanelWidth, PanelHeight );
		HudStyle.DrawPanel( hud, panelRect );

		float xpPercent = XpManager.CurrentXP / MathF.Max( XpManager.XPToNextLevel, 1f );

		hud.DrawText(
			$"LEVEL {XpManager.Level}",
			24f,
			HudStyle.Accent,
			new Vector2( x + 18f, y + 16f ),
			TextFlag.LeftTop
		);

		hud.DrawText(
			$"XP {XpManager.CurrentXP} / {XpManager.XPToNextLevel}",
			17f,
			HudStyle.TextMuted,
			new Vector2( x + 18f, y + 46f ),
			TextFlag.LeftTop
		);

		DrawXpBar( hud, x + 18f, y + PanelHeight - 14f, PanelWidth - 36f, 5f, xpPercent );
	}

	private void DrawXpBar( HudPainter hud, float x, float y, float width, float height, float percent )
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
			HudStyle.Accent,
			new Vector4( 3f, 3f, 3f, 3f ),
			Vector4.Zero,
			Color.Transparent
		);
	}
}
