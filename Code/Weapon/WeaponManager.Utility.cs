using Sandbox;

public sealed partial class WeaponManager : Component
{
	private IGameDamageable FindDamageableOnObject( GameObject hitObject )
	{
		if ( hitObject is null )
			return null;

		foreach ( var component in hitObject.Components.GetAll() )
		{
			if ( component is IGameDamageable damageable )
			{
				return damageable;
			}
		}

		return null;
	}

	private void PlaySoundIfValid( SoundEvent sound )
	{
		if ( sound is null )
			return;

		GameObject.PlaySound( sound, Vector3.Zero );
	}
}
