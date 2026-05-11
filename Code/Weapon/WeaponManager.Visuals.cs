using Sandbox;

public sealed partial class WeaponManager : Component
{
	[Property, Group( "Visuals" )]
	public GameObject ViewModelRoot { get; set; }

	[Property, Group( "Visuals" )]
	public GameObject WorldModelRoot { get; set; }

	[Property, Group( "Visuals" )]
	public bool ShowViewModel { get; set; } = true;

	[Property, Group( "Visuals" )]
	public bool ShowWorldModel { get; set; } = true;

	[Property, Group( "Visuals" )]
	public bool ViewModelRootFollowsCamera { get; set; } = true;

	private GameObject activeViewModel;
	private GameObject activeWorldModel;

	private void UpdateWeaponVisuals()
	{
		if ( PlayerController is null )
			return;

		if ( ViewModelRootFollowsCamera && ViewModelRoot.IsValid() )
		{
			var eye = PlayerController.EyeTransform;
			ViewModelRoot.WorldPosition = eye.Position;
			ViewModelRoot.WorldRotation = eye.Rotation;
		}

		UpdateViewModelAimOffset();
	}

	private void UpdateViewModelAimOffset()
	{
		if ( !activeViewModel.IsValid() || CurrentWeapon is null )
			return;

		UpdateViewModelAnimation();

		float speed = isAiming ? CurrentWeapon.AimInSpeed : CurrentWeapon.AimOutSpeed;

		Vector3 targetPos = isAiming
			? CurrentWeapon.AimPositionOffset
			: CurrentWeapon.ViewModelPositionOffset;

		Rotation targetRot = isAiming
			? Rotation.From( CurrentWeapon.AimRotationOffset )
			: Rotation.From( CurrentWeapon.ViewModelRotationOffset );

		var animRot = new Angles( currentAnimRot.x, currentAnimRot.y, currentAnimRot.z );

		activeViewModel.LocalPosition = activeViewModel.LocalPosition.LerpTo( targetPos + currentAnimPos, speed * Time.Delta );
		activeViewModel.LocalRotation = Rotation.Lerp( activeViewModel.LocalRotation, Rotation.From( targetRot.Angles() + animRot ), speed * Time.Delta );
	}

	private void RefreshWeaponVisuals()
	{
		ClearWeaponVisuals();

		if ( CurrentWeapon is null )
			return;

		CreateViewModel();
		CreateWorldModel();
	}

	private void ClearWeaponVisuals()
	{
		if ( activeViewModel.IsValid() )
		{
			activeViewModel.Destroy();
			activeViewModel = null;
		}

		if ( activeWorldModel.IsValid() )
		{
			activeWorldModel.Destroy();
			activeWorldModel = null;
		}
	}

	private void CreateViewModel()
	{
		if ( !ShowViewModel )
			return;

		if ( !ViewModelRoot.IsValid() )
		{
			Log.Warning( "WeaponManager has no ViewModelRoot assigned." );
			return;
		}

		if ( !CurrentWeapon.ViewModelPrefab.IsValid() )
			return;

		activeViewModel = CurrentWeapon.ViewModelPrefab.Clone(
			ViewModelRoot,
			Vector3.Zero,
			Rotation.Identity,
			new Vector3( CurrentWeapon.ViewModelScale, CurrentWeapon.ViewModelScale, CurrentWeapon.ViewModelScale )
		);

		activeViewModel.Name = $"{CurrentWeapon.DisplayName}_ViewModel";
		activeViewModel.LocalPosition = CurrentWeapon.ViewModelPositionOffset;
		activeViewModel.LocalRotation = Rotation.From( CurrentWeapon.ViewModelRotationOffset );
		activeViewModel.LocalScale = new Vector3(
			CurrentWeapon.ViewModelScale,
			CurrentWeapon.ViewModelScale,
			CurrentWeapon.ViewModelScale
		);

		activeViewModel.Tags.Add( "viewmodel" );
	}

	private void CreateWorldModel()
	{
		if ( !ShowWorldModel )
			return;

		if ( !WorldModelRoot.IsValid() )
			return;

		if ( !CurrentWeapon.WorldModelPrefab.IsValid() )
			return;

		activeWorldModel = CurrentWeapon.WorldModelPrefab.Clone(
			WorldModelRoot,
			Vector3.Zero,
			Rotation.Identity,
			new Vector3( CurrentWeapon.WorldModelScale, CurrentWeapon.WorldModelScale, CurrentWeapon.WorldModelScale )
		);

		activeWorldModel.Name = $"{CurrentWeapon.DisplayName}_WorldModel";
		activeWorldModel.LocalPosition = CurrentWeapon.WorldModelPositionOffset;
		activeWorldModel.LocalRotation = Rotation.From( CurrentWeapon.WorldModelRotationOffset );
		activeWorldModel.LocalScale = new Vector3(
			CurrentWeapon.WorldModelScale,
			CurrentWeapon.WorldModelScale,
			CurrentWeapon.WorldModelScale
		);
	}
}
