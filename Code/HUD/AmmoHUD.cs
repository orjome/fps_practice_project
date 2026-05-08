using Sandbox;

public sealed class AmmoHud : Component
{
	[Property] public WeaponManager WeaponManager { get; set; }

	[Property] public float TextSize { get; set; } = 28f;
	[Property] public float RightOffset { get; set; } = 40f;
	[Property] public float BottomOffset { get; set; } = 40f;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		if ( WeaponManager is null )
			return;

		DrawAmmo();
	}

	private void DrawAmmo()
	{
		var hud = Scene.Camera.Hud;

		float x = Screen.Width - RightOffset;
		float y = Screen.Height - BottomOffset;

		string ammoText = $"{WeaponManager.CurrentAmmo} / {WeaponManager.ReserveAmmo}";
		string weaponText = WeaponManager.WeaponName;

		if ( WeaponManager.IsReloading )
		{
			ammoText = "Reloading...";
		}

		hud.DrawText(
			weaponText,
			TextSize * 0.7f,
			Color.White,
			new Vector2( x, y - 38f ),
			TextFlag.RightBottom
		);

		hud.DrawText(
			ammoText,
			TextSize,
			Color.White,
			new Vector2( x, y ),
			TextFlag.RightBottom
		);
	}
}
