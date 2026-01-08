using Cysharp.Threading.Tasks;
using LiteUI.Popup.Model;
using UnityEngine;

namespace LiteUI.Popup.Panel
{
    public interface IPopup
    {
        UniTask Show(RectTransform target, PopupAlign defaultAlign, Vector2 offset);
        UniTask Hide();
    }
}
