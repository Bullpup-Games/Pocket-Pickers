using UnityEngine;

namespace Assets._Scripts.Entities
{
    public class Player : Entity
    {
        private void Start()
        {
            // Init an Entity with 100 health, -10 grav
            base.Init(100, 10);
        }

        /// <summary>
        /// Player will not have an update loop, this is just here for testing purposes.
        /// </summary>
        void Update()
        {
            base.TakeDamage(10);
        }

        /// <summary>
        /// Override base to trigger player-specific death conditions
        /// </summary>
        public override void Die()
        {
            base.Die();
            Debug.Log("Do player-specific death logic...");
            // TODO: Player based death logic
            // (Game over screen, scene loader, state updates, etc) 
            // Will most likely just want to trigger some global 'OnPlayerDeath' event
        }
    }
}