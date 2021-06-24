using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Modding.Menu.Components
{
    /// <summary>
    /// A component to automatically select a menu item.
    /// </summary>
    public class AutoSelector : MonoBehaviour
    {
        /// <summary>
        /// The menu item to select.
        /// </summary>
        public Selectable Start { get; set; }

        private void OnEnable()
        {
            if (this.Start != null)
            {
                this.StartCoroutine(this.SelectDelayed(this.Start));
            }
        }

        private IEnumerator SelectDelayed(Selectable selectable)
        {
            while (!selectable.gameObject.activeInHierarchy)
            {
                yield return null;
            }
            if (selectable is MenuSelectable ms)
            {
                ms.DontPlaySelectSound = true;
                selectable.Select();
                ms.DontPlaySelectSound = false;
            }
            else
            {
                selectable.Select();
            }
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