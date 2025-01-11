using UnityEngine;
using UnityEngine.EventSystems;

namespace GFFAddons
{
    [RequireComponent(typeof(EventTrigger))]
    public class UIChangeSize : MonoBehaviour
    {
        public RectTransform target;

        public Vector2 minSize = new Vector2(200, 300);
        public Vector2 maxSize = new Vector2(600, 600);

        private EventTrigger _eventTrigger;

        void Start()
        {
            _eventTrigger = GetComponent<EventTrigger>();
            _eventTrigger.AddEventTrigger(OnDrag, EventTriggerType.Drag);
        }

        void OnDrag(BaseEventData data)
        {
            PointerEventData ped = (PointerEventData)data;
            RectTransform.Edge? horizontalEdge = null;
            RectTransform.Edge? verticalEdge = null;

            horizontalEdge = RectTransform.Edge.Left;
            verticalEdge = RectTransform.Edge.Bottom;

            if (horizontalEdge != null)
            {
                if (horizontalEdge == RectTransform.Edge.Right)
                {
                    target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)horizontalEdge,
                        Screen.width - target.position.x - target.pivot.x * target.rect.width,
                        Mathf.Clamp(target.rect.width - ped.delta.x, minSize.x, maxSize.x));

                }
                else
                {
                    target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)horizontalEdge,
                        target.position.x - target.pivot.x * target.rect.width,
                        Mathf.Clamp(target.rect.width + ped.delta.x, minSize.x, maxSize.x));

                }
            }
            if (verticalEdge != null)
            {
                if (verticalEdge == RectTransform.Edge.Top)
                {
                    target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)verticalEdge,
                        Screen.height - target.position.y - target.pivot.y * target.rect.height,
                        Mathf.Clamp(target.rect.height - ped.delta.y, minSize.y, maxSize.y));

                }
                else
                {
                    target.SetInsetAndSizeFromParentEdge((RectTransform.Edge)verticalEdge,
                        target.position.y - target.pivot.y * target.rect.height,
                        Mathf.Clamp(target.rect.height + ped.delta.y, minSize.y, maxSize.y));

                }
            }
        }
    }
}


