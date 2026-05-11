using Sandbox;
using System;

public sealed partial class WeaponManager : Component
{
	[Property, Group( "ViewModel Feel" )] public float SwayAmount { get; set; } = 0.04f;
	[Property, Group( "ViewModel Feel" )] public float SwaySpeed { get; set; } = 5f;
	[Property, Group( "ViewModel Feel" )] public float BreathPositionAmount { get; set; } = 0.08f;
	[Property, Group( "ViewModel Feel" )] public float BreathRotationAmount { get; set; } = 1f;
	[Property, Group( "ViewModel Feel" )] public float BreathSpeed { get; set; } = 2f;
	[Property, Group( "ViewModel Feel" )] public float BobAmount { get; set; } = 0.05f;
	[Property, Group( "ViewModel Feel" )] public float BobSpeed { get; set; } = 8f;

	private Rotation swayLastEyeRot;
	private Vector3 currentAnimPos;
	private Vector3 currentAnimRot;

	private void UpdateViewModelAnimation()
	{
		var targetAnimPos = Vector3.Zero;
		var targetAnimRot = Vector3.Zero;

		if ( !isAiming )
		{
			ApplyBreathing( ref targetAnimPos, ref targetAnimRot );
			ApplyWalkBob( ref targetAnimPos, ref targetAnimRot );
		}

		ApplySway( ref targetAnimPos, ref targetAnimRot );

		currentAnimPos = currentAnimPos.LerpTo( targetAnimPos, 10f * Time.Delta );
		currentAnimRot = currentAnimRot.LerpTo( targetAnimRot, 10f * Time.Delta );
	}

	private void ApplyBreathing( ref Vector3 pos, ref Vector3 rot )
	{
		float t = Time.Now * BreathSpeed;
		pos.z += MathF.Sin( t ) * BreathPositionAmount;
		rot.x += MathF.Cos( t * 0.8f ) * BreathRotationAmount;
		rot.y += MathF.Cos( t * 0.6f ) * BreathRotationAmount * 0.5f;
	}

	private void ApplyWalkBob( ref Vector3 pos, ref Vector3 rot )
	{
		var velocity = PlayerController.Velocity;
		float speed = new Vector3( velocity.x, velocity.y, 0f ).Length;

		if ( speed < 10f )
			return;

		float normalizedSpeed = MathF.Min( speed / 200f, 1f );
		float t = Time.Now * BobSpeed;

		pos.z += MathF.Sin( t ) * BobAmount * normalizedSpeed;
		pos.x += MathF.Cos( t * 0.5f ) * BobAmount * 0.5f * normalizedSpeed;
		rot.z += MathF.Cos( t * 0.5f ) * normalizedSpeed;
	}

	private void ApplySway( ref Vector3 pos, ref Vector3 rot )
	{
		if ( Scene.Camera is null )
			return;

		float swayLerpSpeed = isAiming ? SwaySpeed * 3f : SwaySpeed;
		swayLastEyeRot = Rotation.Lerp( swayLastEyeRot, Scene.Camera.WorldRotation, swayLerpSpeed * Time.Delta );

		var angDiff = Scene.Camera.WorldRotation.Angles() - swayLastEyeRot.Angles();

		pos.x += Math.Clamp( angDiff.yaw * SwayAmount, -1.5f, 1.5f );
		pos.z += Math.Clamp( angDiff.pitch * SwayAmount, -1.5f, 1.5f );
		rot.x += Math.Clamp( angDiff.pitch * 0.2f, -4f, 4f );
		rot.y += Math.Clamp( angDiff.yaw * 0.2f, -4f, 4f );
	}
}
