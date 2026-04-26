using OpenUtility.Data;
using UnityEngine.UI;

namespace OpenUtility
{
    [ScriptableVariableBinder(typeof(Slider), typeof(int), BindingGoal.ReceiveValue, DisplayName = "Default Integer Variable")]
    public class DefaultIntegerSliderBinding : IntegerSliderBinding
    {
        public override void SetValue(float newValue)
        {
            var casted = (int)newValue;
            
            variable.SetValue(casted);
        }
    }
}
