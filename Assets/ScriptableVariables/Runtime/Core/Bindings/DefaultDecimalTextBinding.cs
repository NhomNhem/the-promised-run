using OpenUtility.Exceptions;
using TMPro;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(TMP_InputField), typeof(float), BindingGoal.ReceiveValue, DisplayName = "Default Decimal Variable")]
    public class DefaultDecimalTextBinding : DecimalTextBinding
    {
        public override void SetValue(string newValue)
        {
            ThrowIf.NotFloat(newValue, out float result);
            
            variable.SetValue(result);
        }
    }
}
