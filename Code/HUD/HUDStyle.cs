using Sandbox;
using Sandbox.Rendering;

public static class HudStyle
{
	public static readonly Color PanelBackground = new Color( 0f, 0f, 0f, 0.55f );
	public static readonly Color PanelBorder = new Color( 1f, 1f, 1f, 0.12f );

	public static readonly Color TextPrimary = Color.White;
	public static readonly Color TextMuted = new Color( 1f, 1f, 1f, 0.65f );

	public static readonly Color Good = new Color( 0.35f, 1f, 0.45f, 1f );
	public static readonly Color Warning = new Color( 1f, 0.85f, 0.25f, 1f );
	public static readonly Color Danger = new Color( 1f, 0.25f, 0.25f, 1f );
	public static readonly Color Accent = new Color( 0.35f, 0.75f, 1f, 1f );

	public static void DrawPanel( HudPainter hud, Rect rect, float cornerRadius = 10f )
	{
		hud.DrawRect(
			rect,
			PanelBackground,
			new Vector4( cornerRadius, cornerRadius, cornerRadius, cornerRadius ),
			new Vector4( 1f, 1f, 1f, 1f ),
			PanelBorder
		);
	}
}
