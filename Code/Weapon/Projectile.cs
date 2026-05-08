using Sandbox;
using System;

public sealed class Projectile : Component
{
	[Property] public float CollisionRadius { get; set; } = 2f;

	[Property, Group( "Debug" )] public bool ShowDebugPath { get; set; } = false;
	[Property, Group( "Debug" )] public float DebugPathDuration { get; set; } = 0.08f;

	private GameObject owner;
	private Vector3 velocity;
	private float damage;
	private float lifetime;
	private float spawnTime;
	private bool usesGravity;
	private float gravityStrength;

	private HitmarkerHud hitmarkerHud;
	private DamageNumberHud damageNumberHud;

	public void Initialize(
		GameObject ownerObject,
		Vector3 direction,
		float projectileSpeed,
		float damageAmount,
		float maxLifetime,
		bool projectileUsesGravity,
		float projectileGravityStrength,
		HitmarkerHud hitmarker,
		DamageNumberHud damageNumbers )
	{
		owner = ownerObject;
		velocity = direction.Normal * projectileSpeed;
		damage = damageAmount;
		lifetime = maxLifetime;
		spawnTime = Time.Now;
		usesGravity = projectileUsesGravity;
		gravityStrength = projectileGravityStrength;
		hitmarkerHud = hitmarker;
		damageNumberHud = damageNumbers;

		if ( velocity.Length > 0f )
		{
			WorldRotation = Rotation.LookAt( velocity.Normal, new Vector3( 0f, 0f, 1f ) );
		}
	}

	protected override void OnUpdate()
	{
		if ( Time.Now - spawnTime >= lifetime )
		{
			GameObject.Destroy();
			return;
		}

		MoveProjectile();
	}

	private void MoveProjectile()
	{
		Vector3 oldPosition = WorldPosition;

		if ( usesGravity )
		{
			velocity += new Vector3( 0f, 0f, -gravityStrength ) * Time.Delta;
		}

		Vector3 newPosition = oldPosition + velocity * Time.Delta;

		var trace = Scene.Trace
			.Ray( oldPosition, newPosition )
			.Radius( CollisionRadius )
			.IgnoreGameObjectHierarchy( GameObject )
			.IgnoreGameObjectHierarchy( owner )
			.UseHitPosition( true )
			.Run();

		if ( ShowDebugPath )
		{
			GameObject.DebugOverlay.Line(
				oldPosition,
				trace.Hit ? trace.HitPosition : newPosition,
				trace.Hit ? Color.Green : Color.Red,
				DebugPathDuration,
				default,
				false
			);
		}

		if ( trace.Hit )
		{
			HandleHit( trace );
			return;
		}

		WorldPosition = newPosition;

		if ( velocity.Length > 0f )
		{
			WorldRotation = Rotation.LookAt( velocity.Normal, new Vector3( 0f, 0f, 1f ) );
		}
	}

	private void HandleHit( SceneTraceResult trace )
	{
		WorldPosition = trace.HitPosition;

		var damageable = FindDamageableOnObject( trace.GameObject );

		if ( damageable is not null )
		{
			damageable.TakeDamage( damage );
			hitmarkerHud?.ShowHitmarker();
			damageNumberHud?.ShowDamage( damage );
		}

		GameObject.Destroy();
	}

	private IGameDamageable FindDamageableOnObject( GameObject hitObject )
	{
		if ( hitObject is null )
			return null;

		foreach ( var component in hitObject.Components.GetAll() )
		{
			if ( component is IGameDamageable damageable )
			{
				return damageable;
			}
		}

		return null;
	}
}
