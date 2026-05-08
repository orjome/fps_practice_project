using Sandbox;

[AssetType( Name = "Weapon Data", Extension = "weapon", Category = "Game" )]
public partial class WeaponData : GameResource
{
	public string DisplayName { get; set; } = "Test Rifle";

	[Range( 1f, 500f )]
	public float Damage { get; set; } = 25f;

	[Range( 100f, 10000f )]
	public float FireDistance { get; set; } = 5000f;

	// Shots per second.
	[Range( 0.1f, 30f )]
	public float FireRate { get; set; } = 8f;

	public bool IsAutomatic { get; set; } = true;

	[Range( 1, 200 )]
	public int MagazineSize { get; set; } = 30;

	[Range( 0, 999 )]
	public int StartingReserveAmmo { get; set; } = 90;

	[Range( 0.1f, 10f )]
	public float ReloadTime { get; set; } = 1.5f;

	// Feel values.
	[Range( 0f, 30f )]
	public float CrosshairKick { get; set; } = 5f;

	[Range( 0f, 10f )]
	public float RecoilPitch { get; set; } = 0.65f;

	[Range( 0f, 10f )]
	public float RecoilYaw { get; set; } = 0.25f;
	// Spread values, measured in degrees.
	[Range( 0f, 10f )]
	public float BaseSpread { get; set; } = 0.15f;

	[Range( 0f, 20f )]
	public float MaxSpread { get; set; } = 3.5f;

	[Range( 0f, 10f )]
	public float SpreadPerShot { get; set; } = 0.35f;

	[Range( 0f, 30f )]
	public float SpreadRecoverySpeed { get; set; } = 7f;
	// Audio
	[Property, Group( "Audio" )] public SoundEvent FireSound { get; set; }
	[Property, Group( "Audio" )] public SoundEvent EmptySound { get; set; }
	[Property, Group( "Audio" )] public SoundEvent ReloadStartSound { get; set; }
	[Property, Group( "Audio" )] public SoundEvent ReloadCompleteSound { get; set; }

}


