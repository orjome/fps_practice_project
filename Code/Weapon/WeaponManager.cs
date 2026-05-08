using Sandbox;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public sealed class WeaponManager : Component
{
	private sealed class WeaponInventorySlot
	{
		public WeaponData Weapon;
		public int CurrentAmmo;
		public int ReserveAmmo;

		public WeaponInventorySlot( WeaponData weapon )
		{
			Weapon = weapon;
			CurrentAmmo = weapon.MagazineSize;
			ReserveAmmo = weapon.StartingReserveAmmo;
		}
	}

	[Property] public PlayerController PlayerController { get; set; }
	[Property] public HitmarkerHud HitmarkerHud { get; set; }
	[Property] public SimpleCrosshair Crosshair { get; set; }
	[Property] public WeaponViewKick ViewKick { get; set; }
	[Property] public DamageNumberHud DamageNumberHud { get; set; }

	// Keep this for your starting weapon assignment.
	[Property] public WeaponData CurrentWeapon { get; set; }

	[Property, Group( "Inventory" )] public List<WeaponData> StartingWeapons { get; set; } = new();
	[Property, Group( "Inventory" )] public int MaxWeaponSlots { get; set; } = 3;
	[Property, Group( "Inventory" )] public string Slot1Input { get; set; } = "weapon_slot_1";
	[Property, Group( "Inventory" )] public string Slot2Input { get; set; } = "weapon_slot_2";
	[Property, Group( "Inventory" )] public string Slot3Input { get; set; } = "weapon_slot_3";

	[Property, Group( "Debug" )] public bool ShowDebugBulletPath { get; set; } = true;
	[Property, Group( "Debug" )] public float DebugBulletPathDuration { get; set; } = 0.15f;
	[Property, Group( "Debug" )] public bool DebugDrawFullTrace { get; set; } = true;

	private readonly List<WeaponInventorySlot> inventorySlots = new();

	private int currentSlotIndex = -1;
	private bool isReloading;
	private float nextFireTime;
	private float currentSpread;
	private int reloadVersion;

	public int CurrentAmmo => CurrentSlot is null ? 0 : CurrentSlot.CurrentAmmo;
	public int ReserveAmmo => CurrentSlot is null ? 0 : CurrentSlot.ReserveAmmo;
	public bool IsReloading => isReloading;
	public string WeaponName => CurrentWeapon is null ? "No Weapon" : CurrentWeapon.DisplayName;
	public int MagazineSize => CurrentWeapon is null ? 0 : CurrentWeapon.MagazineSize;
	public float CurrentSpread => currentSpread;

	public int CurrentSlotNumber => currentSlotIndex + 1;
	public int WeaponCount => inventorySlots.Count;

	private WeaponInventorySlot CurrentSlot
	{
		get
		{
			if ( currentSlotIndex < 0 || currentSlotIndex >= inventorySlots.Count )
				return null;

			return inventorySlots[currentSlotIndex];
		}
	}

	protected override void OnStart()
	{
		PlayerController ??= GetComponent<PlayerController>();
		ViewKick ??= GetComponent<WeaponViewKick>();

		if ( PlayerController is null )
		{
			Log.Warning( "WeaponManager needs a PlayerController reference." );
		}

		ResetInventoryForNewRun();
	}

	protected override void OnUpdate()
	{
		if ( PlayerController is null || CurrentWeapon is null )
			return;

		HandleWeaponSlotInput();

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

	private void HandleWeaponSlotInput()
	{
		if ( WasSlotInputPressed( Slot1Input, "1" ) )
		{
			SwitchToSlot( 0 );
		}

		if ( WasSlotInputPressed( Slot2Input, "2" ) )
		{
			SwitchToSlot( 1 );
		}

		if ( WasSlotInputPressed( Slot3Input, "3" ) )
		{
			SwitchToSlot( 2 );
		}
	}

	private bool WasSlotInputPressed( string inputAction, string rawKey )
	{
		// First try the S&box input action.
		if ( !string.IsNullOrWhiteSpace( inputAction ) && Input.Pressed( inputAction ) )
		{
			return true;
		}

		// Fallback: directly check the physical keyboard key.
		if ( Input.Keyboard.Pressed( rawKey ) )
		{
			return true;
		}

		return false;
	}

	public void ResetInventoryForNewRun()
	{
		inventorySlots.Clear();

		if ( StartingWeapons is not null )
		{
			foreach ( var weapon in StartingWeapons )
			{
				if ( weapon is null )
					continue;

				AddWeaponSlot( weapon );
			}
		}

		// Backwards compatibility:
		// If you only assigned CurrentWeapon in the inspector, use it as the starter weapon.
		if ( inventorySlots.Count == 0 && CurrentWeapon is not null )
		{
			AddWeaponSlot( CurrentWeapon );
		}

		if ( inventorySlots.Count == 0 )
		{
			CurrentWeapon = null;
			currentSlotIndex = -1;
			Log.Warning( "WeaponManager has no starting weapons assigned." );
			return;
		}

		SwitchToSlot( 0, true );

		Log.Info( $"Weapon inventory reset. Starting weapon: {WeaponName}" );
	}

	public void ResetToStartingWeapon()
	{
		ResetInventoryForNewRun();
	}

	public void ResetCurrentWeapon()
	{
		if ( CurrentWeapon is null )
			return;

		EquipWeapon( CurrentWeapon );
	}

	public bool EquipWeapon( WeaponData newWeapon )
	{
		return AddOrEquipWeapon( newWeapon );
	}

	public bool AddOrEquipWeapon( WeaponData newWeapon )
	{
		if ( newWeapon is null )
		{
			Log.Warning( "Cannot add or equip a null weapon." );
			return false;
		}

		int existingIndex = FindWeaponSlotIndex( newWeapon );

		if ( existingIndex >= 0 )
		{
			SwitchToSlot( existingIndex );
			Log.Info( $"Equipped existing weapon: {newWeapon.DisplayName}" );
			return true;
		}

		if ( inventorySlots.Count < MaxWeaponSlots )
		{
			int newSlotIndex = AddWeaponSlot( newWeapon );
			SwitchToSlot( newSlotIndex );
			Log.Info( $"Added {newWeapon.DisplayName} to slot {newSlotIndex + 1}." );
			return true;
		}

		// Simple replacement rule:
		// If inventory is full, replace the currently equipped slot.
		if ( CurrentSlot is not null )
		{
			inventorySlots[currentSlotIndex] = new WeaponInventorySlot( newWeapon );
			SwitchToSlot( currentSlotIndex, true );
			Log.Info( $"Inventory full. Replaced current slot with {newWeapon.DisplayName}." );
			return true;
		}

		return false;
	}

	private int AddWeaponSlot( WeaponData weapon )
	{
		inventorySlots.Add( new WeaponInventorySlot( weapon ) );
		return inventorySlots.Count - 1;
	}

	private int FindWeaponSlotIndex( WeaponData weapon )
	{
		for ( int i = 0; i < inventorySlots.Count; i++ )
		{
			if ( inventorySlots[i].Weapon == weapon )
				return i;
		}

		return -1;
	}

	public bool SwitchToSlot( int slotIndex, bool force = false )
	{
		if ( slotIndex < 0 || slotIndex >= inventorySlots.Count )
			return false;

		if ( !force && slotIndex == currentSlotIndex )
			return true;

		reloadVersion++;
		isReloading = false;
		currentSpread = 0f;
		nextFireTime = 0f;

		currentSlotIndex = slotIndex;
		CurrentWeapon = inventorySlots[currentSlotIndex].Weapon;

		Log.Info( $"Switched to slot {currentSlotIndex + 1}: {CurrentWeapon.DisplayName}. Ammo: {CurrentAmmo}/{ReserveAmmo}" );

		return true;
	}

	public bool IsSlotUnlocked( int slotIndex )
	{
		return slotIndex >= 0 && slotIndex < inventorySlots.Count && inventorySlots[slotIndex].Weapon is not null;
	}

	public string GetWeaponNameForSlot( int slotIndex )
	{
		if ( !IsSlotUnlocked( slotIndex ) )
			return "---";

		return inventorySlots[slotIndex].Weapon.DisplayName;
	}

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

		float spreadForThisShot = currentSpread;

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
		return min + Random.Shared.NextSingle() * (max - min);
	}

	private static float DegreesToRadians( float degrees )
	{
		return degrees * MathF.PI / 180f;
	}

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

	private void PlaySoundIfValid( SoundEvent sound )
	{
		if ( sound is null )
			return;

		GameObject.PlaySound( sound, Vector3.Zero );
	}
}
