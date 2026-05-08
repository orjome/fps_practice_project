using Sandbox;
using System;

public sealed class PlayerDamageFeedbackHud : Component
{
	[Property] public PlayerHealth PlayerHealth { get; set; }

	[Property] public float FlashStrength { get; set; } = 0.35f;
	[Property] public float FlashFadeSpeed { get; set; } = 2.8f;

	[Property] public bool ShowLowHealthWarning { get; set; } = true;
	[Property] public float LowHealthPercent { get; set; } = 0.30f;
	[Property] public float LowHealthPulseSpeed { get; set; } = 5f;
	[Property] public float LowHealthMaxAlpha { get; set; } = 0.18f;

	private float flashAlpha;

	protected override void OnStart()
	{
		if ( PlayerHealth is not null )
		{
			PlayerHealth.OnDamaged += HandlePlayerDamaged;
		}
		else
		{
			Log.Warning( "PlayerDamageFeedbackHud needs a PlayerHealth reference." );
		}
	}

	protected override void OnDisabled()
	{
		if ( PlayerHealth is not null )
		{
			PlayerHealth.OnDamaged -= HandlePlayerDamaged;
		}
	}

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		DrawDamageFlash();
		DrawLowHealthWarning();

		UpdateFlashFade();
	}

	private void HandlePlayerDamaged( float damageAmount, float currentHealth )
	{
		flashAlpha = MathF.Min( FlashStrength, flashAlpha + FlashStrength );
	}

	private void UpdateFlashFade()
	{
		if ( flashAlpha <= 0f )
			return;

		flashAlpha -= FlashFadeSpeed * Time.Delta;
		flashAlpha = MathF.Max( flashAlpha, 0f );
	}

	private void DrawDamageFlash()
	{
		if ( flashAlpha <= 0f )
			return;

		DrawFullScreenRedOverlay( flashAlpha );
	}

	private void DrawLowHealthWarning()
	{
		if ( !ShowLowHealthWarning )
			return;

		if ( PlayerHealth is null )
			return;

		if ( PlayerHealth.IsDead )
			return;

		float healthPercent = PlayerHealth.CurrentHealth / MathF.Max( PlayerHealth.MaxHealth, 1f );

		if ( healthPercent > LowHealthPercent )
			return;

		float pulse = (MathF.Sin( Time.Now * LowHealthPulseSpeed ) + 1f) * 0.5f;
		float alpha = pulse * LowHealthMaxAlpha;

		DrawFullScreenRedOverlay( alpha );
	}

	private void DrawFullScreenRedOverlay( float alpha )
	{
		if ( Scene.Camera is null )
			return;

		var hud = Scene.Camera.Hud;

		var screenRect = new Rect( 0f, 0f, Screen.Width, Screen.Height );
		var color = new Color( 1f, 0f, 0f, alpha );

		hud.DrawRect(
			screenRect,
			color,
			Vector4.Zero,
			Vector4.Zero,
			Color.Transparent
		);
	}
}
