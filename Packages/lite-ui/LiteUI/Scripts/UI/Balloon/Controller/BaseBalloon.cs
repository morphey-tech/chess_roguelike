using LiteUI.Common.Extensions;
using LiteUI.UI.Service;
using UnityEngine;
using VContainer;

namespace LiteUI.UI.Balloon.Controller
{
    public abstract class BaseBalloon : MonoBehaviour
    {
        public Vector2 Offset { get; set; }

        public virtual bool Active
        {
            get => isActiveAndEnabled;
            set => gameObject.SetActive(value);
        }
    }
}
