using Mirror;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    [DisallowMultipleComponent]
    public class PlayerFriends : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        public readonly SyncList<Friend> friends = new SyncList<Friend>();
        public readonly SyncList<Friend> friendRequests = new SyncList<Friend>();

        public override void OnStartClient()
        {
            base.OnStartClient();

            CmdSetOnlineForFriends(true);
            CmdCheckFriendsOnlineStatus();
        }

        void OnDestroy()
        {
            if (NetworkServer.active) // isServer
            {
                // notify friends that we are offline
                SetOnlineForFriends(false);
            }
        }

        // find friend index by name
        // (avoid FindIndex for performance/allocations)
        public int GetFriendIndex(string friend)
        {
            for (int i = 0; i < friends.Count; ++i)
                if (friends[i].name == friend)
                    return i;
            return -1;
        }
        private int GetFriendRequestIndex(string friend)
        {
            for (int i = 0; i < friendRequests.Count; ++i)
                if (friendRequests[i].name == friend)
                    return i;
            return -1;
        }
        public bool CheckFriendByName(string name)
        {
            for (int i = 0; i < friends.Count; i++)
            {
                if (friends[i].name == name) return true;
            }
            return false;
        }

        [Command]public void CmdSendFriendRequest(string newfriend)
        {
            // validate: is there someone with that name, and not self?
            if (GetFriendIndex(newfriend) == -1 && newfriend != name && NetworkTime.time >= player.nextRiskyActionTime)
            {
                Friend friend = new Friend()
                {
                    name = name,
                    className = player.className,
                    online = true
                };

                if (Player.onlinePlayers.TryGetValue(newfriend, out Player other))
                {
                    if (other.friends.friendRequests.Contains(friend) == false)
                        other.friends.friendRequests.Add(friend);
                }
                else
                {
                    if (Database.singleton.AddFriendRequest(newfriend, friend) == false) RpcMessageFromTheServer("Player " + newfriend + " not found");
                }

                // reset risky time
                player.nextRiskyActionTime = NetworkTime.time + 5;
            }
        }

        [Command]public void CmdAcceptFriendRequest(string fromPlayer)
        {
            //check request
            int index = GetFriendRequestIndex(fromPlayer);
            if (index != -1)
            {
                if (GetFriendIndex(fromPlayer) == -1)
                {
                    friends.Add(friendRequests[index]);

                    // find sender and add him a new friend
                    if (Player.onlinePlayers.ContainsKey(fromPlayer))
                    {
                        Friend friend = new Friend()
                        {
                            name = name,
                            className = player.className,
                            online = true
                        };

                        Player.onlinePlayers[fromPlayer].friends.friends.Add(friend);
                    }
                    else
                    {
                        Database.singleton.AddFriend(fromPlayer, name);
                    }
                }

                //remove request
                friendRequests.RemoveAt(index);
            }
        }

        [Command]public void CmdCancelFriendRequest(string friendName)
        {
            int index = GetFriendRequestIndex(friendName);
            if (index != -1)
            {
                friendRequests.RemoveAt(index);
            }
        }

        [Command]public void CmdFriendKick(string friendName)
        {
            // validate: is there someone with that name, and not self?
            int index = GetFriendIndex(friendName);
            if (index != -1)
            {
                friends.RemoveAt(index);
                Database.singleton.RemoveFriend(name, friendName);
                Database.singleton.RemoveFriend(friendName, name);

                //find friend in online players
                if (Player.onlinePlayers.TryGetValue(friendName, out Player other))
                {
                    index = other.friends.GetFriendIndex(name);
                    if (index != -1) other.friends.friends.RemoveAt(index);
                }
            }
        }

        [ClientRpc]public void RpcMessageFromTheServer(string message)
        {
            UIFriends.singleton.MessageFromTheServer(message);
        }

        //show friend online or not
        [Command(requiresAuthority = false)]
        private void CmdSetOnlineForFriends(bool online)
        {
            SetOnlineForFriends(online);
        }

        [Server]
        private void SetOnlineForFriends(bool online)
        {
            for (int i = 0; i < friends.Count; i++)
            {
                if (Player.onlinePlayers.TryGetValue(friends[i].name, out Player other))
                {
                    int index = other.friends.GetFriendIndex(name);
                    if (index != -1)
                    {
                        Friend friend = other.friends.friends[index];
                        friend.online = online;
                        other.friends.friends[index] = friend;
                    }
                }
            }
        }

        [Command(requiresAuthority = false)]
        private void CmdCheckFriendsOnlineStatus()
        {
            for (int i = 0; i < friends.Count; i++)
            {
                Friend friend = friends[i];
                friend.online = Player.onlinePlayers.ContainsKey(friends[i].name);
                friends[i] = friend;
            }
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            player = gameObject.GetComponent<Player>();
            player.friends = this;
        }
#endif
    }
}