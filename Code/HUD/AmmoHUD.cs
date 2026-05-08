using Sandbox;

public sealed class AmmoHud : Component
{
	[Property] public WeaponManager WeaponManager { get; set; }

	[Property] public float RightOffset { get; set; } = 40f;
	[Property] public float BottomOffset { get; set; } = 40f;

	[Property] public float PanelWidth { get; set; } = 230f;
	[Property] public float PanelHeight { get; set; } = 92f;

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

		float x = Screen.Width - RightOffset - PanelWidth;
		float y = Screen.Height - BottomOffset - PanelHeight;

		var panelRect = new Rect( x, y, PanelWidth, PanelHeight );
		HudStyle.DrawPanel( hud, panelRect );

		string weaponText = WeaponManager.WeaponName;
		string ammoText = WeaponManager.IsReloading
			? "RELOADING"
			: $"{WeaponManager.CurrentAmmo} / {WeaponManager.ReserveAmmo}";

		Color ammoColor = HudStyle.TextPrimary;

		if ( WeaponManager.IsReloading )
		{
			ammoColor = HudStyle.Warning;
		}
		else if ( WeaponManager.CurrentAmmo <= 5 )
		{
			ammoColor = HudStyle.Danger;
		}

		hud.DrawText(
			weaponText.ToUpper(),
			18f,
			HudStyle.TextMuted,
			new Vector2( x + 18f, y + 18f ),
			TextFlag.LeftTop
		);

		hud.DrawText(
			ammoText,
			34f,
			ammoColor,
			new Vector2( x + PanelWidth - 18f, y + PanelHeight - 18f ),
			TextFlag.RightBottom
		);
	}
}
