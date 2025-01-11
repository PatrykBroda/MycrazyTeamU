using GFFAddons;
using SQLite; // from https://github.com/praeclarum/sqlite-net

namespace uSurvival
{
    public partial class Database
    {
        class character_mail
        {
            [PrimaryKey, AutoIncrement]
            public int id { get; set; }
            public string addressee { get; set; }
            public string sender { get; set; }
            public bool read { get; set; }
            public string topic { get; set; }
            public string message { get; set; }

            public bool itemTake { get; set; }
            public string itemName { get; set; }
            public ushort itemAmount { get; set; }

            //durability addon
            public ushort durability { get; set; }
        }

        public void Connect_Mail()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_mail>();
        }

        public void CharacterLoadMail(Player player)
        {
            player.mail.mail.Clear();

            foreach (character_mail row in connection.Query<character_mail>("SELECT * FROM character_mail WHERE addressee=?", player.name))
            {
                ItemSlot slot = new ItemSlot();
                if (!string.IsNullOrEmpty(row.itemName))
                {
                    if (ScriptableItem.dict.TryGetValue(row.itemName.GetStableHashCode(), out ScriptableItem itemData))
                    {
                        slot.item = new Item(itemData);
                        slot.amount = row.itemAmount;

                        //durability 
                        slot.item.durability = row.durability;

                        //item enchantment addon
                        //slot.item.holes = row.holes;
                        //slot.item.upgradeInd = LoadUpgradeInd(row.upgradeInd);
                    }
                }
                player.mail.mail.Add(new Mail(row.id, row.read, row.sender, row.topic, row.message, slot, row.itemTake));
            }
        }

        public bool SendNewMessage(string addressee, string sender, string topic, string message, bool itemTake, ItemSlot slot)
        {
            if (CharacterCheck(addressee))
            {
                string itemname = "";
                if (slot.amount > 0) itemname = slot.item.name;

                character_mail msg = new character_mail();

                msg.addressee = addressee;
                msg.sender = sender;
                msg.topic = topic;
                msg.message = message;

                msg.itemTake = itemTake;
                msg.itemName = itemname;
                msg.itemAmount = slot.amount;

                //durability 
                msg.durability = slot.item.durability;

                //item enchantment addon
                //msg.holes = slot.item.holes;
                //msg.upgradeInd = SaveUpgradeInd(slot.item);

                connection.Insert(msg);

                return true;
            }
            else return false;
        }

        private bool CharacterCheck(string characterName)
        {
            // checks deleted ones too so we don't end up with duplicates if we un-
            // delete one
            return connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", characterName) != null;
        }

        public void SetMessageRead(int id)
        {
            connection.Execute("UPDATE character_mail SET read = 1 WHERE id =?", id);
        }
        public void SetMessageItemTaken(int id)
        {
            connection.Execute("UPDATE character_mail SET itemTake = 1 WHERE id =?", id);
        }
        public void DeleteMessage(int id)
        {
            connection.Execute("DELETE FROM character_mail WHERE id =?", id);
        }
    }
}




