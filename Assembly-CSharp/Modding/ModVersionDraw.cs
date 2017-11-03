using System;
using GlobalEnums;
using UnityEngine;

namespace Modding
{
	// Token: 0x020009B3 RID: 2483
	public class ModVersionDraw : MonoBehaviour
	{
		// Token: 0x060032A1 RID: 12961 RVA: 0x00137E24 File Offset: 0x00136024
		public void OnGUI()
		{
			if (this.drawString != null && UIManager.instance.uiState == UIState.MAIN_MENU_HOME)
			{
				Color backgroundColor = GUI.backgroundColor;
				Color contentColor = GUI.contentColor;
				Color color = GUI.color;
				Matrix4x4 matrix = GUI.matrix;
				GUI.backgroundColor = Color.white;
				GUI.contentColor = Color.white;
				GUI.color = Color.white;
				GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3((float)Screen.width / 1920f, (float)Screen.height / 1080f, 1f));
				GUI.Label(new Rect(0f, 0f, 1920f, 1080f), this.drawString);
				GUI.backgroundColor = backgroundColor;
				GUI.contentColor = contentColor;
				GUI.color = color;
				GUI.matrix = matrix;
			}
		}

		// Token: 0x04003B38 RID: 15160
		public string drawString;
	}
}
