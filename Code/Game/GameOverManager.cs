using Sandbox;

public sealed class GameOverManager : Component
{
	[Property] public PlayerHealth PlayerHealth { get; set; }
	[Property] public PlayerController PlayerController { get; set; }
	[Property] public WeaponManager WeaponManager { get; set; }

	[Property] public WaveManager WaveManager { get; set; }
	[Property] public XpManager XpManager { get; set; }

	[Property] public string RestartInput { get; set; } = "use";

	[Property] public float TitleTextSize { get; set; } = 64f;
	[Property] public float InfoTextSize { get; set; } = 28f;

	private bool isGameOver;

	public bool IsGameOver => isGameOver;

	protected override void OnStart()
	{
		if ( PlayerHealth is not null )
		{
			PlayerHealth.OnDied += HandlePlayerDied;
		}
		else
		{
			Log.Warning( "GameOverManager needs a PlayerHealth reference." );
		}
	}

	protected override void OnDisabled()
	{
		if ( PlayerHealth is not null )
		{
			PlayerHealth.OnDied -= HandlePlayerDied;
		}
	}

	protected override void OnUpdate()
	{
		if ( !isGameOver )
			return;

		DrawGameOverHud();

		if ( Input.Pressed( RestartInput ) )
		{
			RestartRun();
		}
	}

	private void HandlePlayerDied( PlayerHealth playerHealth )
	{
		StartGameOver();
	}

	private void StartGameOver()
	{
		if ( isGameOver )
			return;

		isGameOver = true;

		Log.Info( "GAME OVER." );

		if ( WaveManager is not null )
		{
			WaveManager.StopWaves();
		}

		if ( WeaponManager is not null )
		{
			WeaponManager.Enabled = false;
		}

		if ( PlayerController is not null )
		{
			PlayerController.Enabled = false;
		}
	}

	private void RestartRun()
	{
		Log.Info( "Restarting run." );

		isGameOver = false;

		if ( XpManager is not null )
		{
			XpManager.ResetXP();
		}

		if ( PlayerHealth is not null )
		{
			PlayerHealth.ResetPlayerForNewRun();
		}

		if ( WeaponManager is not null )
		{
			WeaponManager.Enabled = true;
			WeaponManager.ResetCurrentWeapon();
		}

		if ( PlayerController is not null )
		{
			PlayerController.Enabled = true;
		}

		if ( WaveManager is not null )
		{
			WaveManager.RestartFromWaveOne();
		}
	}

	private void DrawGameOverHud()
	{
		if ( Scene.Camera is null )
			return;

		var hud = Scene.Camera.Hud;

		float centerX = Screen.Width * 0.5f;
		float centerY = Screen.Height * 0.5f;

		hud.DrawText(
			"GAME OVER",
			TitleTextSize,
			Color.Red,
			new Vector2( centerX, centerY - 60f ),
			TextFlag.Center
		);

		hud.DrawText(
			"Press E to Restart",
			InfoTextSize,
			Color.White,
			new Vector2( centerX, centerY + 10f ),
			TextFlag.Center
		);
	}
}
