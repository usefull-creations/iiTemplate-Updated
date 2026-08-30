using UnityEngine;

namespace StupidTemplate.Classes
{
    public sealed class MenuTheme
    {
        public string name;

        public Color backgroundPrimary;
        public Color backgroundSecondary;

        public Color disabledButtonPrimary;
        public Color disabledButtonSecondary;

        public Color enabledButtonPrimary;
        public Color enabledButtonSecondary;

        public Color disabledText = Color.white;
        public Color enabledText  = Color.white;

        public Color outline = Color.black;
    }
}