using Sandbox;

public sealed class AutoDestroy : Component
{
	[Property] public float Lifetime { get; set; } = 2f;

	private float spawnTime;

	protected override void OnStart()
	{
		spawnTime = Time.Now;
	}

	protected override void OnUpdate()
	{
		if ( Time.Now - spawnTime >= Lifetime )
		{
			GameObject.Destroy();
		}
	}
}
