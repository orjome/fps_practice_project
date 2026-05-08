using Sandbox;
using System;

public sealed class WaveManager : Component
{
	[Property] public EnemySpawner EnemySpawner { get; set; }

	[Property] public int StartingEnemyCount { get; set; } = 3;
	[Property] public int EnemiesAddedPerWave { get; set; } = 2;

	[Property] public int StartingMaxActiveEnemies { get; set; } = 2;
	[Property] public int MaxActiveIncreaseEveryWaves { get; set; } = 2;

	[Property] public float StartingSpawnInterval { get; set; } = 2.5f;
	[Property] public float SpawnIntervalDecreasePerWave { get; set; } = 0.15f;
	[Property] public float MinimumSpawnInterval { get; set; } = 0.75f;

	[Property] public float TimeBetweenWaves { get; set; } = 5f;

	private int currentWave;
	private bool waitingForNextWave;
	private float nextWaveStartTime;

	public int CurrentWave => currentWave;
	public bool WaitingForNextWave => waitingForNextWave;
	public float SecondsUntilNextWave => waitingForNextWave ? MathF.Max( 0f, nextWaveStartTime - Time.Now ) : 0f;

	protected override void OnStart()
	{
		EnemySpawner ??= GetComponent<EnemySpawner>();

		if ( EnemySpawner is null )
		{
			Log.Warning( "WaveManager needs an EnemySpawner reference." );
			return;
		}

		EnemySpawner.OnWaveCompleted += HandleWaveCompleted;

		StartNextWave();
	}

	protected override void OnDisabled()
	{
		if ( EnemySpawner is not null )
		{
			EnemySpawner.OnWaveCompleted -= HandleWaveCompleted;
		}
	}

	protected override void OnUpdate()
	{
		if ( !waitingForNextWave )
			return;

		if ( Time.Now < nextWaveStartTime )
			return;

		StartNextWave();
	}

	private void StartNextWave()
	{
		if ( EnemySpawner is null )
			return;

		waitingForNextWave = false;
		currentWave++;

		int enemiesToSpawn = CalculateEnemyCountForWave( currentWave );
		int maxActiveEnemies = CalculateMaxActiveEnemiesForWave( currentWave );
		float spawnInterval = CalculateSpawnIntervalForWave( currentWave );

		Log.Info( $"Starting Wave {currentWave}. Enemies: {enemiesToSpawn}, Max Active: {maxActiveEnemies}, Interval: {spawnInterval}" );

		EnemySpawner.StartWave( currentWave, enemiesToSpawn, maxActiveEnemies, spawnInterval );
	}

	private void HandleWaveCompleted()
	{
		waitingForNextWave = true;
		nextWaveStartTime = Time.Now + TimeBetweenWaves;

		Log.Info( $"Wave {currentWave} complete. Next wave begins in {TimeBetweenWaves} seconds." );
	}

	private int CalculateEnemyCountForWave( int wave )
	{
		return StartingEnemyCount + (wave - 1) * EnemiesAddedPerWave;
	}

	private int CalculateMaxActiveEnemiesForWave( int wave )
	{
		int increases = (wave - 1) / Math.Max( 1, MaxActiveIncreaseEveryWaves );
		return StartingMaxActiveEnemies + increases;
	}

	private float CalculateSpawnIntervalForWave( int wave )
	{
		float interval = StartingSpawnInterval - (wave - 1) * SpawnIntervalDecreasePerWave;
		return MathF.Max( MinimumSpawnInterval, interval );
	}
	public void StopWaves()
	{
		waitingForNextWave = false;

		if ( EnemySpawner is not null )
		{
			EnemySpawner.StopSpawning( true );
		}

		Log.Info( "Waves stopped." );
	}

	public void RestartFromWaveOne()
	{
		waitingForNextWave = false;
		currentWave = 0;

		if ( EnemySpawner is not null )
		{
			EnemySpawner.StopSpawning( true );
		}

		StartNextWave();

		Log.Info( "Waves restarted from Wave 1." );
	}
}
