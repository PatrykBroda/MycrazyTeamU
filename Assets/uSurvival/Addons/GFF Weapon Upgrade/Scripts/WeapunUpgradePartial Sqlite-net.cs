namespace uSurvival
{
    public partial class Database
    {
        public Item LoadModulesForSlot(Item item, string data)
        {     
            if (!string.IsNullOrEmpty(data))
            {
                int[] modules = new int[5];
                string temp = data;

                for (int i = 0; i < modules.Length; i++)
                {
                    if (ScriptableItem.dict.TryGetValue(temp.Substring(0, temp.IndexOf(";")).GetStableHashCode(), out ScriptableItem itemData))
                    {
                        modules[i] = itemData.name.GetStableHashCode();
                    }

                    if (i < modules.Length - 1)
                    {
                        temp = temp.Remove(0, temp.IndexOf(";") + 1);
                    }
                }

                item.modulesHash = modules;
            }

            return item;
        }

        public string SaveModulesFromSlot(Item item)
        {
            string modules = "";
            for (int x = 0; x < item.modulesHash.Length; x++)
            {
                if (item.modulesHash[x] != 0)
                    modules += ScriptableItem.dict[item.modulesHash[x]].name + ";";
                else modules += "0;";
            }

            return modules;
        }
    }
}

