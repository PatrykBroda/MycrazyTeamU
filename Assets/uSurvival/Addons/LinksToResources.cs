using System;
using UnityEngine;
using UnityEngine.UI;
using uSurvival;

namespace GFFAddons
{
    [Serializable]public class LinkToResource
    {
        public string name;
        public Button button;
        public string url;
    }

    public class LinksToResources : MonoBehaviour
    {
        [SerializeField] private LinkToResource[] links;
        [SerializeField] private AudioSource soundSourņe;

        private void Start()
        {
            for (int i = 0; i < links.Length; i++)
            {
                int icopy = i;
                links[icopy].button.onClick.SetListener(() =>
                {
                    if (soundSourņe)soundSourņe.Play();
                    Application.OpenURL(links[icopy].url);
                });
            }
        }
    }
}