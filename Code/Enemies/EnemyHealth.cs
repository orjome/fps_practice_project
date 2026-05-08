using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class EnemyHealth : Component, IGameDamageable
{
	[Property] public float MaxHealth { get; set; } = 100f;
	[Property] public bool RespawnOnDeath { get; set; } = true;
	[Property] public float RespawnDelay { get; set; } = 2f;

	[Property] public bool AwardXpOnDeath { get; set; } = true;
	[Property] public int XpReward { get; set; } = 25;
	[Property] public XpManager XpManager { get; set; }

	private float currentHealth;
	private bool isDead;

	private readonly List<Component> disabledComponents = new();

	public event Action<EnemyHealth> OnDied;

	public float CurrentHealth => currentHealth;
	public bool IsDead => isDead;

	protected override void OnStart()
	{
		currentHealth = MaxHealth;
		isDead = false;

		Log.Info( $"{GameObject.Name} health set to {currentHealth}" );
	}

	public void TakeDamage( float damageAmount )
	{
		if ( isDead )
			return;

		currentHealth -= damageAmount;

		if ( currentHealth < 0f )
			currentHealth = 0f;

		Log.Info( $"{GameObject.Name} took {damageAmount} damage. Health: {currentHealth}" );

		if ( currentHealth <= 0f )
		{
			Die();
		}
	}

	public void SetMaxHealth( float newMaxHealth, bool healToFull = true )
	{
		MaxHealth = MathF.Max( 1f, newMaxHealth );

		if ( healToFull || currentHealth <= 0f )
		{
			currentHealth = MaxHealth;
		}
		else
		{
			currentHealth = MathF.Min( currentHealth, MaxHealth );
		}
	}

	public void SetXpReward( int newXpReward )
	{
		XpReward = Math.Max( 0, newXpReward );
	}

	private void Die()
	{
		if ( isDead )
			return;

		isDead = true;

		Log.Info( $"{GameObject.Name} died." );

		OnDied?.Invoke( this );

		if ( AwardXpOnDeath && XpManager is not null )
		{
			XpManager.AddXP( XpReward );
		}

		if ( RespawnOnDeath )
		{
			DisableTarget();
			_ = RespawnAfterDelay();
		}
		else
		{
			GameObject.Destroy();
		}
	}

	private void DisableTarget()
	{
		disabledComponents.Clear();

		foreach ( var component in GameObject.Components.GetAll() )
		{
			if ( component == this )
				continue;

			if ( !component.Enabled )
				continue;

			component.Enabled = false;
			disabledComponents.Add( component );
		}
	}

	private void EnableTarget()
	{
		foreach ( var component in disabledComponents )
		{
			if ( component.IsValid() )
			{
				component.Enabled = true;
			}
		}

		disabledComponents.Clear();
	}

	private async Task RespawnAfterDelay()
	{
		await Task.DelaySeconds( RespawnDelay );

		if ( !this.IsValid() )
			return;

		currentHealth = MaxHealth;
		isDead = false;

		EnableTarget();

		Log.Info( $"{GameObject.Name} respawned with {currentHealth} health." );
	}
}
