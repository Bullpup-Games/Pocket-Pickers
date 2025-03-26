using System;
using Assets._Scripts.Context;
using Assets._Scripts.Movement;
using UnityEngine;

namespace Assets._Scripts.Entities
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public class Entity : MonoBehaviour
    {
        public EntityContext Context { get; private set; }
        public BaseMovement Movement { get; private set; }

        /// <summary>
        /// Monobehavior derived classes cannot be created with a constructor.
        /// Init handles the setup of a new entity
        /// </summary>
        /// <param name="health"> The entities starting health </param>
        /// <param name="gravity"> Gravity modifier to be used by Movement, Can be +-, does not matter </param>
        public virtual void Init(int health, float gravity)
        {
            Rigidbody rg = GetComponent<Rigidbody>();
            Collider col = GetComponent<Collider>();

            Context = new(rg, col, transform, Math.Abs(health), -Mathf.Abs(gravity));
        }

        /// <summary>
        /// Damage the entity by reducing the stored Health in Context.
        /// If Context.Health reaches 0, Die() is called.
        /// </summary>
        /// <param name="amount"> The amount of damage to take </param>
        public virtual void TakeDamage(int amount)
        {
            Context.Health -= amount;

            if (Context.Health > 0) { return; }

            Die();
        }

        /// <summary>
        /// Destroy this gameObject
        /// </summary>
        public virtual void Die()
        {
            Debug.Log($"{gameObject.name} died.");
            Destroy(gameObject);
        }
    }
}