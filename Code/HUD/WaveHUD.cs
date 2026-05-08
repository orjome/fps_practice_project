using Sandbox;
using System;

public sealed class WaveHud : Component
{
	[Property] public WaveManager WaveManager { get; set; }
	[Property] public EnemySpawner EnemySpawner { get; set; }

	[Property] public float RightOffset { get; set; } = 40f;
	[Property] public float TopOffset { get; set; } = 40f;

	[Property] public float PanelWidth { get; set; } = 250f;
	[Property] public float PanelHeight { get; set; } = 86f;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		if ( WaveManager is null || EnemySpawner is null )
			return;

		DrawWaveInfo();
	}

	private void DrawWaveInfo()
	{
		var hud = Scene.Camera.Hud;

		float x = Screen.Width - RightOffset - PanelWidth;
		float y = TopOffset;

		var panelRect = new Rect( x, y, PanelWidth, PanelHeight );
		HudStyle.DrawPanel( hud, panelRect );

		string waveText = $"WAVE {WaveManager.CurrentWave}";
		string infoText;

		if ( WaveManager.WaitingForNextWave )
		{
			infoText = $"NEXT WAVE IN {MathF.Ceiling( WaveManager.SecondsUntilNextWave )}";
		}
		else
		{
			infoText = $"ENEMIES {EnemySpawner.TotalKilledThisWave} / {EnemySpawner.TotalToSpawnThisWave}";
		}

		hud.DrawText(
			waveText,
			26f,
			HudStyle.Warning,
			new Vector2( x + PanelWidth - 18f, y + 16f ),
			TextFlag.RightTop
		);

		hud.DrawText(
			infoText,
			17f,
			HudStyle.TextMuted,
			new Vector2( x + PanelWidth - 18f, y + 50f ),
			TextFlag.RightTop
		);
	}
}
