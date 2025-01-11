using Mirror;
using System.Collections.Generic;
using UnityEngine;
using uSurvival;

namespace GFFAddons
{
    public enum CraftingErrors : byte { None, Success, Failed, YouDead, IncorrectCombination, InventoryIsFull, LowCraftingLevel }

    [DisallowMultipleComponent]
    public class PlayerCraftingExtended : NetworkBehaviour
    {
        [Header("Components")]
        public Player player;

        [Header("Settings : Crafting Skills")]
        public ScriptableCraftingSkill[] skillTemplates;
        [HideInInspector] public readonly SyncListCraftingSkill skills = new SyncListCraftingSkill();

        [Header("If disabled then all recipes will be shown")]
        public bool useLearnedRecipes;
        public bool showNotLearnedRecipes;

        [HideInInspector] public readonly SyncList<string> learnedCraftingRecipes = new SyncList<string>();
        [HideInInspector] public CraftingRecipeExtended recipeCurrent; // currently crafted recipe. cached to avoid searching ALL recipes in Craft()
        [HideInInspector] public readonly SyncList<int> craftingIndices = new SyncList<int>() { -1, -1, -1, -1, -1 };
        [HideInInspector] public CraftingState craftingState = CraftingState.None; // client sided

        [HideInInspector, SyncVar] public float craftingTime;
        [HideInInspector, SyncVar] public double endTime; // double for long term precision
        [HideInInspector] public bool requestPending; // for state machine event
        private bool boolRecycle;

        public float CraftingTimeRemaining()
        {
            // how much time remaining until the casttime ends? (using server time)
            return NetworkTime.time >= endTime ? 0 : (float)(endTime - NetworkTime.time);
        }

        void OnDragAndDrop_InventorySlot_CraftingExtendedIngredientSlot(int[] slotIndices)
        {
            // slotIndices[0] = slotFrom; slotIndices[1] = slotTo
            // only if not crafting right now
            if (craftingState != CraftingState.InProgress)
            {
                craftingState = CraftingState.None; // reset state
                CmdSwapInventoryCraftingExtended(slotIndices);
            }
        }

        [Command]public void CmdSwapInventoryCraftingExtended(int[] slotIndices)
        {
            if (!craftingIndices.Contains(slotIndices[0]))
            {
                craftingIndices[slotIndices[1]] = slotIndices[0];
            }
        }

        [Command]public void CmdClearСraftingIndex(int index)
        {
            craftingIndices[index] = -1;
        }

        [Command]public void CmdClearAllIndices()
        {
            for (int i = 0; i < craftingIndices.Count; ++i) craftingIndices[i] = -1;
        }

        private void FixedUpdate()
        {
            if (isServer)
            {
                if (requestPending && NetworkTime.time > endTime)
                {
                    requestPending = false;
                    Craft();
                }
            }
        }

        [Command]public void CmdSetRecipeFromBook(string name)
        {
            CheckRecipe(name);
        }

        [Server]private void CheckRecipe(string name)
        {
            recipeCurrent = CraftingRecipeExtended.GetRecipeByName(name);
            if (recipeCurrent != null)
            {
                //if crafting skills are used ?
                if (skillTemplates != null && skillTemplates.Length > 0 && recipeCurrent.skillAndExp.skill != null)
                {
                    for (int i = 0; i < skillTemplates.Length; i++)
                    {
                        if (skillTemplates[i].name == recipeCurrent.skillAndExp.skill.name)
                        {
                            //if (skills[i].level < recipeCurrent.skillAndExp.requiredLevel)
                                //TargetCraftingInfo("Requires " + recipeCurrent.skillAndExp.skill.name + " lv of crafting", true, false);
                        }
                    }
                }

                //preparing
                for (int i = 0; i < craftingIndices.Count; ++i) craftingIndices[i] = -1;

                //looking for items in the inventory from the recipe
                //if the recipe is not for parsing an item for resources
                if (!recipeCurrent.disassembleToResources)
                {
                    //check all the ingredients (items and amount)
                    for (int i = 0; i < recipeCurrent._ingredients.Length; ++i)
                    {
                        for (int x = 0; x < recipeCurrent._ingredients[i].items.Length; x++)
                        {
                            if (recipeCurrent._ingredients[i].items[x].item != null)
                            {
                                int index = player.inventory.GetItemIndexByName(recipeCurrent._ingredients[i].items[x].item.name);
                                if (index != -1) craftingIndices[i] = index;
                                else
                                {
                                    if (x == recipeCurrent._ingredients[i].items.Length)
                                    recipeCurrent = null;
                                }

                                break;
                            }
                        }

                        if (recipeCurrent == null) break;
                    }
                }
                else
                {
                    int index = player.inventory.GetItemIndexByName(recipeCurrent.resultItems[0].name);
                    if (index != -1) craftingIndices[0] = index;
                }
            }
        }

