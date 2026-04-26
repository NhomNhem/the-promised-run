using System;

namespace OpenUtility.Data
{
    public class EnumTextEventBinding<TEnum> : IntegerTextEventBinding where TEnum : Enum
    {
        protected override string ConvertIntegerToText(int newValue)
        {
            return ((TEnum)Enum.ToObject(typeof(TEnum), newValue)).ToString();
        }
    }
}
