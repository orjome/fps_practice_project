using Sandbox;

public sealed partial class WeaponManager : Component
{
	private void FireProjectile( float spreadForThisShot )
	{
		if ( CurrentWeapon.ProjectilePrefab is null )
		{
			Log.Warning( $"{CurrentWeapon.DisplayName} is set to Projectile mode but has no ProjectilePrefab assigned." );
			return;
		}

		var eye = PlayerController.EyeTransform;

		Vector3 direction = GetSpreadDirection( spreadForThisShot );
		Vector3 spawnPosition = eye.Position + direction * CurrentWeapon.ProjectileSpawnForwardOffset;

		GameObject projectileObject = CurrentWeapon.ProjectilePrefab.Clone( spawnPosition );
		projectileObject.WorldRotation = Rotation.LookAt( direction, new Vector3( 0f, 0f, 1f ) );

		var projectile = projectileObject.Components.Get<Projectile>( FindMode.EverythingInSelfAndDescendants );

		if ( projectile is null )
		{
			Log.Warning( $"{CurrentWeapon.ProjectilePrefab.Name} does not have a Projectile component." );
			projectileObject.Destroy();
			return;
		}

		projectile.Initialize(
			GameObject,
			direction,
			CurrentWeapon.ProjectileSpeed,
			CurrentWeapon.Damage,
			CurrentWeapon.ProjectileLifetime,
			CurrentWeapon.ProjectileUsesGravity,
			CurrentWeapon.ProjectileGravityStrength,
			HitmarkerHud,
			DamageNumberHud
		);
	}
}
