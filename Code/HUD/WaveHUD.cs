using Sandbox;
using System;

public sealed class WaveHud : Component
{
	[Property] public WaveManager WaveManager { get; set; }
	[Property] public EnemySpawner EnemySpawner { get; set; }

	[Property] public float TextSize { get; set; } = 24f;
	[Property] public float RightOffset { get; set; } = 40f;
	[Property] public float TopOffset { get; set; } = 40f;

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

		float x = Screen.Width - RightOffset;
		float y = TopOffset;

		string waveText = $"WAVE {WaveManager.CurrentWave}";
		string enemyText;

		if ( WaveManager.WaitingForNextWave )
		{
			enemyText = $"NEXT WAVE IN {MathF.Ceiling( WaveManager.SecondsUntilNextWave )}";
		}
		else
		{
			enemyText = $"ENEMIES {EnemySpawner.TotalKilledThisWave}/{EnemySpawner.TotalToSpawnThisWave}";
		}

		hud.DrawText(
			waveText,
			TextSize,
			Color.White,
			new Vector2( x, y ),
			TextFlag.RightTop
		);

		hud.DrawText(
			enemyText,
			TextSize * 0.75f,
			Color.White,
			new Vector2( x, y + 32f ),
			TextFlag.RightTop
		);
	}
}
