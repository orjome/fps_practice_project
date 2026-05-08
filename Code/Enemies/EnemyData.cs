using Sandbox;
using System;

[AssetType( Name = "Enemy Data", Extension = "enemy", Category = "Game" )]
public partial class EnemyData : GameResource
{
	public string DisplayName { get; set; } = "Basic Enemy";

	[Property] public GameObject EnemyPrefab { get; set; }

	[Range( 1, 100 )]
	public int MinWave { get; set; } = 1;

	[Range( 0f, 100f )]
	public float SpawnWeight { get; set; } = 1f;

	[Group( "Health" ), Range( 1f, 1000f )]
	public float BaseHealth { get; set; } = 100f;

	[Group( "Health" ), Range( 0f, 100f )]
	public float HealthAddedPerWave { get; set; } = 15f;

	[Group( "Movement" ), Range( 1f, 500f )]
	public float BaseMoveSpeed { get; set; } = 120f;

	[Group( "Movement" ), Range( 0f, 50f )]
	public float SpeedAddedPerWave { get; set; } = 5f;

	[Group( "Movement" ), Range( 1f, 800f )]
	public float MaxMoveSpeed { get; set; } = 220f;

	[Group( "Attack" ), Range( 0f, 200f )]
	public float BaseAttackDamage { get; set; } = 10f;

	[Group( "Attack" ), Range( 0f, 50f )]
	public float AttackDamageAddedPerWave { get; set; } = 1f;

	[Group( "XP" ), Range( 0, 1000 )]
	public int BaseXpReward { get; set; } = 25;

	[Group( "XP" ), Range( 0, 100 )]
	public int XpAddedPerWave { get; set; } = 3;

	public float GetHealthForWave( int currentWave )
	{
		int waveIndex = Math.Max( 0, currentWave - MinWave );
		return BaseHealth + HealthAddedPerWave * waveIndex;
	}

	public float GetMoveSpeedForWave( int currentWave )
	{
		int waveIndex = Math.Max( 0, currentWave - MinWave );
		float speed = BaseMoveSpeed + SpeedAddedPerWave * waveIndex;
		return MathF.Min( speed, MaxMoveSpeed );
	}

	public float GetAttackDamageForWave( int currentWave )
	{
		int waveIndex = Math.Max( 0, currentWave - MinWave );
		return BaseAttackDamage + AttackDamageAddedPerWave * waveIndex;
	}

	public int GetXpRewardForWave( int currentWave )
	{
		int waveIndex = Math.Max( 0, currentWave - MinWave );
		return BaseXpReward + XpAddedPerWave * waveIndex;
	}
}
