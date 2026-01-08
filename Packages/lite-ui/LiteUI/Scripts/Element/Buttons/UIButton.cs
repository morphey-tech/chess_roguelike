using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;
using UILabel = LiteUI.Element.Texts.UILabel;

namespace LiteUI.Element.Buttons
{
    [PublicAPI]
    public class UIButton : Button
    {
        [SerializeField]
        private GameObject? _label;

        public string? Label
        {
            get
            {
                if (_label == null) {
                    _label = gameObject;
                }
                if (_label.GetComponentInChildren<UILabel>()) {
                    return _label.GetComponentInChildren<UILabel>().Text;
                }
                return _label.GetComponentInChildren<Text>() ? _label.GetComponentInChildren<Text>().text : null;
            }
            set
            {
                if (_label == null) {
                    _label = gameObject;
                }
                if (_label.GetComponentInChildren<UILabel>()) {
                    _label.GetComponentInChildren<UILabel>().Text = value ?? "";
                }
                if (_label.GetComponentInChildren<Text>()) {
                    _label.GetComponentInChildren<Text>().text = value ?? "";
                }
            }
        }
    }
}
