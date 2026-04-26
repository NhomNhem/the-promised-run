using OpenUtility.Exceptions;
using TMPro;

namespace OpenUtility.Data
{
    [ScriptableVariableBinder(typeof(TMP_InputField), typeof(int), BindingGoal.ReceiveValue, DisplayName = "Default Integer Variable")]
    public class DefaultIntegerTextBinding : IntegerTextBinding
    {
        public override void SetValue(string newValue)
        {
            ThrowIf.NotInt(newValue, out int result);
            
            variable.SetValue(result);
        }
    }
}
