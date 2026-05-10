using Sandbox;
using System.Collections.Generic;

public sealed partial class WeaponManager : Component
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
	private bool isAiming;
	private float currentFOV;
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
	public bool IsAiming => isAiming;
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

		HandleAiming();
		UpdateWeaponVisuals();
		HandleWeaponSlotInput();

		if ( Input.Pressed( "reload" ) && !isAiming )
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
}
