using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LiteUI.Element.Texts
{
    [PublicAPI]
    public class UITrimmedLabel : UILabel
    {
        public int MaxVisibleLines { private get; set; } = -1;
        public int MaxVisibleCharacters { private get; set; } = -1;
        public bool TruncateWithEllipsis { private get; set; }

        public override string Text
        {
            set
            {
                base.Text = value;

                if (MaxVisibleLines > 0 || MaxVisibleCharacters > 0) {
                    TrimLines(base.Text);
                }
            }
        }

        private void TrimLines(string str)
        {
            LayoutElement layout = gameObject.GetComponent<LayoutElement>() ?? gameObject.AddComponent<LayoutElement>();

            overflowMode = TextOverflowModes.Overflow;
            TMP_TextInfo textData = GetTextInfo(str);

            float basePreferredHeight = textData.lineInfo[0].ascender - textData.lineInfo[textData.lineCount - 1].descender;

            if ((textData.lineCount < MaxVisibleLines || MaxVisibleLines <= 0)
                && (textData.characterCount < MaxVisibleCharacters || MaxVisibleCharacters <= 0)) {
                layout.preferredHeight = basePreferredHeight;
                return;
            }

            overflowMode = TruncateWithEllipsis ? TextOverflowModes.Ellipsis : TextOverflowModes.Truncate;
            float linesHeight = MaxVisibleLines <= 0 || textData.lineCount < MaxVisibleLines
                                        ? basePreferredHeight
                                        : textData.lineInfo[0].ascender - textData.lineInfo[MaxVisibleLines - 1].descender;
            float charactersHeight = MaxVisibleCharacters <= 0 || textData.characterCount < MaxVisibleCharacters
                                             ? basePreferredHeight
                                             : textData.lineInfo[0].ascender - textData.characterInfo[MaxVisibleCharacters - 1].descender;

            layout.preferredHeight = Mathf.Min(linesHeight, charactersHeight);
        }
    }
}
