using System;

namespace DebuggingEssentials
{
    [Serializable]
    public class BaseInputField
    {
        public string text;

        public virtual void TryParse(bool logError = true) { }
    }

    [Serializable]
    public class IntInputField : BaseInputField
    {
        public int value;

        public IntInputField(int value)
        {
            this.value = value;
            text = value.ToString();
        }

        public override void TryParse(bool logError = true)
        {
            if (text == string.Empty) return;

            object newValue;
            Parser.ChangeType(typeof(int), text, out newValue, logError);
            value = (int)newValue;
        }
    }

    [Serializable]
    public class FloatInputField : BaseInputField
    {
        public float value;

        public FloatInputField(float value)
        {
            this.value = value;
            text = value.ToString();
        }

        public override void TryParse(bool logError = true)
        {
            object newValue;
            Parser.ChangeType(typeof(float), text, out newValue, logError);
            if (newValue != null) value = (float)newValue;
        }

        public void SetValueText(float value)
        {
            this.value = value;
            text = value.ToString();
        }

        public void UpdateText()
        {
            text = value.ToString();
        }
    }
}