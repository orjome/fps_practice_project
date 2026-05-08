using Sandbox;
using System;
using System.Collections.Generic;

public sealed class WeaponPickup : Component
{
	[Property] public WeaponData WeaponToGive { get; set; }

	[Property] public GameObject Player { get; set; }
	[Property] public WeaponManager WeaponManager { get; set; }

	[Property] public float PickupDistance { get; set; } = 120f;
	[Property] public string PickupInput { get; set; } = "use";

	// Important: we hide the pickup instead of destroying it.
	// This lets GameOverManager bring it back when the run restarts.
	[Property] public bool DisableAfterPickup { get; set; } = true;

	[Property] public bool AnimatePickup { get; set; } = true;
	[Property] public float RotationSpeed { get; set; } = 90f;
	[Property] public float BobHeight { get; set; } = 8f;
	[Property] public float BobSpeed { get; set; } = 2f;

	[Property] public float PromptTextSize { get; set; } = 24f;
	[Property, Group( "Audio" )] public SoundEvent PickupSound { get; set; }

	private Vector3 startPosition;
	private Rotation startRotation;
	private bool isPickedUp;

	private readonly List<Component> disabledComponents = new();

	protected override void OnStart()
	{
		startPosition = WorldPosition;
		startRotation = WorldRotation;
		isPickedUp = false;

		if ( WeaponToGive is null )
		{
			Log.Warning( $"{GameObject.Name} WeaponPickup needs a WeaponToGive asset." );
		}

		if ( Player is null )
		{
			Log.Warning( $"{GameObject.Name} WeaponPickup needs a Player reference." );
		}

		if ( WeaponManager is null && Player is not null )
		{
			WeaponManager = Player.Components.Get<WeaponManager>();
		}
	}

	protected override void OnUpdate()
	{
		if ( isPickedUp )
			return;

		if ( AnimatePickup )
		{
			UpdatePickupAnimation();
		}

		if ( WeaponToGive is null || Player is null || WeaponManager is null )
			return;

		if ( !IsPlayerCloseEnough() )
			return;

		DrawPickupPrompt();

		if ( Input.Pressed( PickupInput ) )
		{
			PickUpWeapon();
		}
	}

	private bool IsPlayerCloseEnough()
	{
		float distance = WorldPosition.Distance( Player.WorldPosition );
		return distance <= PickupDistance;
	}

	private void PickUpWeapon()
	{
		bool equipped = WeaponManager.AddOrEquipWeapon( WeaponToGive );

		if ( !equipped )
			return;

		isPickedUp = true;

		Log.Info( $"Picked up {WeaponToGive.DisplayName}." );
		if ( PickupSound is not null && Player is not null )
		{
			Sound.Play( PickupSound, Player.WorldPosition );
		}

		if ( DisableAfterPickup )
		{
			DisablePickupVisuals();
		}
	}

	private void DisablePickupVisuals()
	{
		disabledComponents.Clear();

		foreach ( var component in GameObject.Components.GetAll() )
		{
			if ( component == this )
				continue;

			if ( !component.Enabled )
				continue;

			component.Enabled = false;
			disabledComponents.Add( component );
		}
	}

	private void EnablePickupVisuals()
	{
		foreach ( var component in disabledComponents )
		{
			if ( component.IsValid() )
			{
				component.Enabled = true;
			}
		}

		disabledComponents.Clear();
	}

	public void ResetPickup()
	{
		isPickedUp = false;

		WorldPosition = startPosition;
		WorldRotation = startRotation;

		EnablePickupVisuals();

		Log.Info( $"{GameObject.Name} pickup reset." );
	}

	private void UpdatePickupAnimation()
	{
		float bobOffset = MathF.Sin( Time.Now * BobSpeed ) * BobHeight;

		WorldPosition = startPosition + new Vector3( 0f, 0f, bobOffset );
		WorldRotation = Rotation.FromYaw( Time.Now * RotationSpeed );
	}

	private void DrawPickupPrompt()
	{
		if ( Scene.Camera is null )
			return;

		var hud = Scene.Camera.Hud;

		string text = $"Press E to pick up {WeaponToGive.DisplayName}";

		hud.DrawText(
			text,
			PromptTextSize,
			Color.White,
			new Vector2( Screen.Width * 0.5f, Screen.Height * 0.62f ),
			TextFlag.Center
		);
	}
}
