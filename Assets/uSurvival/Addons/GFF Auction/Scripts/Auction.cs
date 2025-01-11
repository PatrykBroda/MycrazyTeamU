using Mirror;
using System;
using uSurvival;

namespace GFFAddons
{
    [Serializable]
    public partial struct Auction
    {
        public int id;
        public string owner;
        public string dateRegistration;
        public string buyer;
        public string dateBuy;
        public bool getMoney;
        public string category;
        public string subcategory;
        public uint price;
        public ItemSlot itemslot;

        // constructors
        public Auction(int id, string owner, string dateRegistration, string buyer, string dateBuy, bool getMoney, string category, string subcategory, uint price, ItemSlot slot)
        {
            this.id = id;
            this.owner = owner;
            this.dateRegistration = dateRegistration;
            this.buyer = buyer;
            this.dateBuy = dateBuy;
            this.getMoney = getMoney;
            this.category = category;
            this.subcategory = subcategory;
            this.price = price;
            this.itemslot = slot;
        }
    }
    public class SyncListAuction : SyncList<Auction> { }
}

