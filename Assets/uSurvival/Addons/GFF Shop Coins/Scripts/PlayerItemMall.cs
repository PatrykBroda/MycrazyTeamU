using Mirror;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public enum ShopType { Equipment, Weapons, Ammo, Powers, Currencies, Suits, Bundles, WeaponSkins }

    [Serializable]
    public partial struct ItemMallCategory
    {
        public string category;
        public ShopType type;
        public ScriptableItem[] items;
    }

    [Serializable]
    public class SellingCoinsAndValue
    {
        public byte id;
        public uint price;
        public uint amount;
    }

    [Serializable]
    public class ResponseFromVk
    {
        public string status;
        public string error;
        public string errmsg;
        public string url;
        public string cursor;
        public PaymentsFromVk[] items;
    }

    [Serializable]
    public class PaymentsFromVk
    {
        public string tid;
        public string uid;
        public string currency;
        public string amount;
    }

    [RequireComponent(typeof(PlayerChat))]
    [RequireComponent(typeof(PlayerInventory))]
    [DisallowMultipleComponent]
    public class PlayerItemMall : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;
        public PlayerChat chat;
        public PlayerInventory inventory;

        [Header("Item Mall")]
        public ScriptableItemMall config;
        [SyncVar] public uint coins = 0;
        public float couponWaitSeconds = 3;
        public ScriptableItem shopItem;

        //private int requestCoins = 0;
        public List<SellingCoinsAndValue> sellingCoins;

        [Header("Vk Settings")]
        private string secret = "1ut5Z5tOEVVeQ7rQ";

        public override void OnStartServer()
        {
            InvokeRepeating(nameof(ProcessCoinOrders), 5, 5);
        }

        // item mall ///////////////////////////////////////////////////////////////
        [Command]
        public void CmdEnterCoupon(string coupon)
        {
            // only allow entering one coupon every few seconds to avoid brute force
            if (NetworkTime.time >= player.nextRiskyActionTime)
            {
                // YOUR COUPON VALIDATION CODE HERE
                // coins += ParseCoupon(coupon);
                //Debug.Log("coupon: " + coupon + " => " + name + "@" + NetworkTime.time);
                player.nextRiskyActionTime = NetworkTime.time + couponWaitSeconds;
            }
        }

        [Command]
        public void CmdUnlockItem(int categoryIndex, int itemIndex)
        {
            // validate: only if alive so people can't buy resurrection potions
            // after dieing in a PvP fight etc.
            if (player.health.current > 0 &&
                0 <= categoryIndex && categoryIndex <= config.categories.Length &&
                0 <= itemIndex && itemIndex <= config.categories[categoryIndex].items.Length)
            {
                Item item = new Item(config.categories[categoryIndex].items[itemIndex]);
                if (0 < item.itemMallPrice && item.itemMallPrice <= coins)
                {
                    // try to add it to the inventory, subtract costs from coins
                    if (inventory.Add(item, 1, false))
                    {
                        coins -= item.itemMallPrice;
                        //Debug.Log(name + " unlocked " + item.name);

                        // NOTE: item mall purchases need to be persistent, yet
                        // resaving the player here is not necessary because if the
                        // server crashes before next save, then both the inventory
                        // and the coins will be reverted anyway.
                    }
                }
            }
        }

        // coins can't be increased by an external application while the player is
        // ingame. we use an additional table to store new orders in and process
        // them every few seconds from here. this way we can even notify the player
        // after his order was processed successfully.
        //
        // note: the alternative is to keep player.coins in the database at all
        // times, but then we need RPCs and the client needs a .coins value anyway.
        [Server]
        private void ProcessCoinOrders()
        {
            List<uint> orders = Database.singleton.GrabCharacterOrders(player.account);
            foreach (uint reward in orders)
            {
                coins += GetAmountByPrice(reward);
                UnityEngine.Debug.Log("Processed order for: " + name + ";" + reward);
                chat.TargetMsgInfo("Куплено: " + reward + "Черепов");
            }
        }

        private uint GetAmountByPrice(uint price)
        {
            for (int i = 0; i < sellingCoins.Count; i++)
            {
                if (sellingCoins[i].price == price) return sellingCoins[i].amount;
            }

            return 0;
        }

        public ScriptableItem[] GetShopItemByType(ShopType type)
        {
            for (int i = 0; i < config.categories.Length; i++)
            {
                if (config.categories[i].type == type) return config.categories[i].items;
            }

            return null;
        }
        public int GetShopTypeIndex(ShopType type)
        {
            for (int i = 0; i < config.categories.Length; i++)
            {
                if (config.categories[i].type == type) return i;
            }
            return -1;
        }

        [Command]
        public void CmdBuyItem(ShopType type, short itemIndex, ushort amount)
        {
            if (amount > 0)
            {
                int categoryIndex = GetShopTypeIndex(type);

                if (0 <= categoryIndex && categoryIndex <= config.categories.Length &&
                    0 <= itemIndex && itemIndex <= config.categories[categoryIndex].items.Length)
                {
                    Item item = new Item(config.categories[categoryIndex].items[itemIndex]);
                    if ((item.itemMallPrice * amount) <= coins)
                    {
                        if (type == ShopType.Suits && item.data is EquipmentItem eItem)
                        {
                            if (player.customization.SetNewItem(EquipmentItemType.Suit, eItem) == true)
                            {
                                coins -= item.itemMallPrice;

                                BoughtSuits suit = new BoughtSuits();
                                suit.classname = player.className;
                                suit.suitname = eItem.name;
                                player.customization.boughtSuits.Add(suit);

                                UnityEngine.Debug.Log(name + " succes buy suit " + item.name);
                            }
                            else UnityEngine.Debug.Log(name + " not succes buy suit " + item.name);
                        }
                        else if (type == ShopType.Ammo)
                        {
                            // try to add it to the inventory, subtract costs from coins
                            if (inventory.Add(item, (ushort)(300 * amount), item.data.autoBind))
                            {
                                coins -= (item.itemMallPrice * amount);
                                RpcSendInfo(true);

                                // NOTE: item mall purchases need to be persistent, yet
                                // resaving the player here is not necessary because if the
                                // server crashes before next save, then both the inventory
                                // and the coins will be reverted anyway.
                            }
                            else RpcSendInfo(false);
                        }
                        else
                        {
                            // try to add it to the inventory, subtract costs from coins
                            if (inventory.Add(item, amount, item.data.autoBind))
                            {
                                coins -= item.itemMallPrice;
                                RpcSendInfo(true);

                                // NOTE: item mall purchases need to be persistent, yet
                                // resaving the player here is not necessary because if the
                                // server crashes before next save, then both the inventory
                                // and the coins will be reverted anyway.
                            }
                            else RpcSendInfo(false);
                        }
                    }
                    else RpcSendInfo(false);
                }
            }
        }

        [Command]
        public void CmdBuyBundle(int index)
        {
            if (config.bundles[index].price <= coins)
            {
                coins -= config.bundles[index].price;
                for (int i = 0; i < config.bundles[index].items.Length; i++)
                {
                    player.inventory.Add(new Item(config.bundles[index].items[i].item), config.bundles[index].items[i].amount, config.bundles[index].items[i].item.autoBind);
                }
            }

        }

        [Command]
        public void CmdBuyWeaponSkin(int itemIndex)
        {
            int categoryIndex = GetShopTypeIndex(ShopType.WeaponSkins);

            if (0 <= categoryIndex && categoryIndex <= config.categories.Length &&
                0 <= itemIndex && itemIndex <= config.categories[categoryIndex].items.Length)
            {
                //Item item = new Item(config.categories[categoryIndex].items[itemIndex]);
                //if (item.itemMallPrice <= coins)
                //{
                //    if (item.data is ScriptableWeaponSkin weaponSkin)
                //    {
                //        if (player.weaponSkins.IsSkinAlreadyPurchased(weaponSkin.weapon.name, item.name) == false)
                //        {
                //            coins -= item.itemMallPrice;
                //            player.weaponSkins.skins.Add(new WeaponSkin(weaponSkin.weapon.name, item.name));
                //        }
                //    }
                //}
                //else RpcSendInfo(false);
            }
        }

        [TargetRpc]
        private void RpcSendInfo(bool result)
        {
            UIShop.singleton.ShowConfirmation(result);
        }

        [Command]
        public void CmdExchangeCoinsItem(ushort amount)
        {
            ushort amountInInventory = inventory.Count(shopItem);
            if (amount <= amountInInventory)
            {
                coins += amount;

                inventory.Remove(shopItem, amount);
            }
        }

        [Command]
        public void CmdGetLinkForBuyCoinsFromVk(byte item_id)
        {
            GenerateLinkVk(item_id);
        }

        [Server]
        private void GenerateLinkVk(byte item_id)
        {
            string user_id = player.account;
            string user_ip = player.netIdentity.connectionToClient.address;
            //user_ip = user_ip.Substring(7);

            Debug.Log("GenerateLinkVk " + user_ip);

            string merchant_param = String.Format("{{\"uid\": \"{0}\", \"ip\": \"{1}\"}}", user_id, user_ip);
            string sign_body = String.Format("ids={0}merchant_param={1}{2}", item_id, merchant_param, secret);
            var provider = MD5.Create();
            byte[] sign_bytes = provider.ComputeHash(Encoding.UTF8.GetBytes(sign_body));
            string sign_string = BitConverter.ToString(sign_bytes).Replace("-", "");

            string url = string.Concat("https://vkplay.ru/app/21095/billing/item/client?sign=", sign_string);

            var request_values = new Dictionary<string, string>(){
             { "sign", sign_string },
             { "ids", item_id.ToString() },
             { "merchant_param", merchant_param }};

            var content = new FormUrlEncodedContent(request_values);
            SendMessageVerificationCode(url, content);
        }

        private async void SendMessageVerificationCode(string uri, HttpContent data)
        {
            var client = new HttpClient();

            // отправляем запрос
            using var response = await client.PostAsync(uri, data);

            // получаем ответ
            string responseText = await response.Content.ReadAsStringAsync();

            ResponseFromVk deserializedPostData = JsonUtility.FromJson<ResponseFromVk>(responseText);
            RpcReturnGeneratedUrlFromVk(deserializedPostData.url);

            Debug.Log("GenerateLink sendsd to client " + deserializedPostData.url);
        }

        [TargetRpc]
        private void RpcReturnGeneratedUrlFromVk(string url)
        {
            Application.OpenURL(url);
        }
    }
}