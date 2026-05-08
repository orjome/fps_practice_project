using Sandbox;
using System;
using System.Collections.Generic;

public sealed class EnemySpawner : Component
{
	[Property] public GameObject Target { get; set; }
	[Property] public XpManager XpManager { get; set; }

	[Property] public List<GameObject> SpawnPoints { get; set; } = new();

	[Property, Group( "Enemy Types" )]
	public List<EnemyData> EnemyTypes { get; set; } = new();

	private readonly List<GameObject> activeEnemies = new();

	private bool waveRunning;
	private int currentWave;
	private int totalToSpawnThisWave;
	private int totalSpawnedThisWave;
	private int totalKilledThisWave;
	private int maxActiveEnemies;
	private float spawnInterval;
	private float nextSpawnTime;
	private int nextSpawnPointIndex;
	private int totalSpawnedEver;

	public event Action OnWaveCompleted;

	public bool IsWaveRunning => waveRunning;
	public int CurrentWave => currentWave;
	public int ActiveEnemyCount => activeEnemies.Count;
	public int TotalToSpawnThisWave => totalToSpawnThisWave;
	public int TotalSpawnedThisWave => totalSpawnedThisWave;
	public int TotalKilledThisWave => totalKilledThisWave;
	public int RemainingToSpawn => totalToSpawnThisWave - totalSpawnedThisWave;

	protected override void OnUpdate()
	{
		CleanActiveEnemyList();

		if ( !waveRunning )
			return;

		TrySpawnEnemyForWave();
		CheckForWaveComplete();
	}

	public void StartWave( int waveNumber, int enemiesToSpawn, int maxActive, float interval )
	{
		if ( Target is null )
		{
			Log.Warning( "EnemySpawner needs a Target assigned." );
			return;
		}

		if ( !HasAvailableEnemyTypeForWave( waveNumber ) )
		{
			Log.Warning( $"EnemySpawner has no valid enemy type available for wave {waveNumber}." );
			return;
		}

		CleanActiveEnemyList();

		currentWave = Math.Max( 1, waveNumber );
		totalToSpawnThisWave = Math.Max( 0, enemiesToSpawn );
		totalSpawnedThisWave = 0;
		totalKilledThisWave = 0;
		maxActiveEnemies = Math.Max( 1, maxActive );
		spawnInterval = Math.Max( 0.1f, interval );

		waveRunning = true;
		nextSpawnTime = Time.Now;

		Log.Info( $"Wave {currentWave} started. Enemies: {totalToSpawnThisWave}, Max Active: {maxActiveEnemies}, Spawn Interval: {spawnInterval}" );
	}

	private void TrySpawnEnemyForWave()
	{
		if ( totalSpawnedThisWave >= totalToSpawnThisWave )
			return;

		if ( activeEnemies.Count >= maxActiveEnemies )
			return;

		if ( Time.Now < nextSpawnTime )
			return;

		SpawnEnemy();

		nextSpawnTime = Time.Now + spawnInterval;
	}

	private void SpawnEnemy()
	{
		EnemyData enemyData = PickEnemyDataForCurrentWave();

		if ( enemyData is null || enemyData.EnemyPrefab is null )
		{
			Log.Warning( $"Could not pick valid enemy data for wave {currentWave}." );
			return;
		}

		Vector3 spawnPosition = GetNextSpawnPosition();

		GameObject enemy = enemyData.EnemyPrefab.Clone( spawnPosition );

		totalSpawnedEver++;
		totalSpawnedThisWave++;

		enemy.Name = $"{enemyData.DisplayName}_{totalSpawnedEver}";

		activeEnemies.Add( enemy );

		SetupSpawnedEnemy( enemy, enemyData );

		Log.Info( $"Spawned {enemy.Name}. Wave spawned: {totalSpawnedThisWave}/{totalToSpawnThisWave}. Active: {activeEnemies.Count}/{maxActiveEnemies}" );
	}

	private void SetupSpawnedEnemy( GameObject enemy, EnemyData enemyData )
	{
		var mover = enemy.Components.Get<EnemyMover>( FindMode.EverythingInSelfAndDescendants );

		if ( mover is not null )
		{
			mover.Target = Target;
			ApplyMoverData( mover, enemyData );
		}
		else
		{
			Log.Warning( $"{enemy.Name} spawned without EnemyMover." );
		}

		var health = enemy.Components.Get<EnemyHealth>( FindMode.EverythingInSelfAndDescendants );

		if ( health is not null )
		{
			health.RespawnOnDeath = false;

			if ( XpManager is not null )
			{
				health.XpManager = XpManager;
			}

			ApplyHealthData( health, enemyData );

			health.OnDied += HandleEnemyDied;
		}
		else
		{
			Log.Warning( $"{enemy.Name} spawned without EnemyHealth." );
		}
	}

