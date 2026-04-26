#if UNITY_EDITOR

using System;
using Object = UnityEngine.Object;

namespace OpenUtility.Data.Editor
{
    internal struct BindingData
    {
        public const string DUPLICATE_TYPE_WARNING = "Detected duplicate display name on type {0}. An option with the name '{1}' already exists for value type '{2}'. Make sure to use an original 'DisplayName'.";
        
        public Type variableType;
        public Type bindingType;

        public BindingData(Type variableType, Type bindingType)
        {
            this.variableType = variableType;
            this.bindingType = bindingType;
        }
    }

    internal struct SelectionData
    {
        public Object variableAsset;
        public Type bindingType;
            
        public SelectionData(Object variableAsset, Type bindingType)
        {
            this.variableAsset = variableAsset;
            this.bindingType = bindingType;
        }
    }
}

#endif