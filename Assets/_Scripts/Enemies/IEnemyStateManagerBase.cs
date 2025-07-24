namespace _Scripts.Enemies
{
    public interface IEnemyStateManagerBase
    {
        // Used to transition an enemy to disabled state AND generate sin, called when a card collides with an enemy
        void KillEnemy();
        void KillEnemyWithoutGeneratingSin();
        // Called from the SkreecherAggroState, puts nearby enemies into their aggro state with a bool that it came from the skreecher
        // Enemies will track the player's position at the time of the call
        void AlertFromAggroSkreecher();
        // Called from the SkreecherInvestigatingState, puts nearby enemies into their aggro state with a bool that it came from the skreecher
        // Enemies will track the card's false trigger position at the time of the call
        void AlertFromInvestigatingSkreecher();
        // Shared States between different enemy types
        bool IsPatrollingState();
        bool IsDetectingState();
        bool IsAlertedState();
        bool IsInvestigatingState();
        bool IsDisabledState();
        
        // For alternate enemy types just return false

        // Guard-Specific
        bool IsAggroState(); // Guard & Bat
        bool IsStunnedState();
        
        // Sniper Specific
        bool IsChargingState();
        bool IsReloadingState();
    }
}