using Assets._Scripts.Input;
using Assets._Scripts.Stats;
using UnityEngine;

namespace Assets._Scripts.Context
{
    public class PlayerContext : EntityContext
    {
        public PlayerStats Stats;
        public InputHandler Input;

        /// <summary>
        /// Overrides EntityContext to input acces to PlayerStats and the InputHandler 
        /// </summary>
        public PlayerContext(Rigidbody rb, Collider col, Transform transform, int health, float gravity, PlayerStats stats, InputHandler input)
            : base(rb, col, transform, health, gravity)
        {
            Stats = stats;
            Input = input;
        }

        public void FlipLocalScale()
        {
            var localScale = base.Transform.localScale;
            localScale.x *= -1;
            base.Transform.localScale = localScale;
        }
    }
}