        public void StartCraft(ushort[] amounts, bool _boolRecycle, bool boolGold)
        {
            craftingState = CraftingState.InProgress; // wait for result

            CmdCraft(amounts, _boolRecycle, boolGold);
        }

        [Command]public void CmdCraft(ushort[] amounts, bool _boolRecycle, bool boolGold)
        {
            if (player.health.current > 0)
            {
                FindRecipe(amounts, _boolRecycle);

                if (recipeCurrent != null)
                {
                    //check Crafting Skills
                    if (skillTemplates.Length == 0 || CheckCraftingSkills(recipeCurrent))
                    {
                        //check free space in inventory
                        if (player.inventory.CanAdd(new Item(recipeCurrent.resultItems[0]), recipeCurrent.resultAmountMin))
                        {
                            boolRecycle = _boolRecycle;

                            // start crafting
                            requestPending = true;
                            float timeBonus = 1;
                            int skillIndex = FindSkillIndex(recipeCurrent.skillAndExp.skill.name);
                            if (skillIndex != -1) timeBonus = 1 - (skills[skillIndex].level * 0.02f);
                            craftingTime = recipeCurrent.craftingTime * timeBonus;
                            endTime = NetworkTime.time + craftingTime;
                        }
                        else TargetCraftingInfo(CraftingErrors.InventoryIsFull, true, false);
                    }
                    else TargetCraftingInfo(CraftingErrors.LowCraftingLevel, true, false);
                }
                else TargetCraftingInfo(CraftingErrors.IncorrectCombination, true, false);
            }
            else TargetCraftingInfo(CraftingErrors.YouDead, true, false);
        }

