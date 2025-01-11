using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public class UIAuctionFavorites : MonoBehaviour
    {
        [SerializeField] private GameObject panel;
        [SerializeField] private Transform content;
        [SerializeField] private GameObject prefab;

        [Header("Components")]
        [SerializeField] private UIAuctionShowAllItems allItems;

        public void Show() { panel.SetActive(!panel.activeSelf); }

        private void Update()
        {
            if (panel.activeSelf)
            {
                Player player = Player.localPlayer;
                if (player != null)
                {
                    // instantiate/destroy enough slots
                    UIUtils.BalancePrefabs(prefab, player.auction.favorites.Count, content);

                    for (int i = 0; i < player.auction.favorites.Count; i++)
                    {
                        UIAuctionFavoritesSlot slot = content.transform.GetChild(i).GetComponent<UIAuctionFavoritesSlot>();
                        slot.textName.text = player.auction.favorites[i];

                        int icopy = i;
                        slot.delete.onClick.SetListener(() =>
                        {
                            player.auction.CmdRemoveFromFavorites(player.auction.favorites[icopy]);
                        });

                        slot.button.onClick.SetListener(() =>
                        {
                            player.auction.searchState = PlayerAuction.SearchState.byName;
                            //allItems.buttonSearch.interactable = false;
                            //allItems.buttonSearchByName.interactable = false;

                            player.auction.CmdSearchItemsByName(player.auction.favorites[icopy]);
                        });
                    }
                }
                else panel.SetActive(false);
            }
        }
    }
}