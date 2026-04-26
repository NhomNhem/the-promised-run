#if UNITY_EDITOR

using System;
using System.Globalization;
using System.IO;
using System.Linq;
using OpenUtility.Exceptions;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.ProjectWindowCallback;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OpenUtility.Data.Editor
{
    public static class ScriptableVariableFactory
    {
        public delegate void AssetCreatedCallback(Object asset, Object target, string propertyPath);
        
        private class AssetCreationCallback : EndNameEditAction
        {
            private Object _target;
            private string _propertyPath;
            private Type _variableType;
            private AssetCreatedCallback _callback;

            public void Setup(Object target, string propertyPath, Type scriptableObjectType, AssetCreatedCallback callback)
            {
                _target = target;
                _propertyPath = propertyPath;
                _variableType = scriptableObjectType;
                _callback = callback;
            }

            public override void Action(int instanceId, string pathName, string resourceFile)
            {
                ScriptableObject asset = CreateInstance(_variableType);
                
                AssetDatabase.CreateAsset(asset, pathName);
                AssetDatabase.SaveAssets();

                ProjectWindowUtil.ShowCreatedAsset(asset);
                
                _callback?.Invoke(asset, _target, _propertyPath);
            }
        }
        
        public static Type GetTypeOfComponentBinding<TUIElement>(Type typeOfAttributeImplementer, BindingGoal goal)
        {
            if (typeof(MonoBehaviour).IsAssignableFrom(typeOfAttributeImplementer))
                return (typeOfAttributeImplementer);
            
            if (typeof(TMP_Text).IsAssignableFrom(typeof(TUIElement)))
            {
                switch (goal)
                {
                    case BindingGoal.ReceiveValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerTextBinding));

                        if (typeof(ScriptableFloat).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultDecimalTextBinding));
                        break;
                    
                    case BindingGoal.DetermineValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerTextEventBinding));

                        if (typeof(ScriptableFloat).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultDecimalTextEventBinding));
                        break;
                }
                
                Debug.LogWarning($"Found class {typeOfAttributeImplementer.Name} that implements ScriptableVariableBinder but is neither a ScriptableInt, ScriptableFloat or a MonoBehaviour. This is not allowed.");
                return (null);
            }

            if (typeof(Dropdown).IsAssignableFrom(typeof(TUIElement)))
            {
                Debug.LogWarning($"Found class {typeOfAttributeImplementer.Name} that implements ScriptableVariableBinder but is not a MonoBehaviour. This is not allowed.");
                return (null);
            }

            if (typeof(Slider).IsAssignableFrom(typeof(TUIElement)))
            {
                switch (goal)
                {
                    case BindingGoal.ReceiveValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerSliderBinding));
                        break;
                    
                    case BindingGoal.DetermineValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerSliderEventBinding));
                        break;
                }
                
                Debug.LogWarning($"Found class {typeOfAttributeImplementer.Name} that implements ScriptableVariableBinder but is neither a ScriptableInt or a MonoBehaviour. This is not allowed.");
                return (null);
            }

            if (typeof(TMP_InputField).IsAssignableFrom(typeof(TUIElement)))
            {
                switch (goal)
                {
                    case BindingGoal.ReceiveValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerTextBinding));

                        if (typeof(ScriptableFloat).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultDecimalTextBinding));
                        break;
                    
                    case BindingGoal.DetermineValue:
                        if (typeof(ScriptableInt).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultIntegerTextEventBinding));

                        if (typeof(ScriptableFloat).IsAssignableFrom(typeOfAttributeImplementer))
                            return (typeof(DefaultDecimalTextEventBinding));
                        break;
                }
            
                Debug.LogWarning($"Found class {typeOfAttributeImplementer.Name} that implements ScriptableVariableBinder but is neither a ScriptableInt, ScriptableFloat or a MonoBehaviour. This is not allowed.");
                return (null);
            }

            if (typeof(Toggle).IsAssignableFrom(typeof(TUIElement)))
            {
                Debug.LogWarning($"Found class {typeOfAttributeImplementer.Name} that implements ScriptableVariableBinder but is not a MonoBehaviour. This is not allowed.");
                return (null);
            }

            return null;
        }
        
        public static void AssignIntVariableForSlider(Slider slider, Object variableAsset, Type bindingType)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableIntBinder = (IntegerSliderBinding)slider.gameObject.AddComponent(bindingType);
            var serializedBinder = new SerializedObject(scriptableIntBinder);
            var variableProperty = serializedBinder.FindProperty("_variable");

            variableProperty.objectReferenceValue = scriptableInt;

            serializedBinder.ApplyModifiedProperties();
            serializedBinder.Dispose();
                
            UnityEventTools.AddPersistentListener(slider.onValueChanged, scriptableIntBinder.SetValue);
        }
        
        public static void CreateAndAssignIntVariableForSlider(Slider slider, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            ThrowIf.NotDerivedFrom<IntegerSliderBinding>(bindingType);
            
            var serializedObject = new SerializedObject(slider);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignIntVariableForSlider((Slider)target, asset, bindingType);
            }
        }
        
        public static void AssignTextFieldToEnumVariableEvent(TMP_Text textField, Object variableAsset, Type bindingType)
        {
            var scriptableEnum = (ScriptableEnum)variableAsset;
            var scriptableEvent = (IntegerTextEventBinding)textField.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            var enumValueType = ScriptableEnumEditor.GetEnumValueType(scriptableEnum.GetType());
            var enumValueNames = Enum.GetNames(enumValueType);
            var enumIntValue = scriptableEnum.GetValue();
            
            var enumStringValue = enumValueNames[enumIntValue];
            textField.text = enumStringValue;
            
            variableProperty.objectReferenceValue = scriptableEnum;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, textField.SetText);
        }
        
        public static void CreateEnumVariableAndAssignTextFieldToEvent(TMP_Text textField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableEnum>(variableType);
            ThrowIf.NotDerivedFrom<IntegerTextEventBinding>(bindingType);
            
            CreateNewAsset(textField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignTextFieldToEnumVariableEvent((TMP_Text)target, asset, bindingType);
            }
        }
        
        public static void AssignTextFieldToIntVariableEvent(TMP_Text textField, Object variableAsset, Type bindingType)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableEvent = (IntegerTextEventBinding)textField.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");
            
            var intValue = scriptableInt.GetValue();
            textField.text = intValue.ToString();
            
            variableProperty.objectReferenceValue = scriptableInt;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, textField.SetText);
        }
        
        public static void CreateIntVariableAndAssignTextFieldToEvent(TMP_Text textField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            ThrowIf.NotDerivedFrom<IntegerTextEventBinding>(bindingType);
            
            CreateNewAsset(textField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignTextFieldToIntVariableEvent((TMP_Text)target, asset, bindingType);
            }
        }
        
        public static void AssignTextFieldToFloatVariableEvent(TMP_Text textField, Object variableAsset, Type bindingType)
        {
            var scriptableFloat = (ScriptableFloat)variableAsset;
            var scriptableEvent = (DecimalTextEventBinding)textField.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");
            
            var floatValue = scriptableFloat.GetValue();
            textField.text = floatValue.ToString(CultureInfo.InvariantCulture);
            
            variableProperty.objectReferenceValue = scriptableFloat;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, textField.SetText);
        }
        
        public static void CreateFloatVariableAndAssignTextFieldToEvent(TMP_Text textField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableFloat>(variableType);
            ThrowIf.NotDerivedFrom<DecimalTextEventBinding>(bindingType);
            
            CreateNewAsset(textField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignTextFieldToFloatVariableEvent((TMP_Text)target, asset, bindingType);
            }
        }
        
        public static void AssignTextFieldToStringVariableEvent(TMP_Text textField, Object variableAsset)
        {
            var scriptableString = (ScriptableString)variableAsset;
            var scriptableEvent = textField.gameObject.AddComponent<ScriptableStringEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            var stringValue = scriptableString.GetValue();
            textField.text = stringValue;
            
            variableProperty.objectReferenceValue = scriptableString;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, textField.SetText);
        }
        
        public static void CreateStringVariableAndAssignTextFieldToEvent(TMP_Text textField, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableString>(variableType);
            
            CreateNewAsset(textField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignTextFieldToStringVariableEvent((TMP_Text)target, asset);
            }
        }

        public static void AssignDropdownToIntVariableEvent(TMP_Dropdown dropdown, Object variableAsset)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableEvent = dropdown.gameObject.AddComponent<ScriptableIntEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");
            
            var intValue = scriptableInt.GetValue();
            dropdown.SetValueWithoutNotify(intValue);
            
            variableProperty.objectReferenceValue = scriptableInt;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, dropdown.SetValueWithoutNotify);
        }
        
        public static void CreateIntVariableAndAssignDropdownToEvent(TMP_Dropdown dropdown, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            
            CreateNewAsset(dropdown, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignDropdownToIntVariableEvent(dropdown, asset);
            }
        }
        
        public static void AssignDropdownToEnumVariableEvent(TMP_Dropdown dropdown, Object variableAsset)
        {
            var scriptableEnum = (ScriptableEnum)variableAsset;
            var scriptableEvent = dropdown.gameObject.AddComponent<ScriptableEnumEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            var enumValueType = ScriptableEnumEditor.GetEnumValueType(scriptableEnum.GetType());
            var enumValueNames = Enum.GetNames(enumValueType).ToList();
            dropdown.ClearOptions();
            dropdown.AddOptions(enumValueNames);
            
            var enumIntValue = scriptableEnum.GetValue();
            dropdown.SetValueWithoutNotify(enumIntValue);
            
            variableProperty.objectReferenceValue = scriptableEnum;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, dropdown.SetValueWithoutNotify);
        }
        
        public static void CreateEnumVariableAndAssignDropdownToEvent(TMP_Dropdown dropdown, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableEnum>(variableType);
            
            CreateNewAsset(dropdown, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignDropdownToEnumVariableEvent(dropdown, asset);
            }
        }

        public static void AssignIntVariableToDropdownEvent(TMP_Dropdown dropdown, Object variableAsset)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            
            UnityEventTools.AddPersistentListener(dropdown.onValueChanged, scriptableInt.SetValue);
        }
        
        public static void CreateAndAssignIntVariableToDropdownEvent(TMP_Dropdown dropdown, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            
            var serializedObject = new SerializedObject(dropdown);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignIntVariableToDropdownEvent(dropdown, asset);
            }
        }

        public static void AssignEnumVariableToDropdownEvent(TMP_Dropdown dropdown, Object variableAsset)
        {
            var scriptableEnum = (ScriptableEnum)variableAsset;
            
            UnityEventTools.AddPersistentListener(dropdown.onValueChanged, scriptableEnum.SetValue);
        }
        
        public static void CreateAndAssignEnumVariableToDropdownEvent(TMP_Dropdown dropdown, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableEnum>(variableType);
            
            var serializedObject = new SerializedObject(dropdown);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignEnumVariableToDropdownEvent(dropdown, asset);
            }
        }
        
        public static void AssignToggleToBoolVariableEvent(Toggle toggle, Object variableAsset)
        {
            var scriptableBool = (ScriptableBool)variableAsset;
            var scriptableEvent = toggle.gameObject.AddComponent<ScriptableBoolEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            var boolValue = scriptableBool.GetValue();
            toggle.SetIsOnWithoutNotify(boolValue);
            
            variableProperty.objectReferenceValue = scriptableBool;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, toggle.SetIsOnWithoutNotify);
        }
        
        public static void CreateBoolVariableAndAssignToggleToEvent(Toggle toggle, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableBool>(variableType);
            
            CreateNewAsset(toggle, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignToggleToBoolVariableEvent(toggle, asset);
            }
        }

        public static void AssignBoolVariableToToggleEvent(Toggle toggle, Object variableAsset)
        {
            var scriptableBool = (ScriptableBool)variableAsset;
            
            UnityEventTools.AddPersistentListener(toggle.onValueChanged, scriptableBool.SetValue);
        }
        
        public static void CreateAndAssignBoolVariableToToggleEvent(Toggle toggle, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableBool>(variableType);
            
            var serializedObject = new SerializedObject(toggle);
            var valueChangedProperty = serializedObject.FindProperty("onValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignBoolVariableToToggleEvent(toggle, asset);
            }
        }
        
        public static void AssignSliderToIntVariableEvent(Slider slider, Object variableAsset, Type bindingType)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableEvent = (IntegerSliderEventBinding)slider.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            float floatValue = scriptableInt.GetValue();
            slider.SetValueWithoutNotify(floatValue);
            
            variableProperty.objectReferenceValue = scriptableInt;

            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
                
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, slider.SetValueWithoutNotify);
        }
        
        public static void CreateIntVariableAndAssignSliderToEvent(Slider slider, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            ThrowIf.NotDerivedFrom<IntegerSliderEventBinding>(bindingType);
            
            CreateNewAsset(slider, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignSliderToIntVariableEvent((Slider)target, asset, bindingType);
            }
        }

        public static void AssignSliderToFloatVariableEvent(Slider slider, Object variableAsset)
        {
            var scriptableFloat = (ScriptableFloat)variableAsset;
            var scriptableEvent = slider.gameObject.AddComponent<ScriptableFloatEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            float floatValue = scriptableFloat.GetValue();
            slider.SetValueWithoutNotify(floatValue);
            
            variableProperty.objectReferenceValue = scriptableFloat;

            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
                
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, slider.SetValueWithoutNotify);
        }

        public static void CreateFloatVariableAndAssignSliderToEvent(Slider slider, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableFloat>(variableType);
            
            CreateNewAsset(slider, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignSliderToFloatVariableEvent((Slider)target, asset);
            }
        }
        
        public static void AssignFloatVariableToSliderEvent(Slider slider, Object variableAsset)
        {
            var scriptableFloat = (ScriptableFloat)variableAsset;
                
            UnityEventTools.AddPersistentListener(slider.onValueChanged, scriptableFloat.SetValue);
        }
        
        public static void CreateAndAssignFloatVariableToSliderEvent(Slider slider, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableFloat>(variableType);
            
            var serializedObject = new SerializedObject(slider);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignFloatVariableToSliderEvent(slider, asset);
            }
        }
        
        public static void AssignInputFieldToStringVariableEvent(TMP_InputField inputField, Object variableAsset)
        {
            var scriptableString = (ScriptableString)variableAsset;
            var scriptableEvent = inputField.gameObject.AddComponent<ScriptableStringEvent>();
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            var stringValue = scriptableString.GetValue();
            inputField.text = stringValue;
            
            variableProperty.objectReferenceValue = scriptableString;
            
            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
            
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, inputField.SetTextWithoutNotify);
        }
        
        public static void CreateStringVariableAndAssignInputFieldToEvent(TMP_InputField inputField, Type variableType) 
        {
            ThrowIf.NotDerivedFrom<ScriptableString>(variableType);
            
            CreateNewAsset(inputField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignInputFieldToStringVariableEvent((TMP_InputField)target, asset);
            }
        }
        
        public static void AssignInputFieldToIntVariableEvent(TMP_InputField inputField, Object variableAsset, Type bindingType)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableEvent = (IntegerTextEventBinding)inputField.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            int intValue = scriptableInt.GetValue();
            inputField.text = intValue.ToString();

            variableProperty.objectReferenceValue = scriptableInt;

            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
                
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, inputField.SetTextWithoutNotify);
        }
        
        public static void CreateIntVariableAndAssignInputFieldToEvent(TMP_InputField inputField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            ThrowIf.NotDerivedFrom<IntegerTextEventBinding>(bindingType);
            
            CreateNewAsset(inputField, variableType, OnAssetCreated);
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignInputFieldToIntVariableEvent((TMP_InputField)target, asset, bindingType);
            }
        }
        
        public static void AssignInputFieldToFloatVariableEvent(TMP_InputField inputField, Object variableAsset, Type bindingType)
        {
            var scriptableFloat = (ScriptableFloat)variableAsset;
            var scriptableEvent = (DecimalTextEventBinding)inputField.gameObject.AddComponent(bindingType);
            var serializedEvent = new SerializedObject(scriptableEvent);
            var variableProperty = serializedEvent.FindProperty("_variable");

            float floatValue = scriptableFloat.GetValue();
            inputField.text = floatValue.ToString(CultureInfo.InvariantCulture);

            variableProperty.objectReferenceValue = scriptableFloat;

            serializedEvent.ApplyModifiedProperties();
            serializedEvent.Dispose();
                
            UnityEventTools.AddPersistentListener(scriptableEvent.ValueChanged, inputField.SetTextWithoutNotify);
        }
        
        public static void AssignStringVariableToInputFieldEvent(TMP_InputField inputField, Object variableAsset)
        {
            var scriptableString = (ScriptableString)variableAsset;
                
            UnityEventTools.AddPersistentListener(inputField.onValueChanged, scriptableString.SetValue);
        }
        
        public static void CreateAndAssignStringVariableToInputFieldEvent(TMP_InputField inputField, Type variableType)
        {
            ThrowIf.NotDerivedFrom<ScriptableString>(variableType);
            
            var serializedObject = new SerializedObject(inputField);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignStringVariableToInputFieldEvent(inputField, asset);
            }
        }

        public static void AssignIntVariableToInputFieldEvent(TMP_InputField inputField, Object variableAsset, Type bindingType)
        {
            var scriptableInt = (ScriptableInt)variableAsset;
            var scriptableIntBinder = (IntegerTextBinding)inputField.gameObject.AddComponent(bindingType);
            var serializedBinder = new SerializedObject(scriptableIntBinder);
            var variableProperty = serializedBinder.FindProperty("_variable");

            variableProperty.objectReferenceValue = scriptableInt;

            serializedBinder.ApplyModifiedProperties();
            serializedBinder.Dispose();
                
            UnityEventTools.AddPersistentListener(inputField.onValueChanged, scriptableIntBinder.SetValue);
        }
        
        public static void CreateAndAssignIntVariableToInputFieldEvent(TMP_InputField inputField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableInt>(variableType);
            ThrowIf.NotDerivedFrom<IntegerTextBinding>(bindingType);
            
            var serializedObject = new SerializedObject(inputField);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignIntVariableToInputFieldEvent((TMP_InputField)target, asset, bindingType);
            }
        }

        public static void AssignFloatVariableToInputFieldEvent(TMP_InputField inputField, Object variableAsset, Type bindingType)
        {
            var scriptableFloat = (ScriptableFloat)variableAsset;
            var scriptableFloatBinding = (DecimalTextBinding)inputField.gameObject.AddComponent(bindingType);
            var serializedBinder = new SerializedObject(scriptableFloatBinding);
            var variableProperty = serializedBinder.FindProperty("_variable");

            variableProperty.objectReferenceValue = scriptableFloat;

            serializedBinder.ApplyModifiedProperties();
            serializedBinder.Dispose();
                
            UnityEventTools.AddPersistentListener(inputField.onValueChanged, scriptableFloatBinding.SetValue);
        }
        
        public static void CreateAndAssignFloatVariableToInputFieldEvent(TMP_InputField inputField, Type variableType, Type bindingType)
        {
            ThrowIf.NotDerivedFrom<ScriptableFloat>(variableType);
            ThrowIf.NotDerivedFrom<DecimalTextBinding>(bindingType);
            
            var serializedObject = new SerializedObject(inputField);
            var valueChangedProperty = serializedObject.FindProperty("m_OnValueChanged");
            
            CreateNewAsset(valueChangedProperty, variableType, OnAssetCreated);
            
            serializedObject.Dispose();
            
            void OnAssetCreated(Object asset, Object target, string propertyPath)
            {
                AssignFloatVariableToInputFieldEvent((TMP_InputField)target, asset, bindingType);
            }
        }
        
        public static void CreateNewAsset(Object targetObject, Type variableType, AssetCreatedCallback callback)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                if (targetObject is Component component)
                {
                    Scene scene = component.gameObject.scene;
                    
                    if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
                    {
                        path = Path.GetDirectoryName(scene.path);
                    }
                    else
                    {
                        GameObject prefab = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
                        path = prefab == null ? "Assets" : Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab));
                    }
                }
                else
                {
                    path = "Assets";
                }
            }
            else if (!Directory.Exists(path)) 
            {
                path = Path.GetDirectoryName(path);
            }
            
            ScriptableObject newVariable = ScriptableObject.CreateInstance(variableType);

            AssetCreationCallback action = ScriptableObject.CreateInstance<AssetCreationCallback>();
            action.Setup(targetObject, null, variableType, callback);
            
            string defaultName = $"New{variableType.Name}.asset";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                action, 
                assetPathAndName,
                AssetPreview.GetMiniThumbnail(newVariable),
                null);
        }
        
        public static void CreateNewAsset(SerializedProperty property, Type variableType, AssetCreatedCallback callback)
        {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (string.IsNullOrEmpty(path))
            {
                if (property.serializedObject.targetObject is Component component)
                {
                    Scene scene = component.gameObject.scene;
                    
                    if (scene.IsValid() && !string.IsNullOrEmpty(scene.path))
                    {
                        path = Path.GetDirectoryName(scene.path);
                    }
                    else
                    {
                        GameObject prefab = PrefabUtility.GetNearestPrefabInstanceRoot(component.gameObject);
                        path = prefab == null ? "Assets" : Path.GetDirectoryName(AssetDatabase.GetAssetPath(prefab));
                    }
                }
                else
                {
                    path = "Assets";
                }
            }
            else if (!Directory.Exists(path)) 
            {
                path = Path.GetDirectoryName(path);
            }
            
            ScriptableObject newVariable = ScriptableObject.CreateInstance(variableType);

            Object target = property.serializedObject.targetObject;
            AssetCreationCallback action = ScriptableObject.CreateInstance<AssetCreationCallback>();
            action.Setup(target, property.propertyPath, variableType, callback);
            
            string defaultName = $"New{variableType.Name}.asset";
            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath($"{path}/{defaultName}");

            ProjectWindowUtil.StartNameEditingIfProjectWindowExists(
                0,
                action, 
                assetPathAndName,
                AssetPreview.GetMiniThumbnail(newVariable),
                null);
        }
    }
}

#endif