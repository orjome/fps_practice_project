using Sandbox;
using System;

public sealed class SimpleCrosshair : Component
{
	[Property] public float LineLength { get; set; } = 8f;
	[Property] public float BaseGap { get; set; } = 5f;
	[Property] public float Thickness { get; set; } = 2f;

	[Property] public float MaxExtraGap { get; set; } = 18f;
	[Property] public float RecoverySpeed { get; set; } = 45f;

	private float extraGap;

	protected override void OnUpdate()
	{
		RecoverCrosshair();
		DrawCrosshair();
	}

	public void Pulse( float amount )
	{
		extraGap += amount;
		extraGap = MathF.Min( extraGap, MaxExtraGap );
	}

	private void RecoverCrosshair()
	{
		if ( extraGap <= 0f )
			return;

		extraGap -= RecoverySpeed * Time.Delta;
		extraGap = MathF.Max( extraGap, 0f );
	}

	private void DrawCrosshair()
	{
		if ( Scene.Camera is null )
			return;

		var hud = Scene.Camera.Hud;

		float centerX = Screen.Width * 0.5f;
		float centerY = Screen.Height * 0.5f;

		float gap = BaseGap + extraGap;

		hud.DrawLine(
			new Vector2( centerX - gap - LineLength, centerY ),
			new Vector2( centerX - gap, centerY ),
			Thickness,
			Color.White
		);

		hud.DrawLine(
			new Vector2( centerX + gap, centerY ),
			new Vector2( centerX + gap + LineLength, centerY ),
			Thickness,
			Color.White
		);

		hud.DrawLine(
			new Vector2( centerX, centerY - gap - LineLength ),
			new Vector2( centerX, centerY - gap ),
			Thickness,
			Color.White
		);

		hud.DrawLine(
			new Vector2( centerX, centerY + gap ),
			new Vector2( centerX, centerY + gap + LineLength ),
			Thickness,
			Color.White
		);
	}
}
