using Sandbox;
using System;
using System.Threading.Tasks;

public sealed partial class WeaponManager : Component
{
	private void StartReload()
	{
		var slot = CurrentSlot;

		if ( CurrentWeapon is null || slot is null )
			return;

		if ( isReloading )
			return;

		if ( slot.CurrentAmmo >= CurrentWeapon.MagazineSize )
		{
			Log.Info( "Magazine already full." );
			return;
		}

		if ( slot.ReserveAmmo <= 0 )
		{
			Log.Info( "No reserve ammo." );
			return;
		}

		isReloading = true;

		PlaySoundIfValid( CurrentWeapon.ReloadStartSound );

		int thisReloadVersion = ++reloadVersion;

		_ = ReloadAfterDelay( thisReloadVersion );
	}

	private async Task ReloadAfterDelay( int thisReloadVersion )
	{
		if ( CurrentWeapon is not null )
		{
			Log.Info( $"Reloading {CurrentWeapon.DisplayName}..." );
		}

		float reloadTime = CurrentWeapon is null ? 1f : CurrentWeapon.ReloadTime;

		await Task.DelaySeconds( reloadTime );

		if ( !this.IsValid() || CurrentWeapon is null )
			return;

		if ( thisReloadVersion != reloadVersion )
			return;

		var slot = CurrentSlot;

		if ( slot is null )
			return;

		int ammoNeeded = CurrentWeapon.MagazineSize - slot.CurrentAmmo;
		int ammoToLoad = Math.Min( ammoNeeded, slot.ReserveAmmo );

		slot.CurrentAmmo += ammoToLoad;
		slot.ReserveAmmo -= ammoToLoad;

		isReloading = false;

		PlaySoundIfValid( CurrentWeapon.ReloadCompleteSound );

		Log.Info( $"Reload complete. Ammo: {slot.CurrentAmmo}/{slot.ReserveAmmo}" );
	}
}
