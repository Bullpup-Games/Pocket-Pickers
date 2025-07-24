using UnityEngine;

namespace _Scripts.Sound
{
    public class LavaSoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip sizzleClip;

        private int _lastPlayedIndex = -1;
        
        #region Singleton

        public static LavaSoundManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(LavaSoundManager)) as LavaSoundManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static LavaSoundManager _instance;

        #endregion


        public void PlayLavaSizzleClip()
        {
            if (sizzleClip is null) return;
            audioSource.pitch = Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(sizzleClip);
        }
    }
}