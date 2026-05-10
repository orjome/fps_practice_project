using Sandbox;

public sealed class EnemyMover : Component
{
	[Property] public GameObject Target { get; set; }

	[Property] public float MoveSpeed { get; set; } = 120f;
	[Property] public float StopDistance { get; set; } = 80f;
	[Property] public bool FaceTarget { get; set; } = true;

	[Property] public float AttackDistance { get; set; } = 90f;
	[Property] public float AttackDamage { get; set; } = 10f;
	[Property] public float AttackCooldown { get; set; } = 1f;

	[Property, Group( "Collision" )] public bool UseCollisionTrace { get; set; } = true;
	[Property, Group( "Collision" )] public float CollisionRadius { get; set; } = 24f;

	private float nextAttackTime;

	protected override void OnStart()
	{
		if ( Target is null )
		{
			Log.Warning( $"{GameObject.Name} EnemyMover needs a Target assigned." );
		}
	}

	protected override void OnUpdate()
	{
		if ( Target is null )
			return;

		MoveTowardTarget();
		TryAttackTarget();
	}

	private void MoveTowardTarget()
	{
		Vector3 enemyPosition = WorldPosition;
		Vector3 targetPosition = Target.WorldPosition;

		Vector3 toTarget = targetPosition - enemyPosition;
		toTarget.z = 0f;

		float distance = toTarget.Length;

		if ( distance <= StopDistance )
			return;

		Vector3 direction = toTarget.Normal;
		Vector3 moveAmount = direction * MoveSpeed * Time.Delta;
		Vector3 desiredPosition = enemyPosition + moveAmount;

		if ( CanMoveToPosition( enemyPosition, desiredPosition ) )
		{
			WorldPosition = desiredPosition;
		}

		if ( FaceTarget )
		{
			WorldRotation = Rotation.LookAt( direction, new Vector3( 0f, 0f, 1f ) );
		}
	}

	private bool CanMoveToPosition( Vector3 fromPosition, Vector3 toPosition )
	{
		if ( !UseCollisionTrace )
			return true;

		var trace = Scene.Trace
			.Ray( fromPosition, toPosition )
			.Radius( CollisionRadius )
			.IgnoreGameObjectHierarchy( GameObject )
			.Run();

		return !trace.Hit;
	}

	private void TryAttackTarget()
	{
		if ( Time.Now < nextAttackTime )
			return;

		Vector3 toTarget = Target.WorldPosition - WorldPosition;
		toTarget.z = 0f;

		float distance = toTarget.Length;

		if ( distance > AttackDistance )
			return;

		var damageable = FindDamageableOnObject( Target );

		if ( damageable is null )
			return;

		if ( damageable is PlayerHealth playerHealth && playerHealth.IsDead )
			return;

		damageable.TakeDamage( AttackDamage );

		nextAttackTime = Time.Now + AttackCooldown;

		Log.Info( $"{GameObject.Name} attacked {Target.Name} for {AttackDamage} damage." );
	}

	private IGameDamageable FindDamageableOnObject( GameObject targetObject )
	{
		foreach ( var component in targetObject.Components.GetAll() )
		{
			if ( component is IGameDamageable damageable )
			{
				return damageable;
			}
		}

		return null;
	}
}
