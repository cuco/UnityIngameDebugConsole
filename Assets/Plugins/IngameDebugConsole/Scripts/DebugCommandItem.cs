using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace IngameDebugConsole
{
    // command method item
    public class DebugCommandItem : MonoBehaviour, IPointerClickHandler
    {
        //Cache Components
        [SerializeField] RectTransform transformComponent;
        [SerializeField] Text text;
        [SerializeField] Image textBG;

        public RectTransform Transform { get { return transformComponent; } }
        public Image Image { get { return textBG; } }
        public ConsoleMethodInfo MethodInfo { get; private set; }
        public int Index { get; private set; }
        private DebugCommandItemListView manager;

        public void Initialize(DebugCommandItemListView manager)
        {
            this.manager = manager;
        }

        public void SetContent(ConsoleMethodInfo methodInfo, int index)
        {
            this.MethodInfo = methodInfo;
            this.Index = index;

            Vector2 size = transformComponent.sizeDelta;
            size.y = manager.ItemHeight;
            transformComponent.sizeDelta = size;

            text.text = methodInfo.signature;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            manager.OnItemClicked(this);
        }

		public override string ToString()
		{
            return MethodInfo.signature;
		}
	}
}
