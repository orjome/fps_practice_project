using Sandbox;
using Sandbox.Rendering;

public sealed class WeaponSlotsHud : Component
{
	[Property] public WeaponManager WeaponManager { get; set; }

	[Property] public float CenterYOffset { get; set; } = 92f;
	[Property] public float SlotWidth { get; set; } = 150f;
	[Property] public float SlotHeight { get; set; } = 46f;
	[Property] public float SlotGap { get; set; } = 10f;

	protected override void OnUpdate()
	{
		if ( Scene.Camera is null )
			return;

		if ( WeaponManager is null )
			return;

		DrawSlots();
	}

	private void DrawSlots()
	{
		var hud = Scene.Camera.Hud;

		int maxSlots = 3;
		float totalWidth = maxSlots * SlotWidth + (maxSlots - 1) * SlotGap;

		float startX = (Screen.Width - totalWidth) * 0.5f;
		float y = Screen.Height - CenterYOffset;

		for ( int i = 0; i < maxSlots; i++ )
		{
			DrawSlot( hud, i, startX + i * (SlotWidth + SlotGap), y );
		}
	}

	private void DrawSlot( HudPainter hud, int slotIndex, float x, float y )
	{
		bool isUnlocked = WeaponManager.IsSlotUnlocked( slotIndex );
		bool isSelected = WeaponManager.CurrentSlotNumber == slotIndex + 1;

		Color background = isSelected
			? new Color( 0.15f, 0.35f, 0.55f, 0.72f )
			: HudStyle.PanelBackground;

		Color border = isSelected
			? HudStyle.Accent
			: HudStyle.PanelBorder;

		var rect = new Rect( x, y, SlotWidth, SlotHeight );

		hud.DrawRect(
			rect,
			background,
			new Vector4( 8f, 8f, 8f, 8f ),
			new Vector4( 1f, 1f, 1f, 1f ),
			border
		);

		string slotNumber = (slotIndex + 1).ToString();
		string weaponName = isUnlocked
			? WeaponManager.GetWeaponNameForSlot( slotIndex ).ToUpper()
			: "EMPTY";

		Color textColor = isUnlocked
			? HudStyle.TextPrimary
			: HudStyle.TextMuted;

		hud.DrawText(
			slotNumber,
			16f,
			HudStyle.TextMuted,
			new Vector2( x + 12f, y + 10f ),
			TextFlag.LeftTop
		);

		hud.DrawText(
			weaponName,
			15f,
			textColor,
			new Vector2( x + SlotWidth - 12f, y + 14f ),
			TextFlag.RightTop
		);
	}
}
