using TMPro;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(TMP_Text), typeof(int), BindingGoal.DetermineValue, DisplayName = "Default Integer Variable")]
    public class DefaultIntegerTextEventBinding : IntegerTextEventBinding
    {
        protected override string ConvertIntegerToText(int newValue) => newValue.ToString();
    }
}
