using Sandbox;

public sealed partial class WeaponManager : Component
{
	private void FireRaycast( float spreadForThisShot )
	{
		var eye = PlayerController.EyeTransform;

		Vector3 start = eye.Position;
		Vector3 direction = GetSpreadDirection( spreadForThisShot );
		Vector3 end = start + direction * CurrentWeapon.FireDistance;

		var trace = Scene.Trace
			.Ray( start, end )
			.IgnoreGameObjectHierarchy( PlayerController.GameObject )
			.Run();

		DrawDebugBulletPath( start, end, trace );
		if ( !trace.Hit )
		{
			Log.Info( "Shot missed." );
			return;
		}

		SpawnBulletImpact( trace.HitPosition, trace.Normal );

		if ( !trace.Hit )
		{
			Log.Info( "Shot missed." );
			return;
		}

		Log.Info( $"Shot hit: {trace.GameObject.Name}" );

		var damageable = FindDamageableOnObject( trace.GameObject );

		if ( damageable is not null )
		{
			damageable.TakeDamage( CurrentWeapon.Damage );
			HitmarkerHud?.ShowHitmarker();
			DamageNumberHud?.ShowDamage( CurrentWeapon.Damage );
		}
		else
		{
			Log.Info( $"{trace.GameObject.Name} cannot take damage." );
		}
	}

	private void SpawnBulletImpact( Vector3 position, Vector3 normal )
	{
		if ( CurrentWeapon?.BulletImpactPrefab is null )
			return;

		var impact = CurrentWeapon.BulletImpactPrefab.Clone( position );
		impact.WorldRotation = Rotation.LookAt( normal, Vector3.Up );
	}
}
