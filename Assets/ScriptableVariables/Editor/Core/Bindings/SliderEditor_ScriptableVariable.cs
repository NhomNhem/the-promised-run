#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using OpenUtility.Editor;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace OpenUtility.Data.Editor
{
    [CanEditMultipleObjects]
    [CustomEditor(typeof(Slider), true)]
    public class SliderEditor_ScriptableVariable : SliderEditor
    {
        private static readonly Dictionary<BindingGoal, Dictionary<string, BindingData>> _bindingDataCache = new ();
        private static readonly Dictionary<BindingGoal, Dictionary<string, SelectionData>> _selectionDataCache = new ();
        
        private static Type[] SupportedVariableTypes { get; } = new Type[]
        {
            typeof(ScriptableInt),
            typeof(ScriptableFloat)
        };

        [DidReloadScripts]
        private static void ClearBindingDataCache()
        {
            _bindingDataCache.Clear();
            _selectionDataCache.Clear();
        }
        
        private static Dictionary<Type, List<Object>> GetScriptableVariableAssetData(BindingGoal goal) 
        {
            var guids = AssetDatabase.FindAssets("t:ScriptableVariable`1");
            if (guids.Length == 0)
                return (null);

            Dictionary<string, BindingData> bindingData = GetBindingData(goal);
            
            var assets = guids.Select(AssetDatabase.GUIDToAssetPath).Select(AssetDatabase.LoadAssetAtPath<Object>);
            var dictionary = new Dictionary<Type, List<Object>>();

            foreach (var asset in assets)
            {
                var typeOfAsset = asset.GetType();
                if (!SupportedVariableTypes.Any(type => type.IsAssignableFrom(typeOfAsset)))
                    continue;
                
                if (bindingData.All(bd => bd.Value.variableType != typeOfAsset))
                    continue;
                
                if (!dictionary.TryGetValue(typeOfAsset, out List<Object> list))
                {
                    list = new List<Object>();
                    dictionary[typeOfAsset] = list;
                }
                
                var assignableTypes = dictionary.Keys.Where(t => t.IsAssignableFrom(typeOfAsset));
                foreach (var type in assignableTypes)
                    dictionary[type].Add(asset);
            }

            return (dictionary);
        }
        
        private static Dictionary<string, SelectionData> GetSelectableItems(BindingGoal goal)
        {
            if (_selectionDataCache.TryGetValue(goal, out Dictionary<string, SelectionData> cache))
                return (cache);

            cache = new Dictionary<string, SelectionData>();
            
            Dictionary<string, BindingData> bindingData = GetBindingData(goal);
            Dictionary<Type, List<Object>> assetData = GetScriptableVariableAssetData(goal);
            foreach (KeyValuePair<string, BindingData> dataPoint in bindingData)
            {
                var nameOfOption = dataPoint.Key;
                var data = dataPoint.Value;
                
                if (!assetData.TryGetValue(data.variableType, out List<Object> assets))
                    continue;
                
                foreach (Object asset in assets)
                {
                    string nameOfSubOption = asset.name;
                    string path = $"{nameOfOption}/{nameOfSubOption}";
                    cache.TryAdd(path, new SelectionData(asset, data.bindingType));
                }
            }
            
            _selectionDataCache[goal] = cache;

            return (cache);
        }

        private static Dictionary<string, BindingData> GetBindingData(BindingGoal goal)
        {
            if (_bindingDataCache.TryGetValue(goal, out Dictionary<string, BindingData> cache))
                return (cache);
            
            cache = new Dictionary<string, BindingData>();

            TypeCache.TypeCollection collection = TypeCache.GetTypesWithAttribute<ScriptableVariableBinder>();
            foreach (Type type in collection)
            {
                var attributes = type.GetCustomAttributes<ScriptableVariableBinder>();
                var attribute = attributes.FirstOrDefault(a => a.TypeOfComponentToBindTo == typeof(Slider));
                if (attribute == null)
                    continue;
                
                if (!attribute.Goal.HasFlag(goal))
                    continue;
                
                var valueType = attribute.TypeOfValue;

                Type variableType;
                Type bindingType;
                if (typeof(ScriptableFloat).IsAssignableFrom(type))
                {
                    variableType = type;
                    bindingType = null;
                }
                else
                {
                    variableType = attribute.TypeOfScriptableVariable;
                    bindingType = ScriptableVariableFactory.GetTypeOfComponentBinding<Slider>(type, goal);
                }
                
                string nameOfOption = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(valueType.Name);
                string nameOfSubOption = attribute.DisplayName ?? bindingType?.Name ?? variableType.Name;
                string path = $"{nameOfOption}/{nameOfSubOption}";
                
                var bindingData = new BindingData(variableType, bindingType);
                if (!cache.TryAdd(path, bindingData))
                    Debug.LogWarningFormat(BindingData.DUPLICATE_TYPE_WARNING, type, nameOfSubOption, nameOfOption);
            }
            
            _bindingDataCache[goal] = cache;

            return (cache);
        }
        
        private bool _foldoutBindScriptableVariableGUI = false;
        private bool _foldoutListenToScriptableVariableGUI = false;
        
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Scriptable Variables", new GUIStyle(EditorStyles.boldLabel) { fontSize = 12 });
            
            OnBindScriptableVariableGUI();
            EditorGUILayout.Space(12f);
            OnListenToScriptableVariableGUI();
        }

        private void OnBindScriptableVariableGUI()
        {
            var totalRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.foldout);
            var contentRect = EditorGUI.PrefixLabel(totalRect, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            var foldoutRect = new Rect(totalRect.x, totalRect.y, 8f, EditorGUIUtility.singleLineHeight);
            _foldoutBindScriptableVariableGUI = EditorGUI.Foldout(foldoutRect, _foldoutBindScriptableVariableGUI, GUIContent.none, false);
            
            var content = new GUIContent("Bind to Scriptable Variable");
            var selectButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            contentRect.height = 20;
            contentRect.width -= 24;
            
            if (GUI.Button(contentRect, content, selectButtonStyle))
                OnSelectButtonBindingClicked(contentRect);
            
            var createRect = new Rect(contentRect.xMax + 4, contentRect.y, 20, contentRect.height);
            var buttonContent = EditorGUIUtility.IconContent("Toolbar Plus");
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };  
            
            if (GUI.Button(createRect, buttonContent, buttonStyle))
                OnCreateButtonBindingClicked(contentRect);

            if (_foldoutBindScriptableVariableGUI)
            {
                EditorGUILayout.Space(); 
                OnInfoMessageGUI("Select or Create a variable that will <b>receive</b> the value of this dropdown.");
            }
        }

        private void OnSelectButtonBindingClicked(Rect rect)
        {
            Texture2D variableIcon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;
            Dictionary<string, SelectionData> selectionData = GetSelectableItems(BindingGoal.ReceiveValue);
            ExtendedDropdownBuilder builder = new ExtendedDropdownBuilder("Select Binding", rect);
            
            var intItems = selectionData.Where(bd => bd.Key.StartsWith("Int32/")).ToArray();
            builder.StartIndent("Whole Numbers");
            for (int i = 0; i < intItems.Length; i++)
            {
                var item = intItems[i];
                var path = item.Key;
                var itemName = path.Substring(path.IndexOf('/') + 1);
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnSelectIntegerVariableBinding);
            }
            builder.EndIndent();
            
            var floatItems = selectionData.Where(bd => bd.Key.StartsWith("Single/")).ToArray();
            builder.StartIndent("Decimal Numbers");
            for (int i = 0; i < floatItems.Length; i++)
            {
                var item = floatItems[i];
                var path = item.Key;
                var itemName = path.Substring(path.IndexOf('/') + 1);
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnSelectFloatVariableBinding);
            }
            builder.EndIndent();
            
            var maxItemsPerColumn = Mathf.Max(intItems.Length, floatItems.Length);
            var minimumHeight = (maxItemsPerColumn + 3) * 20f;
            var minimumSize = new Vector2(rect.width, minimumHeight);
            builder.AddMinimumSize(minimumSize).GetResult().Show();
        }

        private void OnSelectIntegerVariableBinding(object data)
        {
            var selectionData = (SelectionData)data;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.AssignIntVariableForSlider(slider, selectionData.variableAsset, selectionData.bindingType);
        }

        private void OnSelectFloatVariableBinding(object data)
        {
            var selectionData = (SelectionData)data;
            var variableAsset = selectionData.variableAsset;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.AssignFloatVariableToSliderEvent(slider, variableAsset);
        }

        private void OnCreateButtonBindingClicked(Rect rect)
        {
            Texture2D variableIcon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;
            Dictionary<string, BindingData> bindingData = GetBindingData(BindingGoal.ReceiveValue);
            ExtendedDropdownBuilder builder = new ExtendedDropdownBuilder("Create Binding", rect);
            
            var intItems = bindingData.Where(bd => bd.Key.StartsWith("Int32/")).ToArray();
            builder.StartIndent("Whole Numbers");
            for (int i = 0; i < intItems.Length; i++)
            {
                var item = intItems[i];
                var itemName = item.Key.Split('/')[1];
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnCreateIntegerVariableBinding);
            }
            builder.EndIndent();
            
            var floatItems = bindingData.Where(bd => bd.Key.StartsWith("Single/")).ToArray();
            builder.StartIndent("Decimal Numbers");
            for (int i = 0; i < floatItems.Length; i++)
            {
                var item = floatItems[i];
                var itemName = item.Key.Split('/')[1];
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnCreateFloatVariableBinding);
            }
            builder.EndIndent();

            var maxItemsPerColumn = Mathf.Max(SupportedVariableTypes.Length, intItems.Length, floatItems.Length);
            var minimumHeight = (maxItemsPerColumn + 3) * 20f;
            var minimumSize = new Vector2(rect.width, minimumHeight);
            builder.AddMinimumSize(minimumSize).GetResult().Show();
        }

        private void OnCreateIntegerVariableBinding(object data)
        {
            var bindingData = (BindingData)data;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.CreateAndAssignIntVariableForSlider(slider, bindingData.variableType, bindingData.bindingType);
        }

        private void OnCreateFloatVariableBinding(object data)
        {
            var bindingData = (BindingData)data;
            var variableType = bindingData.variableType;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.CreateAndAssignFloatVariableToSliderEvent(slider, variableType);
        }

        private void OnListenToScriptableVariableGUI()
        {
            var totalRect = GUILayoutUtility.GetRect(GUIContent.none, EditorStyles.foldout);
            var contentRect = EditorGUI.PrefixLabel(totalRect, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);
            var foldoutRect = new Rect(totalRect.x, totalRect.y, 8f, EditorGUIUtility.singleLineHeight);
            _foldoutListenToScriptableVariableGUI = EditorGUI.Foldout(foldoutRect, _foldoutListenToScriptableVariableGUI, GUIContent.none, false);
            
            var content = new GUIContent("Listen to Scriptable Variable");
            var selectButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            contentRect.height = 20;
            contentRect.width -= 24;
            
            if (GUI.Button(contentRect, content, selectButtonStyle))
                OnSelectEventButtonClicked(contentRect);
            
            var createRect = new Rect(contentRect.xMax + 4, contentRect.y, 20, contentRect.height);
            var buttonContent = EditorGUIUtility.IconContent("Toolbar Plus");
            var buttonStyle = new GUIStyle(GUI.skin.button)
            {
                padding = new RectOffset(0, 0, 0, 0),
                margin = new RectOffset(0, 0, 0, 0),
                alignment = TextAnchor.MiddleCenter
            };  
            
            if (GUI.Button(createRect, buttonContent, buttonStyle))
                OnCreateEventButtonClicked(contentRect);

            if (_foldoutListenToScriptableVariableGUI)
            {
                EditorGUILayout.Space(); 
                OnInfoMessageGUI("Select or Create a variable that will <b>determine</b> the value of this dropdown.");
            }
        }

        private void OnSelectEventButtonClicked(Rect rect)
        {
            Texture2D variableIcon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;
            Dictionary<string, SelectionData> selectionData = GetSelectableItems(BindingGoal.DetermineValue);
            ExtendedDropdownBuilder builder = new ExtendedDropdownBuilder("Select Event", rect);
            
            var intItems = selectionData.Where(bd => bd.Key.StartsWith("Int32/")).ToArray();
            builder.StartIndent("Whole Numbers");
            for (int i = 0; i < intItems.Length; i++)
            {
                var item = intItems[i];
                var path = item.Key;
                var itemName = path.Substring(path.IndexOf('/') + 1);
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnSelectIntVariableEvent);
            }
            builder.EndIndent();
            
            var floatItems = selectionData.Where(bd => bd.Key.StartsWith("Single/")).ToArray();
            builder.StartIndent("Decimal Numbers");
            for (int i = 0; i < floatItems.Length; i++)
            {
                var item = floatItems[i];
                var path = item.Key;
                var itemName = path.Substring(path.IndexOf('/') + 1);
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnSelectFloatVariableEvent);
            }
            builder.EndIndent();
            
            var maxItemsPerColumn = Mathf.Max(intItems.Length, floatItems.Length);
            var minimumHeight = (maxItemsPerColumn + 3) * 20f;
            var minimumSize = new Vector2(rect.width, minimumHeight);
            builder.AddMinimumSize(minimumSize).GetResult().Show();
        }
        
        private void OnSelectIntVariableEvent(object data)
        {
            var selectionData = (SelectionData)data;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.AssignSliderToIntVariableEvent(slider, selectionData.variableAsset, selectionData.bindingType);
        }
        
        private void OnSelectFloatVariableEvent(object data)
        {
            var selectionData = (SelectionData)data;
            var variableAsset = selectionData.variableAsset;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.AssignSliderToFloatVariableEvent(slider, variableAsset);
        }

        private void OnCreateEventButtonClicked(Rect rect)
        {
            Texture2D variableIcon = (Texture2D)EditorGUIUtility.IconContent("ScriptableObject Icon").image;
            Dictionary<string, BindingData> bindingData = GetBindingData(BindingGoal.DetermineValue);
            ExtendedDropdownBuilder builder = new ExtendedDropdownBuilder("Create Binding", rect);
            
            var intItems = bindingData.Where(bd => bd.Key.StartsWith("Int32/")).ToArray();
            builder.StartIndent("Whole Numbers");
            for (int i = 0; i < intItems.Length; i++)
            {
                var item = intItems[i];
                var itemName = item.Key.Split('/')[1];
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnCreateIntVariableEvent);
            }
            builder.EndIndent();
            
            var floatItems = bindingData.Where(bd => bd.Key.StartsWith("Single/")).ToArray();
            builder.StartIndent("Decimal Numbers");
            for (int i = 0; i < floatItems.Length; i++)
            {
                var item = floatItems[i];
                var itemName = item.Key.Split('/')[1];
                
                builder.AddItem(itemName, false, variableIcon, item.Value, OnCreateFloatVariableEvent);
            }
            builder.EndIndent();

            var maxItemsPerColumn = Mathf.Max(SupportedVariableTypes.Length, intItems.Length, floatItems.Length);
            var minimumHeight = (maxItemsPerColumn + 3) * 20f;
            var minimumSize = new Vector2(rect.width, minimumHeight);
            builder.AddMinimumSize(minimumSize).GetResult().Show();
        }
        
        private void OnCreateIntVariableEvent(object data)
        {
            var bindingData = (BindingData)data;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.CreateIntVariableAndAssignSliderToEvent(slider, bindingData.variableType, bindingData.bindingType);
        }
        
        private void OnCreateFloatVariableEvent(object data)
        {
            var bindingData = (BindingData)data;
            var variableType = bindingData.variableType;
            var slider = (Slider)target;
            
            ScriptableVariableFactory.CreateFloatVariableAndAssignSliderToEvent(slider, variableType);
        }

        private void OnInfoMessageGUI(string message)
        {
            var fieldStyle = new GUIStyle(EditorStyles.label) { richText = true, wordWrap = true };
            var icon = EditorGUIUtility.IconContent("console.infoicon");

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label(icon, GUILayout.Width(32), GUILayout.Height(32));
            EditorGUILayout.LabelField(message, fieldStyle);
            EditorGUILayout.EndHorizontal();
        }
    }
}

#endif