	private void ApplyHealthData( EnemyHealth health, EnemyData enemyData )
	{
		float scaledHealth = enemyData.GetHealthForWave( currentWave );
		int scaledXp = enemyData.GetXpRewardForWave( currentWave );

		health.SetMaxHealth( scaledHealth, true );
		health.SetXpReward( scaledXp );

		Log.Info( $"{health.GameObject.Name} health: {scaledHealth}, XP reward: {scaledXp}" );
	}

	private void ApplyMoverData( EnemyMover mover, EnemyData enemyData )
	{
		float scaledSpeed = enemyData.GetMoveSpeedForWave( currentWave );
		float scaledAttackDamage = enemyData.GetAttackDamageForWave( currentWave );

		mover.MoveSpeed = scaledSpeed;
		mover.AttackDamage = scaledAttackDamage;

		Log.Info( $"{mover.GameObject.Name} speed: {scaledSpeed}, attack damage: {scaledAttackDamage}" );
	}

	private EnemyData PickEnemyDataForCurrentWave()
	{
		var availableTypes = GetAvailableEnemyTypesForWave( currentWave );

		if ( availableTypes.Count == 0 )
			return null;

		float totalWeight = 0f;

		foreach ( var enemyType in availableTypes )
		{
			totalWeight += MathF.Max( 0f, enemyType.SpawnWeight );
		}

		if ( totalWeight <= 0f )
			return availableTypes[0];

		float randomValue = Random.Shared.NextSingle() * totalWeight;
		float runningWeight = 0f;

		foreach ( var enemyType in availableTypes )
		{
			runningWeight += MathF.Max( 0f, enemyType.SpawnWeight );

			if ( randomValue <= runningWeight )
			{
				return enemyType;
			}
		}

		return availableTypes[^1];
	}

	private List<EnemyData> GetAvailableEnemyTypesForWave( int waveNumber )
	{
		var availableTypes = new List<EnemyData>();

		foreach ( var enemyType in EnemyTypes )
		{
			if ( enemyType is null )
				continue;

			if ( enemyType.EnemyPrefab is null )
				continue;

			if ( waveNumber < enemyType.MinWave )
				continue;

			if ( enemyType.SpawnWeight <= 0f )
				continue;

			availableTypes.Add( enemyType );
		}

		return availableTypes;
	}

	private bool HasAvailableEnemyTypeForWave( int waveNumber )
	{
		return GetAvailableEnemyTypesForWave( waveNumber ).Count > 0;
	}

	private Vector3 GetNextSpawnPosition()
	{
		if ( SpawnPoints is null || SpawnPoints.Count == 0 )
			return WorldPosition;

		for ( int i = 0; i < SpawnPoints.Count; i++ )
		{
			GameObject spawnPoint = SpawnPoints[nextSpawnPointIndex % SpawnPoints.Count];
			nextSpawnPointIndex++;

			if ( spawnPoint.IsValid() )
			{
				return spawnPoint.WorldPosition;
			}
		}

		return WorldPosition;
	}

	private void HandleEnemyDied( EnemyHealth enemyHealth )
	{
		if ( enemyHealth is null || !enemyHealth.IsValid() )
			return;

		enemyHealth.OnDied -= HandleEnemyDied;

		GameObject enemyObject = enemyHealth.GameObject;

		activeEnemies.RemoveAll( enemy => !enemy.IsValid() || enemy == enemyObject );

		totalKilledThisWave++;

		Log.Info( $"Enemy killed. Wave kills: {totalKilledThisWave}/{totalToSpawnThisWave}. Active: {activeEnemies.Count}/{maxActiveEnemies}" );
	}

	private void CheckForWaveComplete()
	{
		if ( totalSpawnedThisWave < totalToSpawnThisWave )
			return;

		if ( activeEnemies.Count > 0 )
			return;

		waveRunning = false;

		Log.Info( "Wave completed." );

		OnWaveCompleted?.Invoke();
	}

	public void StopSpawning( bool clearActiveEnemies )
	{
		waveRunning = false;

		if ( clearActiveEnemies )
		{
			ClearActiveEnemies();
		}

		Log.Info( "Enemy spawning stopped." );
	}

	public void ClearActiveEnemies()
	{
		foreach ( var enemy in activeEnemies )
		{
			if ( enemy.IsValid() )
			{
				enemy.Destroy();
			}
		}

		activeEnemies.Clear();

		totalSpawnedThisWave = 0;
		totalKilledThisWave = 0;

		Log.Info( "Active enemies cleared." );
	}

	private void CleanActiveEnemyList()
	{
		activeEnemies.RemoveAll( enemy => !enemy.IsValid() );
	}
}
