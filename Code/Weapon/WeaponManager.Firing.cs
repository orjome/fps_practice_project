using Sandbox;
using System;

public sealed partial class WeaponManager : Component
{
	private void TryFire()
	{
		var slot = CurrentSlot;

		if ( slot is null || CurrentWeapon is null )
			return;

		if ( isReloading )
			return;

		if ( Time.Now < nextFireTime )
			return;

		if ( slot.CurrentAmmo <= 0 )
		{
			Log.Info( "Click! Magazine empty." );
			PlaySoundIfValid( CurrentWeapon.EmptySound );

			nextFireTime = Time.Now + 0.2f;

			StartReload();
			return;
		}

		slot.CurrentAmmo--;

		float secondsBetweenShots = 1f / MathF.Max( CurrentWeapon.FireRate, 0.01f );
		nextFireTime = Time.Now + secondsBetweenShots;

		float spreadForThisShot = isAiming
	? currentSpread * CurrentWeapon.AimSpreadMultiplier
	: currentSpread;

		FireCurrentWeapon( spreadForThisShot );
		ApplyWeaponFeel();
		PlaySoundIfValid( CurrentWeapon.FireSound );
		IncreaseSpread();

		Log.Info( $"Fired {CurrentWeapon.DisplayName}. Ammo: {slot.CurrentAmmo}/{slot.ReserveAmmo}. Shot Spread: {CurrentWeapon.BaseSpread + spreadForThisShot:0.00}, Current Spread: {CurrentWeapon.BaseSpread + currentSpread:0.00}" );
	}

	private void FireCurrentWeapon( float spreadForThisShot )
	{
		if ( CurrentWeapon.FireMode == WeaponFireMode.Projectile )
		{
			FireProjectile( spreadForThisShot );
			return;
		}

		FireRaycast( spreadForThisShot );
	}
}
