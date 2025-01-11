using UnityEngine;

namespace GFFAddons
{
    [CreateAssetMenu(menuName = "GFF Addons/Chat Filter Data", order = 999)]
    public class ChatWordFilter : ScriptableObject
    {
        [SerializeField] private string[] words;

        public string CheckMessage(string message)
        {
            string reply = message;

            for (int i = 0; i < words.Length; i++)
            {
                if (message.Contains(words[i]))
                {
                    reply = reply.Replace(words[i], " *** ");
                }
            }

            return reply;
        }
    }
}
