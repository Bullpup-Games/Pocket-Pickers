using UnityEngine;

namespace _Scripts.Sound
{
    public class CardSoundEffectManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip teleportClip;
        [SerializeField] private AudioClip falseTriggerClip;
        [SerializeField] private AudioClip cardDestroyClip;
        [SerializeField] private AudioClip enemyHitClip;
        [SerializeField] private AudioClip[] cardThrowClips;
        [SerializeField] private AudioClip[] cardHitClips;

        private int _lastPlayedIndex = -1;
        
        #region Singleton

        public static CardSoundEffectManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(CardSoundEffectManager)) as CardSoundEffectManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static CardSoundEffectManager _instance;

        #endregion

        private void Start()
        {
            if (cardThrowClips is null || cardThrowClips.Length == 0)
                Debug.Log("No footstep clips assigned to FootStepManager.");
        }

        public void PlayCardThrowClip()
        {
            if (cardThrowClips is null || cardThrowClips.Length == 0)
            {
                Debug.Log("Error trying to find footstep clips.");
                return;
            }

            // Choose a random clip, excluding the one that played last to give a more 'random' feeling
            var newIndex = GetRandomClipIndexExcluding(_lastPlayedIndex);
            _lastPlayedIndex = newIndex;

            if (cardThrowClips is null) return;
            
            // Randomize pitch slightly before playing
            audioSource.pitch = Random.Range(0.9f, 1.1f);
                
            var clipToPlay = cardThrowClips[newIndex];
            audioSource.PlayOneShot(clipToPlay);
        }

        public void PlayCardHitClip()
        {
            if (cardHitClips is null || cardHitClips.Length == 0)
            {
                Debug.Log("Error trying to find footstep clips.");
                return;
            }

            // Choose a random clip, excluding the one that played last to give a more 'random' feeling
            var newIndex = GetRandomClipIndexExcluding(_lastPlayedIndex);
            _lastPlayedIndex = newIndex;

            if (cardHitClips is null) return;
            
            // Randomize pitch slightly before playing
            audioSource.pitch = Random.Range(0.9f, 1.1f);
                
            var clipToPlay = cardHitClips[newIndex];
            audioSource.PlayOneShot(clipToPlay); 
        }

        public void PlayTeleportClip()
        {
            if (teleportClip is null) return;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(teleportClip);
        }

        public void PlayFalseTriggerClip()
        {
            if (falseTriggerClip is null) return;
            if (cardDestroyClip is null) return;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(cardDestroyClip);
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(falseTriggerClip);
            
        }

        public void PlayCardDestroyClip()
        {
            if (cardDestroyClip is null) return;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(cardDestroyClip); 
        }
        
        public void PlayEnemyHitClip()
        {
            if (cardDestroyClip is null) return;
            if (enemyHitClip is null) return;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(enemyHitClip); 
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(cardDestroyClip); 
        }

        private int GetRandomClipIndexExcluding(int excludeIndex)
        {
            var randomIndex = Random.Range(0, cardThrowClips.Length);
        
            while (randomIndex == excludeIndex)
                randomIndex = Random.Range(0, cardThrowClips.Length);
        
            return randomIndex;
        } 
    }
}