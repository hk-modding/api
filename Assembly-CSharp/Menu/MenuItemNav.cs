using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Modding.Menu
{
    /// <summary>
    /// A component to automatically calculate menu item navigation.
    /// </summary>
    public class MenuItemNav : MonoBehaviour
    {
        /// <summary>
        /// The currently selected menu item.
        /// </summary>
        public MenuSelectable Selected { get; private set; }
        /// <summary>
        /// The list of menu items in the content pane.
        /// </summary>
        public List<MenuSelectable> Content { get; set; } = new List<MenuSelectable>();
        /// <summary>
        /// The list of menu items in the control pane.
        /// </summary>
        public List<MenuSelectable> Controls { get; set; } = new List<MenuSelectable>();

        /// <summary>
        /// Calculates the <c>Navigation</c>s of all of the menu items.
        /// </summary>
        public void RecalculateNavigation()
        {
            for (var i = 0; i < this.Content.Count + this.Controls.Count; i++)
            {
                var last = this.GetSelJoint(i - 1);
                var sel = this.GetSelJoint(i);
                var next = this.GetSelJoint(i + 1);

                var nav = sel.navigation;
                nav.mode = Navigation.Mode.Explicit;
                nav.selectOnUp = last;
                nav.selectOnDown = next;
                sel.navigation = nav;
                sel.OnSelected += self => this.Selected = self;
            }
        }

        /// <summary>
        /// Gets the first menu item, or null if none have been added.
        /// </summary>
        /// <returns></returns>
        public MenuSelectable FirstItem()
        {
            if (this.Content.Count > 0) return this.Content[0];
            else if (this.Controls.Count > 0) return this.Controls[0];
            else return null;
        }

        // should only be called on a valid offset +/- 1
        private MenuSelectable GetSelJoint(int index)
        {
            if (index == -1)
            {
                return this.Controls.Count == 0 ?
                    this.Content[this.Content.Count - 1] :
                    this.Controls[this.Controls.Count - 1];
            }
            if (index < this.Content.Count)
            {
                return this.Content[index];
            }
            index -= this.Content.Count;
            if (index < this.Controls.Count)
            {
                return this.Controls[index];
            }
            return this.Content.Count == 0 ? this.Controls[0] : this.Content[0];
        }

        private void OnEnable()
        {
            this.Select();
        }

        private void Select()
        {
            if (this.Selected != null)
            {
                this.StartCoroutine(this.SelectDelayed(this.Selected));
            }
            else
            {
                var top = this.FirstItem();
                if (top != null) this.StartCoroutine(this.SelectDelayed(top));
            }
        }

        private IEnumerator SelectDelayed(Selectable selectable)
        {
            while (!selectable.gameObject.activeInHierarchy)
            {
                yield return null;
            }
            selectable.Select();
            if (selectable is MenuSelectable ms) ms.DontPlaySelectSound = false;
            foreach (Animator animator in selectable.GetComponentsInChildren<Animator>())
            {
                if (animator.HasParameter("hide", null))
                {
                    animator.ResetTrigger("hide");
                }
                if (animator.HasParameter("show", null))
                {
                    animator.SetTrigger("show");
                }
            }
            yield break;
        }
    }
}