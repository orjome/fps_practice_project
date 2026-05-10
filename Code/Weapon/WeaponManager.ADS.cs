using Sandbox;

public sealed partial class WeaponManager : Component
{
	private void HandleAiming()
	{
		isAiming = Input.Down( "attack2" ) && !isReloading;

		float targetFOV = isAiming ? CurrentWeapon.AimFOV : Preferences.FieldOfView;
		float speed = isAiming ? CurrentWeapon.AimInSpeed : CurrentWeapon.AimOutSpeed;

		if ( currentFOV == 0f )
			currentFOV = Preferences.FieldOfView;

		currentFOV = currentFOV.LerpTo( targetFOV, speed * Time.Delta );

		if ( Scene.Camera is not null )
			Scene.Camera.FieldOfView = currentFOV;
	}
}
