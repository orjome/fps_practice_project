using Sandbox;
using System;

public sealed partial class WeaponManager : Component
{
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
}
