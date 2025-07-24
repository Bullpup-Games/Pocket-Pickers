using UnityEngine;

namespace _Scripts.Sound
{
    public class SinSoundManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private AudioClip[] sinCollectClips;

        #region Singleton

        public static SinSoundManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(SinSoundManager)) as SinSoundManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static SinSoundManager _instance;

        #endregion

        public void PlaySinCollectedClip()
        {
            if (sinCollectClips is null) return;

            foreach (var clip in sinCollectClips)
            {
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                audioSource.PlayOneShot(clip);
            }
        }
    }
}