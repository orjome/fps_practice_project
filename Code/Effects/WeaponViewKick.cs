using Sandbox;
using System;

public sealed class WeaponViewKick : Component
{
	[Property] public PlayerController PlayerController { get; set; }
	[Property] public bool EnableViewKick { get; set; } = true;

	protected override void OnStart()
	{
		PlayerController ??= GetComponent<PlayerController>();

		if ( PlayerController is null )
		{
			Log.Warning( "WeaponViewKick needs a PlayerController reference." );
		}
	}

	public void ApplyFireKick( float pitchKick, float yawKick )
	{
		if ( !EnableViewKick )
			return;

		if ( PlayerController is null )
			return;

		float randomYaw = ((Random.Shared.NextSingle() * 2f) - 1f) * yawKick;

		var eyeAngles = PlayerController.EyeAngles;

		// Small upward kick plus tiny random side movement.
		eyeAngles.pitch -= pitchKick;
		eyeAngles.yaw += randomYaw;

		PlayerController.EyeAngles = eyeAngles;
	}
}
