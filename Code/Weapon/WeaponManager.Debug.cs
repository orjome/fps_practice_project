using Sandbox;

public sealed partial class WeaponManager : Component
{
	private void DrawDebugBulletPath( Vector3 start, Vector3 end, SceneTraceResult trace )
	{
		if ( !ShowDebugBulletPath )
			return;

		if ( DebugDrawFullTrace )
		{
			GameObject.DebugOverlay.Trace( trace, DebugBulletPathDuration, false );
		}

		Vector3 lineEnd = trace.Hit ? trace.HitPosition : end;
		Color lineColor = trace.Hit ? Color.Green : Color.Red;

		GameObject.DebugOverlay.Line( start, lineEnd, lineColor, DebugBulletPathDuration, default, false );
	}
}