        // finish the crafting
        [Server]public void Craft()
        {
            //Debug.Log("craft end");
            // should only be called while CRAFTING and if recipe still valid
            // (no one should touch 'craftingRecipe', but let's just be sure.
            // -> we already validated everything in CmdCraft. let's just craft.
            if (recipeCurrent != null)
            {
                // enough space?
                if (player.inventory.CanAdd(new Item(recipeCurrent.resultItems[0]), recipeCurrent.resultAmountMin))
                {
                    // roll the dice to decide if we add the result or not
                    // IMPORTANT: we use rand() < probability to decide.
                    // => UnityEngine.Random.value is [0,1] inclusive:
                    //    for 0% probability it's fine because it's never '< 0'
                    //    for 100% probability it's not because it's not always '< 1', it might be == 1
                    //    and if we use '<=' instead then it won't work for 0%
                    // => C#'s Random value is [0,1) exclusive like most random functions. this works fine.

                    if (new System.Random().NextDouble() < recipeCurrent.probabilitySuccess)
                    {
                        //remove the ingredients from inventory in any case
                        RemoveIngredients();

                        //adding items
                        ItemSlot slotResult = new ItemSlot();

                        if (!recipeCurrent.disassembleToResources)
                        {
                            //check for improved chance of crafting
                            if (recipeCurrent.resultItemsImproved != null && recipeCurrent.resultItemsImproved.Length > 0 && new System.Random().NextDouble() < recipeCurrent.probabilitySuccessImproved)
                            {
                                slotResult.item = new Item(recipeCurrent.resultItemsImproved[Random.Range(0, recipeCurrent.resultItemsImproved.Length)]);

                                //item amount
                                if (recipeCurrent.resultAmountMinImproved == recipeCurrent.resultAmountMaxImproved) slotResult.amount = recipeCurrent.resultAmountMinImproved;
                                else slotResult.amount = (ushort)Random.Range(recipeCurrent.resultAmountMinImproved, recipeCurrent.resultAmountMaxImproved);
                            }
                            else
                            {
                                slotResult.item = new Item(recipeCurrent.resultItems[Random.Range(0, recipeCurrent.resultItems.Length)]);

                                //item amountint 
                                if (recipeCurrent.resultAmountMax == recipeCurrent.resultAmountMin) slotResult.amount = recipeCurrent.resultAmountMin;
                                else slotResult.amount = (ushort)Random.Range(recipeCurrent.resultAmountMin, recipeCurrent.resultAmountMax);
                            }

                            //set the amount of slots for enchantment
                            /*if (useUpgradeAddon && slotResult.item.data is EquipmentItem item && item.GetCompatibleRunes.Length > 0)
                            {
                                if (recipeCurrent.holesRandom) slotResult.item.holes = Random.Range(recipeCurrent.minHoles, recipeCurrent.maxHoles);
                                else slotResult.item.holes = recipeCurrent.minHoles;
                            }
                            else slotResult.item.holes = 0;*/

                            player.inventory.Add(slotResult.item, slotResult.amount);
                        }
                        else
                        {
                            for (int i = 0; i < recipeCurrent._ingredients.Length; i++)
                            {
                                for (int x = 0; x < recipeCurrent._ingredients[i].items.Length; x++)
                                {
                                    if (recipeCurrent._ingredients[i].items[x].item != null)
                                    {
                                        player.inventory.Add(new Item(recipeCurrent._ingredients[i].items[x].item), recipeCurrent._ingredients[i].items[x].amount);
                                        break;
                                    }
                                }
                            }
                        }

                        //crafting skills
                        if (skillTemplates.Length > 0) AddSkillExp(recipeCurrent);

                        //info
                        TargetCraftingInfo(CraftingErrors.Success, false, true);
                    }
                    else
                    {
                        //remove the ingredients from inventory
                        if (recipeCurrent.removeAllIngredientsIfFailed) RemoveIngredients();

                        //remove the random ingredients
                        if (!recipeCurrent.removeAllIngredientsIfFailed && recipeCurrent.randomRemoveIngredientsIfFailed &&
                            recipeCurrent.chanceDestructionIngredients > 0) RemoveIngredientsRandom();

                        //info
                        TargetCraftingInfo(CraftingErrors.Failed, false, true);
                    }
                }
            }
        }

        private void FindRecipe(ushort[] amounts, bool boolRecycle)
        {
            recipeCurrent = null;
            List<ScriptableItemAndAmount> ingredients = new List<ScriptableItemAndAmount>() { };

            //find all the slots that are not empty
            for (int i = 0; i < craftingIndices.Count; i++)
            {
                if (craftingIndices[i] != -1 &&
                    craftingIndices[i] < player.inventory.slots.Count &&
                    player.inventory.slots[craftingIndices[i]].amount > 0 &&
                    player.inventory.slots[craftingIndices[i]].amount >= amounts[i])
                {
                    ingredients.Add(new ScriptableItemAndAmount
                    {
                        item = player.inventory.slots[craftingIndices[i]].item.data,
                        amount = amounts[i]
                    });
                }
            }

            CraftingRecipeExtended recipe = CraftingRecipeExtended.CanCraftWith(ingredients.ToArray(), boolRecycle);

            if (!player.craftingExtended.useLearnedRecipes || CheckRecipes(recipe))
            {
                recipeCurrent = recipe;
            }
        }

