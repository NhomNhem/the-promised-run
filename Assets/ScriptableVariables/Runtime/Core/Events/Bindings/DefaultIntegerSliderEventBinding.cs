using UnityEngine.UI;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(Slider), typeof(int), BindingGoal.DetermineValue, DisplayName = "Default Integer Variable")]
    public class DefaultIntegerSliderEventBinding : IntegerSliderEventBinding
    {
        protected override float ConvertIntegerToDecimal(int newValue) => newValue;
    }
}