#if UNITY_EDITOR
#if UNITY_2019_4_OR_NEWER

using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace FIMSpace.Generating
{

    public class SearchableDropdown<T> : AdvancedDropdown where T : class
    {
        public static string ChoosingHelperID = "";
        //public static object ChoosedO = null;
        public static DropElement Selected = null;
        //public static T Choosed = null;
        string title = "Dropdown";

        #region Implementations

        public SearchableDropdown(List<T> elements, List<string> names, string title) : base(new AdvancedDropdownState())
        {
            this.title = title;
            minimumSize = new Vector2(160, 220);
            if (allElements != null) allElements.Clear();
            SetElements(elements, names, true);
        }

        protected override void ItemSelected(AdvancedDropdownItem item)
        {
            base.ItemSelected(item);
            if (item is DropElement) Selected = item as DropElement;

            if (Selected != null)
            {
                //Choosed = Selected.toAdd;
                //ChoosedO = Selected.toAdd;
                Searchable.Choose(Selected.toAdd);
            }
        }

        public AdvancedDropdownItem FindParent(string name, List<AdvancedDropdownItem> categories)
        {
            for (int i = 0; i < categories.Count; i++)
            {
                if (categories[i].name == name) return categories[i];
            }

            return null;
        }

        #endregion

        public class DropElement : AdvancedDropdownItem
        {
            public T toAdd;
            #region Implentations

            public AdvancedDropdownItem adItem;
            public DropElement parent;

            #endregion

            public DropElement(string name, T toAdd) : base(name)
            {
                this.name = name;
                this.toAdd = toAdd;
                #region Implentations
                adItem = new AdvancedDropdownItem(name);
                parent = null;
                #endregion
            }
        }


        public static List<DropElement> allElements = new List<DropElement>();

        public void SetElements(List<T> elements, List<string> names, bool overrideList = false)
        {
            if (elements.Count != names.Count)
            {
                UnityEngine.Debug.Log("[Searchable Dropdown] Wrong list counts!");
                return;
            }

            if (allElements.Count <= 1 || overrideList)
                for (int i = 0; i < elements.Count; i++)
                {
                    string name = names[i];
                    allElements.Add(new DropElement(name, elements[i]));
                }
        }


        protected override AdvancedDropdownItem BuildRoot()
        {
            var root = new AdvancedDropdownItem(title);
            List<AdvancedDropdownItem> alreadyAddedParents = new List<AdvancedDropdownItem>();

            #region Managing parenting for elements

            for (int i = 0; i < allElements.Count; i++)
            {
                AdvancedDropdownItem parentDropItem = null;

                string[] parents = allElements[i].name.Split('/');

                for (int p = 0; p < parents.Length - 1; p++) // Checking if sections like "Transforming/Noise" are added
                { // -1 so we don't check for rule name as category
                    parentDropItem = FindParent(parents[p], alreadyAddedParents);

                    if (parentDropItem == null) // Parent category not added yet to the list
                    {
                        if (p == 0) // It's first depth parent name - so category in root
                        {
                            parentDropItem = new AdvancedDropdownItem(parents[p]);
                            root.AddChild(parentDropItem);
                            alreadyAddedParents.Add(parentDropItem);
                        }
                        else // It's another depth parent name - so child of other categories
                        {
                            var backParentItem = FindParent(parents[p - 1], alreadyAddedParents);

                            if (backParentItem != null)
                            {
                                parentDropItem = new AdvancedDropdownItem(parents[p]);
                                backParentItem.AddChild(parentDropItem);
                                alreadyAddedParents.Add(parentDropItem);
                            }
                        }
                    }
                }

                if (parents.Length > 1)
                    parentDropItem = FindParent(parents[parents.Length - 2], alreadyAddedParents);

                if (parentDropItem != null) // Found or new added
                {
                    int lastSlash = allElements[i].name.LastIndexOf('/');
                    if (lastSlash > 1 && lastSlash < allElements[i].name.Length - 1)
                    {
                        string nodeName = allElements[i].name.Substring(lastSlash + 1, allElements[i].name.Length - (lastSlash + 1));
                        var ruleItem = new DropElement(nodeName, allElements[i].toAdd);
                        parentDropItem.AddChild(ruleItem);
                    }
                }
                else
                {
                    if (parents.Length <= 1) // Elements without categories
                    {
                        var ruleItem = new DropElement(allElements[i].name, allElements[i].toAdd);
                        root.AddChild(ruleItem);
                    }
                }
            }

            #endregion

            return root;
        }

    }
}

#endif
#endif

namespace FIMSpace.Generating
{
    public static class Searchable
    {

#if UNITY_2019_4_OR_NEWER
#else
        public static string ChoosingHelperID = "";
#endif

        public static bool CheckSubType = true;

        public static void Choose(object value)
        {
            IsSetted = true;
            choosed = value;
        }

        public static T Get<T>(bool extensiveTypeMatch = false) where T : UnityEngine.Object
        {
            if (choosed == null)
            {
                IsSetted = false;
                return null;
            }
            else
            {
                bool typeMatch;

                if (extensiveTypeMatch)
                {
                    if (CheckSubType)
                    {
                        typeMatch = choosed.GetType().IsSubclassOf(typeof(T));
                    }
                    else
                    {
                        typeMatch = choosed.GetType() == typeof(T);
                    }
                }
                else
                {
                    typeMatch = typeof(T) == choosed.GetType();
                }


                if (typeMatch)
                {
                    IsSetted = false;
                    T ch = (T)choosed;
                    choosed = null;
                    return ch;
                }


                return null;
            }
        }

        public static object Get()
        {
            if (IsSetted)
            {
                IsSetted = false;
                object c = choosed;
                choosed = null;
                return c;
            }

            return null;
        }

        public static bool IsSetted { get; private set; }
        private static object choosed = null;
    }
}
