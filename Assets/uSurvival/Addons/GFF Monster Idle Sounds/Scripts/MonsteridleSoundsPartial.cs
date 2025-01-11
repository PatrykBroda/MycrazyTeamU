using Mirror;
using UnityEngine;

namespace uSurvival
{
    public partial class Monster
    {
        [Header("GFF Idle Sounds Addon")]
        public float idleTimeWaitingMin = 30;
        public float idleTimeWaitingMax = 90;
        public AudioClip[] idleSounds;
        private double nextIdleSound;

        public override void OnStartClient()
        {
            base.OnStartClient();

            nextIdleSound = NetworkTime.time + Random.Range(idleTimeWaitingMin, idleTimeWaitingMax);
        }

        private void IdleSounds()
        {
            if (NetworkTime.time > nextIdleSound && audioSource.isPlaying == false)
            {
                nextIdleSound = NetworkTime.time + Random.Range(idleTimeWaitingMin, idleTimeWaitingMax);
                PlayIdleAudio();
            }
        }
        private void PlayIdleAudio()
        {
            // pick & play a random footstep sound from the array,
            // excluding sound at index 0
            if (idleSounds != null && idleSounds.Length > 0)
            {
                int n = Random.Range(0, idleSounds.Length);
                audioSource.clip = idleSounds[n];
                if (audioSource.clip != null) audioSource.PlayOneShot(audioSource.clip);
            }
        }
    }
}




