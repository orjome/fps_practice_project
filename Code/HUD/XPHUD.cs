using Sandbox;

public sealed class XpHud : Component
{
	[Property] public XpManager XpManager { get; set; }

	[Property] public float TextSize { get; set; } = 24f;
	[Property] public float LeftOffset { get; set; } = 40f;
	[Property] public float TopOffset { get; set; } = 40f;

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

		string levelText = $"LEVEL {XpManager.Level}";
		string xpText = $"XP {XpManager.CurrentXP} / {XpManager.XPToNextLevel}";

		hud.DrawText(
			levelText,
			TextSize,
			Color.White,
			new Vector2( x, y ),
			TextFlag.LeftTop
		);

		hud.DrawText(
			xpText,
			TextSize * 0.75f,
			Color.White,
			new Vector2( x, y + 32f ),
			TextFlag.LeftTop
		);
	}
}