        //used when displaying all available recipes in dropdown && and when we are looking for a recipe
        public bool CheckRecipes(CraftingRecipeExtended recipe)
        {
            if (recipe.studyScroll == null) return true;
            else
            {
                for (int i = 0; i < learnedCraftingRecipes.Count; i++)
                {
                    if (learnedCraftingRecipes[i] == recipe.studyScroll.name) return true;
                }
                return false;
            }
        }

        private bool CheckCraftingSkills(CraftingRecipeExtended recipe)
        {
            if (recipe.skillAndExp.skill == null) return true;

            int index = FindSkillIndex(recipe.skillAndExp.skill.name);
            if (index != -1)
                return recipe.skillAndExp.requiredLevel <= skills[index].level;
            else return false;
        }
        private void AddSkillExp(CraftingRecipeExtended recipe)
        {
            if (recipe.skillAndExp.skill != null)
            {
                int index = FindSkillIndex(recipe.skillAndExp.skill.name);

                if (index != -1 && skills[index].level < skills[index].maxLevel)
                {
                    CraftingSkill skill = skills[index];
                    uint requiredExp = skill._experienceMax.Get(skill.level + 1);

                    if (skill.exp + recipe.skillAndExp.addsExp < requiredExp) skill.exp += skill.exp + recipe.skillAndExp.addsExp;
                    else if (skill.exp + recipe.skillAndExp.addsExp == requiredExp)
                    {
                        skill.level++;
                        skill.exp = 0;
                    }
                    else
                    {
                        uint neededToLevelUp = requiredExp - skill.exp;
                        skill.level++;
                        skill.exp = skill.exp + recipe.skillAndExp.addsExp - neededToLevelUp;
                    }

                    skills[index] = skill;
                }
            }
        }
        public int FindSkillIndex(string name)
        {
            for (int i = 0; i < skills.Count; i++)
            {
                if (skills[i].name == name) return i;
            }

            return -1;
        }

        private void RemoveIngredients()
        {
            for (int i = 0; i < craftingIndices.Count; i++)
            {
                if (craftingIndices[i] != -1)
                {
                    // decrease item amount
                    ItemSlot slot = player.inventory.slots[craftingIndices[i]];

                    if (!boolRecycle) slot.DecreaseAmount(recipeCurrent.GetRequiredItemAmount(slot.item.data));
                    else slot.DecreaseAmount(recipeCurrent.resultAmountMin);

                    player.inventory.slots[craftingIndices[i]] = slot;

                    //clear reforge ingredient if amount < 1
                    if (slot.amount < 1)
                    {
                        if (!boolRecycle) craftingIndices[i] = player.inventory.GetItemIndexByName(slot.item.name);
                        else craftingIndices[i] = player.inventory.GetItemIndexByName(recipeCurrent.resultItems[0].name);
                    }
                }
            }
        }
        private void RemoveIngredientsRandom()
        {
            for (int i = 0; i < craftingIndices.Count; i++)
            {
                if (craftingIndices[i] != -1)
                {
                    if (Random.Range(0f, 1f) < recipeCurrent.chanceDestructionIngredients)
                    {
                        // decrease item amount
                        ItemSlot slot = player.inventory.slots[craftingIndices[i]];
                        slot.amount -= recipeCurrent.GetRequiredItemAmount(slot.item.data);
                        player.inventory.slots[craftingIndices[i]] = slot;

                        //clear reforge ingredient if amount < 1
                        if (player.inventory.slots[craftingIndices[i]].amount < 1) craftingIndices[i] = -1;
                    }
                }
            }
        }


        [TargetRpc] // only send to one client
        public void TargetCraftingInfo(CraftingErrors message, bool failed, bool continueCrafting)
        {
            craftingState = CraftingState.None;
            UICraftingExtended.singleton.ShowInfo(player, failed, continueCrafting, message);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            player = gameObject.GetComponent<Player>();
            player.craftingExtended = this;
        }
#endif
    }
}