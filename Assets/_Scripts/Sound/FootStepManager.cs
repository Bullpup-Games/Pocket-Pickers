using System.Collections;
using _Scripts.Player;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Scripts.Sound
{
    public class FootStepManager : MonoBehaviour
    {
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float footstepCooldown = 0.1f;
        [SerializeField] private AudioClip[] footstepClips;

        private int _lastPlayedIndex = -1;
        private bool _cd;

        private void Start()
        {
            if (footstepClips is null || footstepClips.Length == 0)
                Debug.Log("No footstep clips assigned to FootStepManager.");
        }

        private void Update()
        {
            if (!PlayerMovement.Instance.IsGrounded() || !(Mathf.Abs(PlayerMovement.Instance.FrameInput.x) > 0f)) return;
            if (!_cd) StartCoroutine(PlayFootstep());
        }

        private IEnumerator PlayFootstep()
        {
            if (footstepClips is null || footstepClips.Length == 0)
            {
                Debug.Log("Error trying to find footstep clips.");
                yield return null;
            }

            _cd = true;
            // Choose a random clip, excluding the one that played last to give a more 'random' feeling
            var newIndex = GetRandomClipIndexExcluding(_lastPlayedIndex);
            _lastPlayedIndex = newIndex;

            // Confirm the player is still grounded and moving them player the clip
            if (!PlayerMovement.Instance.IsGrounded() || !(Mathf.Abs(PlayerMovement.Instance.FrameInput.x) > 0f)) 
                yield return null;

            if (footstepClips is not null)
            {
                // Randomize pitch slightly before playing
                audioSource.pitch = Random.Range(0.9f, 1.1f);
                
                var clipToPlay = footstepClips[newIndex];
                audioSource.PlayOneShot(clipToPlay);
            }

            // wait before allowing a followup clip
            yield return new WaitForSeconds(footstepCooldown);
            _cd = false;
        }

        private int GetRandomClipIndexExcluding(int excludeIndex)
        {
            var randomIndex = Random.Range(0, footstepClips.Length);
        
            while (randomIndex == excludeIndex)
                randomIndex = Random.Range(0, footstepClips.Length);
        
            return randomIndex;
        }
    }
}