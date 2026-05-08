using Sandbox;
using System;
using System.Collections.Generic;

public sealed class EnemySpawner : Component
{
	[Property] public GameObject EnemyPrefab { get; set; }
	[Property] public GameObject Target { get; set; }
	[Property] public XpManager XpManager { get; set; }

	[Property] public List<GameObject> SpawnPoints { get; set; } = new();

	[Property, Group( "Wave Scaling" )] public float BaseEnemyHealth { get; set; } = 100f;
	[Property, Group( "Wave Scaling" )] public float HealthAddedPerWave { get; set; } = 15f;

	[Property, Group( "Wave Scaling" )] public float BaseEnemySpeed { get; set; } = 120f;
	[Property, Group( "Wave Scaling" )] public float SpeedAddedPerWave { get; set; } = 5f;
	[Property, Group( "Wave Scaling" )] public float MaxEnemySpeed { get; set; } = 220f;

	[Property, Group( "Wave Scaling" )] public float BaseEnemyAttackDamage { get; set; } = 10f;
	[Property, Group( "Wave Scaling" )] public float AttackDamageAddedPerWave { get; set; } = 1f;

	[Property, Group( "Wave Scaling" )] public int BaseXpReward { get; set; } = 25;
	[Property, Group( "Wave Scaling" )] public int XpAddedPerWave { get; set; } = 3;

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
		if ( EnemyPrefab is null )
		{
			Log.Warning( "EnemySpawner needs an EnemyPrefab assigned." );
			return;
		}

		if ( Target is null )
		{
			Log.Warning( "EnemySpawner needs a Target assigned." );
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
		Vector3 spawnPosition = GetNextSpawnPosition();

		GameObject enemy = EnemyPrefab.Clone( spawnPosition );

		totalSpawnedEver++;
		totalSpawnedThisWave++;

		enemy.Name = $"Enemy_{totalSpawnedEver}";

		activeEnemies.Add( enemy );

		SetupSpawnedEnemy( enemy );

		Log.Info( $"Spawned {enemy.Name}. Wave spawned: {totalSpawnedThisWave}/{totalToSpawnThisWave}. Active: {activeEnemies.Count}/{maxActiveEnemies}" );
	}

	private void SetupSpawnedEnemy( GameObject enemy )
	{
		var mover = enemy.Components.Get<EnemyMover>( FindMode.EverythingInSelfAndDescendants );

		if ( mover is not null )
		{
			mover.Target = Target;
			ApplyMoverScaling( mover );
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

			ApplyHealthScaling( health );

			health.OnDied += HandleEnemyDied;
		}
		else
		{
			Log.Warning( $"{enemy.Name} spawned without EnemyHealth." );
		}
	}

	private void ApplyHealthScaling( EnemyHealth health )
	{
		int waveIndex = currentWave - 1;

		float scaledHealth = BaseEnemyHealth + HealthAddedPerWave * waveIndex;
		int scaledXp = BaseXpReward + XpAddedPerWave * waveIndex;

		health.SetMaxHealth( scaledHealth, true );
		health.SetXpReward( scaledXp );

		Log.Info( $"{health.GameObject.Name} scaled health: {scaledHealth}, XP reward: {scaledXp}" );
	}

	private void ApplyMoverScaling( EnemyMover mover )
	{
		int waveIndex = currentWave - 1;

		float scaledSpeed = BaseEnemySpeed + SpeedAddedPerWave * waveIndex;
		scaledSpeed = MathF.Min( scaledSpeed, MaxEnemySpeed );

		float scaledAttackDamage = BaseEnemyAttackDamage + AttackDamageAddedPerWave * waveIndex;

		mover.MoveSpeed = scaledSpeed;
		mover.AttackDamage = scaledAttackDamage;

		Log.Info( $"{mover.GameObject.Name} scaled speed: {scaledSpeed}, attack damage: {scaledAttackDamage}" );
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

	private void CleanActiveEnemyList()
	{
		activeEnemies.RemoveAll( enemy => !enemy.IsValid() );
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
}
