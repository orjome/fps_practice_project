using Sandbox;

public sealed class HitmarkerHud : Component
{
	[Property] public float ShowTime { get; set; } = 0.12f;
	[Property] public float LineLength { get; set; } = 10f;
	[Property] public float Gap { get; set; } = 5f;
	[Property] public float Thickness { get; set; } = 2f;

	private float hitmarkerTimer;

	protected override void OnUpdate()
	{
		if ( hitmarkerTimer > 0f )
		{
			hitmarkerTimer -= Time.Delta;
			DrawHitmarker();
		}
	}

	public void ShowHitmarker()
	{
		hitmarkerTimer = ShowTime;
	}

	private void DrawHitmarker()
	{
		if ( Scene.Camera is null )
			return;

		var hud = Scene.Camera.Hud;

		float centerX = Screen.Width * 0.5f;
		float centerY = Screen.Height * 0.5f;

		var center = new Vector2( centerX, centerY );

		// Top-left line
		hud.DrawLine(
			new Vector2( center.x - Gap - LineLength, center.y - Gap - LineLength ),
			new Vector2( center.x - Gap, center.y - Gap ),
			Thickness,
			Color.Red
		);

		// Top-right line
		hud.DrawLine(
			new Vector2( center.x + Gap, center.y - Gap ),
			new Vector2( center.x + Gap + LineLength, center.y - Gap - LineLength ),
			Thickness,
			Color.Red
		);

		// Bottom-left line
		hud.DrawLine(
			new Vector2( center.x - Gap - LineLength, center.y + Gap + LineLength ),
			new Vector2( center.x - Gap, center.y + Gap ),
			Thickness,
			Color.Red
		);

		// Bottom-right line
		hud.DrawLine(
			new Vector2( center.x + Gap, center.y + Gap ),
			new Vector2( center.x + Gap + LineLength, center.y + Gap + LineLength ),
			Thickness,
			Color.Red
		);
	}
}
