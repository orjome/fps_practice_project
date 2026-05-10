using Sandbox;

public enum WeaponFireMode
{
	Hitscan,
	Projectile
}

[AssetType( Name = "Weapon Data", Extension = "weapon", Category = "Game" )]
public partial class WeaponData : GameResource
{
	[Property, Group( "Identity" )]
	public string DisplayName { get; set; } = "Test Rifle";

	[Property, Group( "Shooting" )]
	public WeaponFireMode FireMode { get; set; } = WeaponFireMode.Hitscan;

	[Property, Group( "Shooting" ), Range( 1f, 500f )]
	public float Damage { get; set; } = 25f;

	[Property, Group( "Shooting" ), Range( 100f, 10000f )]
	public float FireDistance { get; set; } = 5000f;

	// Shots per second.
	[Property, Group( "Shooting" ), Range( 0.1f, 30f )]
	public float FireRate { get; set; } = 8f;

	[Property, Group( "Shooting" )]
	public bool IsAutomatic { get; set; } = true;

	[Property, Group( "Ammo" ), Range( 1, 200 )]
	public int MagazineSize { get; set; } = 30;

	[Property, Group( "Ammo" ), Range( 0, 999 )]
	public int StartingReserveAmmo { get; set; } = 90;

	[Property, Group( "Ammo" ), Range( 0.1f, 10f )]
	public float ReloadTime { get; set; } = 1.5f;

	// Feel values.
	[Property, Group( "Feel" ), Range( 0f, 30f )]
	public float CrosshairKick { get; set; } = 5f;

	[Property, Group( "Feel" ), Range( 0f, 10f )]
	public float RecoilPitch { get; set; } = 0.65f;

	[Property, Group( "Feel" ), Range( 0f, 10f )]
	public float RecoilYaw { get; set; } = 0.25f;

	// Spread values, measured in degrees.
	[Property, Group( "Spread" ), Range( 0f, 10f )]
	public float BaseSpread { get; set; } = 0.15f;

	[Property, Group( "Spread" ), Range( 0f, 20f )]
	public float MaxSpread { get; set; } = 3.5f;

	[Property, Group( "Spread" ), Range( 0f, 10f )]
	public float SpreadPerShot { get; set; } = 0.35f;

	[Property, Group( "Spread" ), Range( 0f, 30f )]
	public float SpreadRecoverySpeed { get; set; } = 7f;
	[Property, Group( "ADS" )]
	public float AimFOV { get; set; } = 60f;

	[Property, Group( "ADS" ), Range( 0.1f, 1f )]
	public float AimSensitivity { get; set; } = 0.6f;

	[Property, Group( "ADS" ), Range( 1f, 20f )]
	public float AimInSpeed { get; set; } = 8f;

	[Property, Group( "ADS" ), Range( 1f, 20f )]
	public float AimOutSpeed { get; set; } = 10f;

	[Property, Group( "ADS" )]
	public Vector3 AimPositionOffset { get; set; } = Vector3.Zero;

	[Property, Group( "ADS" )]
	public Angles AimRotationOffset { get; set; } = Angles.Zero;

	[Property, Group( "ADS" ), Range( 0f, 1f )]
	public float AimSpreadMultiplier { get; set; } = 0.25f;

	// Projectile settings.
	[Property, Group( "Projectile" )]
	public GameObject ProjectilePrefab { get; set; }

	[Property, Group( "Projectile" ), Range( 50f, 10000f )]
	public float ProjectileSpeed { get; set; } = 1400f;

	[Property, Group( "Projectile" ), Range( 0.1f, 20f )]
	public float ProjectileLifetime { get; set; } = 4f;

	[Property, Group( "Projectile" )]
	public bool ProjectileUsesGravity { get; set; } = false;

	[Property, Group( "Projectile" ), Range( 0f, 3000f )]
	public float ProjectileGravityStrength { get; set; } = 800f;

	[Property, Group( "Projectile" ), Range( 0f, 200f )]
	public float ProjectileSpawnForwardOffset { get; set; } = 40f;

	// Visuals.
	[Property, Group( "Visuals - ViewModel" )]
	public GameObject ViewModelPrefab { get; set; }

	[Property, Group( "Visuals - ViewModel" )]
	public Vector3 ViewModelPositionOffset { get; set; } = new Vector3( 0f, 0f, 0f );

	[Property, Group( "Visuals - ViewModel" )]
	public Angles ViewModelRotationOffset { get; set; } = new Angles( 0f, 0f, 0f );

	[Property, Group( "Visuals - ViewModel" ), Range( 0.01f, 10f )]
	public float ViewModelScale { get; set; } = 1f;

	[Property, Group( "Visuals - WorldModel" )]
	public GameObject WorldModelPrefab { get; set; }

	[Property, Group( "Visuals - WorldModel" )]
	public Vector3 WorldModelPositionOffset { get; set; } = Vector3.Zero;

	[Property, Group( "Visuals - WorldModel" )]
	public Angles WorldModelRotationOffset { get; set; } = new Angles( 0f, 0f, 0f );

	[Property, Group( "Visuals - WorldModel" ), Range( 0.01f, 10f )]
	public float WorldModelScale { get; set; } = 1f;

	// Audio.
	[Property, Group( "Audio" )]
	public SoundEvent FireSound { get; set; }

	[Property, Group( "Audio" )]
	public SoundEvent EmptySound { get; set; }

	[Property, Group( "Audio" )]
	public SoundEvent ReloadStartSound { get; set; }

	[Property, Group( "Audio" )]
	public SoundEvent ReloadCompleteSound { get; set; }
}
