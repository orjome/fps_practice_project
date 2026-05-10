using Sandbox;

public sealed partial class WeaponManager : Component
{
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
		if ( !string.IsNullOrWhiteSpace( inputAction ) && Input.Pressed( inputAction ) )
		{
			return true;
		}

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
		RefreshWeaponVisuals();

		Log.Info( $"Switched to slot {currentSlotIndex + 1}: {CurrentWeapon.DisplayName}. Ammo: {CurrentAmmo}/{ReserveAmmo}" );

		return true;
	}

	public bool IsSlotUnlocked( int slotIndex )
	{
		return slotIndex >= 0
			&& slotIndex < inventorySlots.Count
			&& inventorySlots[slotIndex].Weapon is not null;
	}

	public string GetWeaponNameForSlot( int slotIndex )
	{
		if ( !IsSlotUnlocked( slotIndex ) )
			return "---";

		return inventorySlots[slotIndex].Weapon.DisplayName;
	}
}
