using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LiteUI.Binding.Components
{
    public class UIDragAndDropComponent : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
    {
        public event Action<Drag>? OnBeginDragAction;
        public event Action<Drag>? OnDragAction;
        public event Action<Drag>? OnEndDragAction;
        public event Action<Drag>? OnDropAction;

        public void OnBeginDrag(PointerEventData eventData)
        {
            OnBeginDragAction?.Invoke(new Drag(eventData.pressPosition, eventData.position, eventData.delta, eventData.pointerDrag));
        }

        public void OnDrag(PointerEventData eventData)
        {
            OnDragAction?.Invoke(new Drag(eventData.pressPosition, eventData.position, eventData.delta, eventData.pointerDrag));
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            OnEndDragAction?.Invoke(new Drag(eventData.pressPosition, eventData.position, eventData.delta, eventData.pointerDrag));
        }

        public void OnDrop(PointerEventData eventData)
        {
            OnDropAction?.Invoke(new Drag(eventData.pressPosition, eventData.position, eventData.delta, eventData.pointerDrag));
        }
    }
}
