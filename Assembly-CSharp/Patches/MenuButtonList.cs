using System.Collections.Generic;
using MonoMod;
using UnityEngine.UI;

// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming
#pragma warning disable 649
#pragma warning disable 1591

namespace Modding.Patches
{
    [MonoModPatch("global::MenuButtonList")]
    public class MenuButtonList : global::MenuButtonList
    {
        [MonoModIgnore]
        private static List<MenuButtonList> menuButtonLists;

        [MonoModIgnore]
        private Entry[] entries;

        public void AddSelectable(Selectable sel)
        {
            if (entries != null)
            {
                AddSelectable(sel, entries.Length);
            }
        }

        public void AddSelectableEnd(Selectable sel, int controlButtons)
        {
            AddSelectable(sel, entries.Length - controlButtons);
        }

        public void AddSelectable(Selectable sel, int index)
        {
            if (sel == null || entries == null || index < 0 || index > entries.Length)
            {
                return;
            }

            Entry[] newEntries = new Entry[entries.Length + 1];

            for (int i = 0; i < index; i++)
            {
                newEntries[i] = entries[i];
            }

            Entry newEntry = new Entry();
            ReflectionHelper.SetAttr(newEntry, "selectable", sel);

            newEntries[index] = newEntry;

            for (int i = index + 1; i < newEntries.Length; i++)
            {
                newEntries[i] = entries[i - 1];
            }

            entries = newEntries;
        }

        public void ClearSelectables()
        {
            entries = new Entry[0];
        }

        public void RecalculateNavigation()
        {
            menuButtonLists.Remove(this);
            Start();
        }

        [MonoModIgnore]
        private class Entry { }
    }
}