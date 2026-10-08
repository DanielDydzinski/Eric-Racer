using TMPro;
using UnityEngine;

namespace EricRacer.UI
{
    /// <summary>
    /// For fixed texts: type the message in the TMP text box with tokens like {A} or {Enter};
    /// at runtime they become coloured button labels (see <see cref="ButtonHints"/>).
    /// </summary>
    [RequireComponent(typeof(TMP_Text))]
    public class ButtonHintText : MonoBehaviour
    {
        void Awake()
        {
            var text = GetComponent<TMP_Text>();
            text.text = ButtonHints.Format(text.text);
        }
    }
}
