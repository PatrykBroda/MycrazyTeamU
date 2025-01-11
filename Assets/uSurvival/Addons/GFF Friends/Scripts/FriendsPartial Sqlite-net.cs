using GFFAddons;

namespace uSurvival
{
    public partial class Database
    {
        public class character_friends
        {
            public string character { get; set; }
            public string friend { get; set; }
        }
        public class character_friendRequests
        {
            public string character { get; set; }
            public string inviteFrom { get; set; }
            public string className { get; set; }
        }

        public void Connect_Friends()
        {
            // create tables if they don't exist yet or were deleted
            connection.CreateTable<character_friends>();
            connection.CreateIndex(nameof(character_friends), new[] { "character", "friend" });

            connection.CreateTable<character_friendRequests>();
            connection.CreateIndex(nameof(character_friendRequests), new[] { "character", "inviteFrom" });
        }

        public void CharacterLoad_Friends(Player player)
        {
            //load friends
            foreach (character_friends row in connection.Query<character_friends>("SELECT * FROM character_friends WHERE character=?", player.name))
            {
                characters character = connection.FindWithQuery<characters>("SELECT * FROM characters WHERE name=? AND deleted=0", row.friend);
                if (row != null && character != null)
                {
                    player.friends.friends.Add(new Friend()
                    {
                        name = row.friend,
                        className = character.classname
                    });
                }
            }

            //load friends Requests
            foreach (character_friendRequests row in connection.Query<character_friendRequests>("SELECT * FROM character_friendRequests WHERE character=?", player.name))
            {
                player.friends.friendRequests.Add(new Friend() { name = row.inviteFrom, className = row.className });
            }
        }

        public void CharacterSave_Friends(Player player)
        {
            connection.Execute("DELETE FROM character_friends WHERE character=?", player.name);
            connection.Execute("DELETE FROM character_friendRequests WHERE character=?", player.name);

            //save friends
            for (int i = 0; i < player.friends.friends.Count; ++i)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_friends
                {
                    character = player.name,
                    friend = player.friends.friends[i].name
                });
            }

            //save friend requests
            for (int i = 0; i < player.friends.friendRequests.Count; ++i)
            {
                // note: .Insert causes a 'Constraint' exception. use Replace.
                connection.InsertOrReplace(new character_friendRequests
                {
                    character = player.name,
                    inviteFrom = player.friends.friendRequests[i].name,
                    className = player.friends.friendRequests[i].className
                });
            }
        }

        public bool AddFriendRequest(string forPlayer, Friend friend)
        {
            if (CharacterExists(forPlayer))
            {
                connection.InsertOrReplace(new character_friendRequests
                {
                    character = forPlayer,
                    inviteFrom = friend.name,
                    className = friend.className
                });

                return true;
            }

            return false;
        }

        public void AddFriend(string forPlayer, string friend)
        {
            if (connection.FindWithQuery<character_friends>("SELECT * FROM character_friends WHERE character=? AND friend=?", forPlayer, friend) == null)
            {
                connection.InsertOrReplace(new character_friends
                {
                    character = forPlayer,
                    friend = friend
                });
            }
            else print("Friend already added");
        }

        public void RemoveFriend(string character, string friend)
        {
            connection.Execute("DELETE FROM character_friends WHERE character=? and friend=? ", friend, character);
        }
    }
}


