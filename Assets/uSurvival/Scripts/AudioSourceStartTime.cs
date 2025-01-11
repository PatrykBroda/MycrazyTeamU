using UnityEngine;

namespace uSurvival
{
    [RequireComponent(typeof(AudioSource))]
    public class AudioSourceStartTime : MonoBehaviour
    {
    #pragma warning disable CS0109 // member does not hide accessible member
        public new AudioSource audio;
    #pragma warning restore CS0109 // member does not hide accessible member
        public float time = 0;

        void Start()
        {
            audio.time = time;
        }
    }
}