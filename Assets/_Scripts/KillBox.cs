using System;
using System.Collections;
using _Scripts.Card;
using _Scripts.Enemies;
using _Scripts.Sound;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _Scripts
{
    [RequireComponent(typeof(Collider2D))]
    public class KillBox : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D col)
        {
            if (col.gameObject.CompareTag("Player"))
                StartCoroutine(WaitBeforeKill(col));

            if (col.gameObject.CompareTag("Card"))
                StartCoroutine(WaitBeforeKill(col));

            if (col.gameObject.CompareTag("enemy"))
                StartCoroutine(WaitBeforeKill(col));
        }
        
        private IEnumerator WaitBeforeKill(Collider2D col)
        {
            yield return new WaitForSeconds(0.15f);
            
            if (col is null) yield return null;

            try
            {
                if (col.gameObject.CompareTag("Player"))
                {
                    // TODO: Hook up to the cleanup handler when it is made
                    //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    LavaSoundManager.Instance.PlayLavaSizzleClip();
                    GameManager.Instance.Die();
                } 
            
                if (col.gameObject.CompareTag("Card"))
                {
                    if (Card.Card.Instance is not null && CardManager.Instance.cardPrefab is not null)
                    {
                        CardEffectHandler.Instance.DestroyEffect(Card.Card.Instance.transform.position);
                        LavaSoundManager.Instance.PlayLavaSizzleClip();
                        Card.Card.Instance.DestroyCard();
                    }
                } 
            
                if (col.gameObject.CompareTag("enemy"))
                {
                    var enemy = col.gameObject.GetComponent<IEnemyStateManagerBase>();
                    LavaSoundManager.Instance.PlayLavaSizzleClip();
                    enemy.KillEnemyWithoutGeneratingSin();
                }
            }
            catch (Exception e)
            {
                Debug.Log("CATCH!");
            }
        }
    }
}