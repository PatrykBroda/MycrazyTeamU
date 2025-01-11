// Saves the crafting recipe info in a ScriptableObject that can be used ingame
// by referencing it from a MonoBehaviour. It only stores static data.

// A Recipe can be created by right clicking the Resources folder and selecting
// Create -> GFF Addons/Crafting Extended/Recipe for Crafting. Existing recipes can be found in the Resources folder.

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using uSurvival;

namespace GFFAddons
{
    [Serializable]public struct SeveralItemsToChooseFrom
    {
       public ScriptableItemAndAmount[] items;                   
    }

    [CreateAssetMenu(fileName = "New Recipe", menuName = "GFF Addons/Crafting Extended/Recipe for Crafting", order = 999)]
    public partial class CraftingRecipeExtended : ScriptableObject
    {
        [Header("Ingredients")]
        //public ScriptableItemAndAmount[] ingredients = new ScriptableItemAndAmount[] { };
        [SerializeField]public SeveralItemsToChooseFrom[] _ingredients = new SeveralItemsToChooseFrom[] { };

        [Header("Сrafting time in seconds")]
        [SerializeField] private float _craftingTime = 10;
        [Header("Need to pay for craft?")]
        [SerializeField] private int _goldForCraft = 0;

        [Header("Result Item if succes normal")]
        public ScriptableItem[] resultItems = new ScriptableItem[] { };
        public ushort resultAmountMin = 1;
        public ushort resultAmountMax = 1;
        [Range(0, 1)] public float probabilitySuccess = 1;

        [Header("Result Item if succes improved")]
        public ScriptableItem[] resultItemsImproved = new ScriptableItem[] { };
        public ushort resultAmountMinImproved = 1;
        public ushort resultAmountMaxImproved = 1;
        [Range(0, 1)] public float probabilitySuccessImproved = 1;

        [Header("Result Items if Parse item into resources")]
        public bool disassembleToResources;

        [Header("Other settings")]
        public bool removeAllIngredientsIfFailed;
        public bool randomRemoveIngredientsIfFailed;
        [Range(0, 1)] public float chanceDestructionIngredients = 0;

        [Header("If use Crafting Skills")]
        public ScriptableCraftingSkillAndExp skillAndExp;

        [Header("If use Learned Recipes")]
        [SerializeField] private StudyableScroll _studyScroll;

        public float craftingTime => _craftingTime;
        public int goldForCraft => _goldForCraft;
        public StudyableScroll studyScroll => _studyScroll;

        // check if the list of items works for this recipe. the list shouldn't contain 'null'.
        // (inheriting classes can modify the matching algorithm if needed)
        public static CraftingRecipeExtended CanCraftWith(ScriptableItemAndAmount[] items, bool recycle)
        {
            // Ingridients list should not be touched, since it's often used to check more
            // than one recipe. so let's just create a local copy.
            ScriptableItemAndAmount[] ingredients = items;

            if (ingredients.Length > 0)
            {
                if (!recycle)
                {
                    //check all recipes in dict
                    foreach (CraftingRecipeExtended recipe in dict.Values)
                    {
                        //check found recipes by the number of ingredients
                        if (!recipe.disassembleToResources && recipe._ingredients.Length == ingredients.Length)
                        {
                            int itemsFind = 0;

                            //check all ingredients by name and amount
                            for (int i = 0; i < recipe._ingredients.Length; i++)
                            {
                                //looking for ingredients from the list
                                for (int x = 0; x < recipe._ingredients[i].items.Length; x++)
                                {
                                    int index = GetItemIndex(ingredients, recipe._ingredients[i].items[x].item);
                                    if (index != -1 && recipe._ingredients[index].items[x].amount == ingredients[i].amount)
                                    {
                                        itemsFind++;
                                        break;
                                    }
                                }
                            }

                            //Debug.Log("check recipe " + itemsFind);
                            if (itemsFind == recipe._ingredients.Length) return recipe;
                        }
                    }
                }
                else
                {
                    //check found recipes by the number of ingredients
                    foreach (var r in dict)
                    {
                        if (r.Value.disassembleToResources && ingredients.Length == r.Value.resultAmountMin && r.Value.resultItems[0] == ingredients[0].item)
                        {
                            return r.Value;
                        }
                    }
                }
            }
            return null;
        }

        private static int GetItemIndex(ScriptableItemAndAmount[] list, ScriptableItem item)
        {
            // (avoid FindIndex to minimize allocations)
            for (int i = 0; i < list.Length; ++i)
                if (list[i].item.name == item.name)
                    return i;
            return -1;
        }

        //for Remove Ingredients
        public ushort GetRequiredItemAmount(ScriptableItem item)
        {
            //for (int i = 0; i < ingredients.Length; i++)
            //{
            //    if (ingredients[i].Equals(item)) return ingredients[i].amount;
            //}

            for (int i = 0; i < _ingredients.Length; i++)
            {
                for (int x = 0; x < _ingredients[i].items.Length; x++)
                {
                    if (_ingredients[i].items[x].item.Equals(item)) return _ingredients[i].items[x].amount;
                }
            }

            return 0;
        }

        public static CraftingRecipeExtended GetRecipeByName(string name)
        {
            //check found recipes by the number of ingredients
            foreach (CraftingRecipeExtended r in dict.Values)
            {
                if (r.name == name)
                {
                    return r;
                }
            }
            return null;
        }

        // caching /////////////////////////////////////////////////////////////////
        // we can only use Resources.Load in the main thread. we can't use it when
        // declaring static variables. so we have to use it as soon as 'dict' is
        // accessed for the first time from the main thread.
        static Dictionary<string, CraftingRecipeExtended> cache;
        public static Dictionary<string, CraftingRecipeExtended> dict
        {
            get
            {
                // not loaded yet?
                if (cache == null)
                {
                    // old
                    // cache = Resources.LoadAll<GFFScriptableRecipe>("").ToDictionary(recipe => recipe.name, recipe => recipe);

                    // get all ScriptableRecipes in resources
                    CraftingRecipeExtended[] recipes = Resources.LoadAll<CraftingRecipeExtended>("");

                    // check for duplicates, then add to cache
                    List<string> duplicates = recipes.ToList().FindDuplicates(recipe => recipe.name);

                    if (duplicates.Count == 0)
                    {
                        cache = recipes.ToDictionary(recipe => recipe.name, recipe => recipe);
                    }
                    else
                    {
                        foreach (string duplicate in duplicates)
                            Debug.LogError("Resources folder contains multiple ScriptableRecipes with the name " + duplicate + ". If you are using subfolders like 'Warrior/Ring' and 'Archer/Ring', then rename them to 'Warrior/(Warrior)Ring' and 'Archer/(Archer)Ring' instead.");
                    }

                }
                return cache;
            }
        }
    }
}