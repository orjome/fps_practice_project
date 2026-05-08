using Sandbox;
using System;
using System.Threading.Tasks;

public sealed class WeaponManager : Component
{
	[Property] public PlayerController PlayerController { get; set; }
	[Property] public HitmarkerHud HitmarkerHud { get; set; }
	[Property] public SimpleCrosshair Crosshair { get; set; }
	[Property] public WeaponViewKick ViewKick { get; set; }

	[Property] public WeaponData CurrentWeapon { get; set; }
	[Property, Group( "Debug" )] public bool ShowDebugBulletPath { get; set; } = true;
	[Property, Group( "Debug" )] public float DebugBulletPathDuration { get; set; } = 0.15f;
	[Property, Group( "Debug" )] public bool DebugDrawFullTrace { get; set; } = true;
	[Property] public DamageNumberHud DamageNumberHud { get; set; }

	private WeaponData startingWeapon;
	private int currentAmmo;
	private int reserveAmmo;

	private bool isReloading;
	private float nextFireTime;
	private float currentSpread;

	private int reloadVersion;

	public int CurrentAmmo => currentAmmo;
	public int ReserveAmmo => reserveAmmo;
	public bool IsReloading => isReloading;
	public string WeaponName => CurrentWeapon is null ? "No Weapon" : CurrentWeapon.DisplayName;
	public int MagazineSize => CurrentWeapon is null ? 0 : CurrentWeapon.MagazineSize;
	public float CurrentSpread => currentSpread;

	protected override void OnStart()
	{
		PlayerController ??= GetComponent<PlayerController>();
		ViewKick ??= GetComponent<WeaponViewKick>();

		if ( PlayerController is null )
		{
			Log.Warning( "WeaponManager needs a PlayerController reference." );
		}

		if ( CurrentWeapon is null )
		{
			Log.Warning( "WeaponManager needs a WeaponData asset assigned." );
			return;
		}

		startingWeapon = CurrentWeapon;
		EquipWeapon( CurrentWeapon );
	}

	protected override void OnUpdate()
	{
		if ( PlayerController is null || CurrentWeapon is null )
			return;

		if ( Input.Pressed( "reload" ) )
		{
			StartReload();
		}

		bool wantsToFire = CurrentWeapon.IsAutomatic
			? Input.Down( "attack1" )
			: Input.Pressed( "attack1" );

		if ( wantsToFire )
		{
			TryFire();
		}
		else
		{
			RecoverSpread();
		}
	}

	public bool EquipWeapon( WeaponData newWeapon )
	{
		if ( newWeapon is null )
		{
			Log.Warning( "Cannot equip a null weapon." );
			return false;
		}

		reloadVersion++;
		isReloading = false;

		CurrentWeapon = newWeapon;

		currentAmmo = CurrentWeapon.MagazineSize;
		reserveAmmo = CurrentWeapon.StartingReserveAmmo;
		currentSpread = 0f;
		nextFireTime = 0f;

		Log.Info( $"Equipped {CurrentWeapon.DisplayName}. Ammo: {currentAmmo}/{reserveAmmo}" );

		return true;
	}
	public void ResetCurrentWeapon()
	{
		if ( CurrentWeapon is null )
			return;

		EquipWeapon( CurrentWeapon );
	}
	public void ResetToStartingWeapon()
	{
		if ( startingWeapon is null )
		{
			ResetCurrentWeapon();
			return;
		}

		EquipWeapon( startingWeapon );
	}
	private void PlaySoundIfValid( SoundEvent sound )
	{
		if ( sound is null )
			return;

		// WeaponManager is on the Player, so this makes the sound follow the player.
		GameObject.PlaySound( sound );
	}

