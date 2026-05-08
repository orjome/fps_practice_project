using Sandbox;
using System;
using System.Threading.Tasks;

public sealed class PlayerHealth : Component, IGameDamageable
{
	[Property] public float MaxHealth { get; set; } = 100f;

	// For our survival mode, this should be false.
	// The GameOverManager will handle what happens after death.
	[Property] public bool RespawnOnDeath { get; set; } = false;
	[Property] public float RespawnDelay { get; set; } = 2f;

	[Property] public GameObject RespawnPoint { get; set; }
	[Property] public WeaponManager WeaponManager { get; set; }

	private float currentHealth;
	private bool isDead;

	private Vector3 startPosition;
	private Rotation startRotation;

	public event Action<PlayerHealth> OnDied;

	public float CurrentHealth => currentHealth;
	public bool IsDead => isDead;

	protected override void OnStart()
	{
		currentHealth = MaxHealth;
		isDead = false;

		startPosition = WorldPosition;
		startRotation = WorldRotation;

		WeaponManager ??= GetComponent<WeaponManager>();

		Log.Info( $"Player health set to {currentHealth}" );
	}

	public void TakeDamage( float damageAmount )
	{
		if ( isDead )
			return;

		currentHealth -= damageAmount;
		currentHealth = MathF.Max( currentHealth, 0f );

		Log.Info( $"Player took {damageAmount} damage. Health: {currentHealth}" );

		if ( currentHealth <= 0f )
		{
			Die();
		}
	}

	private void Die()
	{
		if ( isDead )
			return;

		isDead = true;

		Log.Info( "Player died." );

		if ( WeaponManager is not null )
		{
			WeaponManager.Enabled = false;
		}

		OnDied?.Invoke( this );

		if ( RespawnOnDeath )
		{
			_ = RespawnAfterDelay();
		}
	}

	private async Task RespawnAfterDelay()
	{
		await Task.DelaySeconds( RespawnDelay );

		if ( !this.IsValid() )
			return;

		ResetPlayerForNewRun();
	}

	public void ResetPlayerForNewRun()
	{
		currentHealth = MaxHealth;
		isDead = false;

		if ( RespawnPoint is not null )
		{
			WorldPosition = RespawnPoint.WorldPosition;
			WorldRotation = RespawnPoint.WorldRotation;
		}
		else
		{
			WorldPosition = startPosition;
			WorldRotation = startRotation;
		}

		if ( WeaponManager is not null )
		{
			WeaponManager.Enabled = true;
		}

		Log.Info( $"Player reset with {currentHealth} health." );
	}
}
