using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;

namespace LiteUI.Element.Texts
{
    [PublicAPI]
    public class UILabel : TextMeshProUGUI
    {
        public override void SetLayoutDirty()
        {
            base.SetLayoutDirty();

            const string TEXT_CHANGED = "TextChanged";
            if (Application.IsPlaying(gameObject)) {
                gameObject.BroadcastMessage(TEXT_CHANGED, SendMessageOptions.DontRequireReceiver);
            }
        }

        // перекрываем использование проперти text класса TextMeshProUGUI для исключения опечаток и вызова базовой установки текста без обработки
        [Obsolete("Use 'Text' property")]
        // ReSharper disable once InconsistentNaming
        public new string text
        {
            get => Text;
            set => Text = value;
        }

        public virtual string Text
        {
            get
            {
                // ReSharper disable once UnusedVariable
                // RectTransform resolveRectTransform = rectTransform; // Фикс бага с неуспевшим инициализироваться текстом.

                // ReSharper disable once ArrangeAccessorOwnerBody
                return base.text;
            }
            set
            {
                if (!font.HasCharacters(value, out List<char> missing)) {
                    if (missing == null!) {
                        base.text = value;
                        return;
                    }
                    
                    foreach (char character in missing) {
                        value = value.Replace(character, '?');
                    }
                }

                base.text = value;
            }
        }
    }
}
