using Sandbox;
using System;
using System.Collections.Generic;

public sealed class DamageNumberHud : Component
{
	[Property] public float Lifetime { get; set; } = 0.65f;
	[Property] public float TextSize { get; set; } = 28f;
	[Property] public float FloatSpeed { get; set; } = 55f;
	[Property] public float RandomOffset { get; set; } = 35f;

	private readonly List<DamagePopup> popups = new();

	private struct DamagePopup
	{
		public string Text;
		public Vector2 Position;
		public Vector2 Velocity;
		public float Age;
		public float Lifetime;
	}

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		UpdatePopups();
		DrawPopups();
	}

	public void ShowDamage( float damageAmount )
	{
		float centerX = Screen.Width * 0.5f;
		float centerY = Screen.Height * 0.5f;

		float offsetX = RandomRange( -RandomOffset, RandomOffset );
		float offsetY = RandomRange( -RandomOffset * 0.5f, RandomOffset * 0.5f );

		var popup = new DamagePopup
		{
			Text = MathF.Ceiling( damageAmount ).ToString(),
			Position = new Vector2( centerX + offsetX, centerY - 45f + offsetY ),
			Velocity = new Vector2( 0f, -FloatSpeed ),
			Age = 0f,
			Lifetime = Lifetime
		};

		popups.Add( popup );
	}

	private void UpdatePopups()
	{
		for ( int i = popups.Count - 1; i >= 0; i-- )
		{
			var popup = popups[i];

			popup.Age += Time.Delta;
			popup.Position += popup.Velocity * Time.Delta;

			if ( popup.Age >= popup.Lifetime )
			{
				popups.RemoveAt( i );
				continue;
			}

			popups[i] = popup;
		}
	}

	private void DrawPopups()
	{
		var hud = Scene.Camera.Hud;

		foreach ( var popup in popups )
		{
			float lifePercent = popup.Age / MathF.Max( popup.Lifetime, 0.01f );
			float alpha = 1f - lifePercent;

			var color = new Color( 1f, 1f, 1f, alpha );

			hud.DrawText(
				popup.Text,
				TextSize,
				color,
				popup.Position,
				TextFlag.Center
			);
		}
	}

	private static float RandomRange( float min, float max )
	{
		return min + Random.Shared.NextSingle() * (max - min);
	}
}
