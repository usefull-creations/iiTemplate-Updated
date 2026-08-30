using System;

namespace StupidTemplate.Classes
{
    public enum ButtonMode
    {
        Toggle,
        Action,
        Incremental,
    }

    public class ButtonInfo
    {
        public string buttonText = "-";

        public ExtGradient  disabledColor = null;
        public Action       disableMethod = null;
        public Func<string> displayText   = null;
        public bool         enabled       = false;
        public ExtGradient  enabledColor  = null;
        public Action       enableMethod  = null;

        public Action<bool> incrementMethod = null;
        public Action       method          = null;
        public ButtonMode   mode            = ButtonMode.Toggle;
        public string       overlapText     = null;
        public string       toolTip         = "This button doesn't have a tooltip/tutorial.";

        public bool? useGradient = null;
        public bool? useRounded  = null;

        public string GetDisplayText() =>
                displayText?.Invoke() ?? overlapText ?? buttonText;
    }
}