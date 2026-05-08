using Sandbox;
using System;

public sealed class XpManager : Component
{
	[Property] public int StartingLevel { get; set; } = 1;
	[Property] public int StartingXp { get; set; } = 0;
	[Property] public int BaseXpToNextLevel { get; set; } = 100;
	[Property] public float XpGrowthMultiplier { get; set; } = 1.5f;

	private int level;
	private int currentXp;
	private int xpToNextLevel;

	public int Level => level;
	public int CurrentXP => currentXp;
	public int XPToNextLevel => xpToNextLevel;

	protected override void OnStart()
	{
		level = Math.Max( 1, StartingLevel );
		currentXp = Math.Max( 0, StartingXp );
		xpToNextLevel = CalculateXpToNextLevel( level );

		CheckForLevelUps();

		Log.Info( $"XP system started. Level: {level}, XP: {currentXp}/{xpToNextLevel}" );
	}

	public void AddXP( int amount )
	{
		if ( amount <= 0 )
			return;

		currentXp += amount;

		Log.Info( $"Gained {amount} XP. XP: {currentXp}/{xpToNextLevel}" );

		CheckForLevelUps();
	}

	private void CheckForLevelUps()
	{
		while ( currentXp >= xpToNextLevel )
		{
			currentXp -= xpToNextLevel;
			level++;

			xpToNextLevel = CalculateXpToNextLevel( level );

			Log.Info( $"Level up! New level: {level}. XP: {currentXp}/{xpToNextLevel}" );
		}
	}

	private int CalculateXpToNextLevel( int currentLevel )
	{
		float scaledXp = BaseXpToNextLevel * MathF.Pow( XpGrowthMultiplier, currentLevel - 1 );
		return Math.Max( 1, (int)MathF.Round( scaledXp ) );
	}
	public void ResetXP()
	{
		level = Math.Max( 1, StartingLevel );
		currentXp = Math.Max( 0, StartingXp );
		xpToNextLevel = CalculateXpToNextLevel( level );

		CheckForLevelUps();

		Log.Info( $"XP reset. Level: {level}, XP: {currentXp}/{xpToNextLevel}" );
	}
}