	private void TryFire()
	{
		if ( isReloading )
			return;

		if ( Time.Now < nextFireTime )
			return;

		if ( currentAmmo <= 0 )
		{
			Log.Info( "Click! Magazine empty." );
			PlaySoundIfValid( CurrentWeapon.EmptySound );
			nextFireTime = Time.Now + 0.2f;
			StartReload();
			return;
		}

		currentAmmo--;

		float secondsBetweenShots = 1f / MathF.Max( CurrentWeapon.FireRate, 0.01f );
		nextFireTime = Time.Now + secondsBetweenShots;

		float spreadForThisShot = currentSpread;

		// Important: fire first, then apply recoil.
		// That means recoil affects the next shot, not the shot that already happened.
		FireRaycast( spreadForThisShot );
		ApplyWeaponFeel();
		PlaySoundIfValid( CurrentWeapon.FireSound );
		IncreaseSpread();

		Log.Info( $"Fired {CurrentWeapon.DisplayName}. Ammo: {currentAmmo}/{reserveAmmo}. Shot Spread: {CurrentWeapon.BaseSpread + spreadForThisShot:0.00}, Current Spread: {CurrentWeapon.BaseSpread + currentSpread:0.00}" );
	}

	private void ApplyWeaponFeel()
	{
		Crosshair?.Pulse( CurrentWeapon.CrosshairKick );
		ViewKick?.ApplyFireKick( CurrentWeapon.RecoilPitch, CurrentWeapon.RecoilYaw );
	}

	private void IncreaseSpread()
	{
		currentSpread += CurrentWeapon.SpreadPerShot;
		currentSpread = MathF.Min( currentSpread, CurrentWeapon.MaxSpread );
	}

	private void RecoverSpread()
	{
		if ( currentSpread <= 0f )
			return;

		currentSpread -= CurrentWeapon.SpreadRecoverySpeed * Time.Delta;
		currentSpread = MathF.Max( currentSpread, 0f );
	}

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
	private void DrawDebugBulletPath( Vector3 start, Vector3 end, SceneTraceResult trace )
	{
		if ( !ShowDebugBulletPath )
			return;

		// Draw S&box's full trace visualization.
		if ( DebugDrawFullTrace )
		{
			GameObject.DebugOverlay.Trace( trace, DebugBulletPathDuration, false );
		}

		Vector3 lineEnd = trace.Hit ? trace.HitPosition : end;
		Color lineColor = trace.Hit ? Color.Green : Color.Red;

		GameObject.DebugOverlay.Line(
			start,
			lineEnd,
			lineColor,
			DebugBulletPathDuration,
			default,
			false
		);
	}

	private Vector3 GetSpreadDirection( float spreadForThisShot )
	{
		var eye = PlayerController.EyeTransform;

		float totalSpreadDegrees = CurrentWeapon.BaseSpread + spreadForThisShot;

		if ( totalSpreadDegrees <= 0.001f )
			return eye.Forward;

		float randomYaw = RandomRange( -totalSpreadDegrees, totalSpreadDegrees );
		float randomPitch = RandomRange( -totalSpreadDegrees, totalSpreadDegrees );

		float yawRadians = DegreesToRadians( randomYaw );
		float pitchRadians = DegreesToRadians( randomPitch );

		Vector3 direction = eye.Forward;
		direction += eye.Right * MathF.Tan( yawRadians );
		direction += eye.Up * MathF.Tan( pitchRadians );

		return direction.Normal;
	}

	private static float RandomRange( float min, float max )
	{
		return min + System.Random.Shared.NextSingle() * (max - min);
	}

	private static float DegreesToRadians( float degrees )
	{
		return degrees * MathF.PI / 180f;
	}

	private void StartReload()
	{
		if ( CurrentWeapon is null )
			return;

		if ( isReloading )
			return;

		if ( currentAmmo >= CurrentWeapon.MagazineSize )
		{
			Log.Info( "Magazine already full." );
			return;
		}

		if ( reserveAmmo <= 0 )
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
		Log.Info( $"Reloading {CurrentWeapon.DisplayName}..." );

		await Task.DelaySeconds( CurrentWeapon.ReloadTime );

		if ( !this.IsValid() || CurrentWeapon is null )
			return;

		if ( thisReloadVersion != reloadVersion )
			return;

		int ammoNeeded = CurrentWeapon.MagazineSize - currentAmmo;
		int ammoToLoad = Math.Min( ammoNeeded, reserveAmmo );

		currentAmmo += ammoToLoad;
		reserveAmmo -= ammoToLoad;

		isReloading = false;
		PlaySoundIfValid( CurrentWeapon.ReloadCompleteSound );


		Log.Info( $"Reload complete. Ammo: {currentAmmo}/{reserveAmmo}" );
	}

	private IGameDamageable FindDamageableOnObject( GameObject hitObject )
	{
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
