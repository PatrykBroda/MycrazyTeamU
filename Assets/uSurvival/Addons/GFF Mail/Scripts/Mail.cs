using Mirror;
using System;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public partial struct Mail
    {
        public int id;
        public bool read;
        public string sender;
        public string subject;
        public string message;

        public ItemSlot itemslot;
        public bool itemTake;

        // constructors
        public Mail(int id, bool read, string sender, string subject, string message, ItemSlot slot, bool itemTake)
        {
            this.id = id;
            this.read = read;
            this.sender = sender;
            this.subject = subject;
            this.message = message;

            this.itemTake = itemTake;
            this.itemslot = slot;
        }
    }
    public class SyncListMail : SyncList<Mail> { }
}