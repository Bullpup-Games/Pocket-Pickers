using _Scripts.Sound;
using UnityEngine;

namespace _Scripts
{
    public class Sin : MonoBehaviour
    {
        // Start is called before the first frame update

        public int weight;
        public Vector3 location;
        void Awake()
        {
                if (location == Vector3.zero) {
                    location = gameObject.transform.position;
                }
                
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                Debug.Log("Collided with a sin");
                Debug.Log("Colision is with " + col.gameObject.name);
                Physics2D.IgnoreCollision(col.collider, GetComponent<Collider2D>());

                Debug.Log("Passing in transform of " +gameObject.transform.position);

                SinSoundManager.Instance.PlaySinCollectedClip();
                
                GameManager.Instance.CollectSin(gameObject);
                DestroySin();
            }
        }

        public void DestroySin()
        {
            Destroy(gameObject); 
        }
    }
}