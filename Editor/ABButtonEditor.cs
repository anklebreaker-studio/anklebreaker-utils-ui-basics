using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;
using AnkleBreaker.Utils.Inspector;
using AnkleBreaker.Utils.Inspector.Editor;

namespace AnkleBreaker.Utils.UIBasics.Editor
{
    /// <summary>
    /// Custom editor for ABButton and all subclasses.
    /// Extends Unity's ButtonEditor to preserve the standard Button inspector
    /// (transitions, navigation, onClick) and appends our custom fields
    /// with full AB attribute support (FoldoutGroup, ShowInInspector, [Button], etc.).
    /// </summary>
    [CustomEditor(typeof(ABButton), true)]
    [CanEditMultipleObjects]
    public class ABButtonEditor : ButtonEditor
    {
        private static readonly HashSet<string> BaseButtonProperties = new HashSet<string>
        {
            "m_Script", "m_Navigation", "m_Transition", "m_Colors",
            "m_SpriteState", "m_AnimationTriggers", "m_TargetGraphic",
            "m_Interactable", "m_OnClick"
        };

        private const BindingFlags MemberFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private List<SerializedProperty> _customProperties;
        private List<ButtonMethodInfo> _buttonMethods;
        private List<ShowInInspectorEntry> _showInInspectorEntries;

        private static readonly Dictionary<string, bool> FoldoutStates =
            new Dictionary<string, bool>();

        protected override void OnEnable()
        {
            base.OnEnable();
            _buttonMethods = ButtonDrawerUtility.CollectButtonMethods(target);
            _showInInspectorEntries = CollectShowInInspector(target);
            CacheCustomProperties();
        }

        private void CacheCustomProperties()
        {
            _customProperties = new List<SerializedProperty>();
            SerializedProperty prop = serializedObject.GetIterator();
            if (!prop.NextVisible(true)) return;
            do
            {
                if (!BaseButtonProperties.Contains(prop.name))
                    _customProperties.Add(prop.Copy());
            }
            while (prop.NextVisible(false));
        }

        private const string ButtonFoldoutTitle = "Button";
        private const FoldoutGroupStyle ButtonFoldoutStyle = FoldoutGroupStyle.CenterLine;

        public override void OnInspectorGUI()
        {
            // --- Unity's standard Button inspector inside a foldout ---
            if (BeginFoldout(ButtonFoldoutTitle, ButtonFoldoutStyle, null))
            {
                base.OnInspectorGUI();
            }
            EndFoldout(ButtonFoldoutTitle);

            if (_customProperties == null || _customProperties.Count == 0)
                return;

            // --- Our custom properties ---
            serializedObject.Update();

            EditorGUILayout.Space(6);

            var entries = BuildEntries();
            DrawEntries(entries);

            serializedObject.ApplyModifiedProperties();

            // --- ShowInInspector runtime properties ---
            DrawShowInInspector();

            // --- [Button] methods ---
            ButtonDrawerUtility.DrawButtons(_buttonMethods, targets);
        }

        #region Entry Building & Drawing

        private struct PropertyEntry
        {
            public SerializedProperty Property;
            public string FoldoutGroup;
            public FoldoutGroupStyle FoldoutStyle;
            public Color? FoldoutColor;
            public int Order;
            public int OriginalIndex;
        }

        private List<PropertyEntry> BuildEntries()
        {
            var entries = new List<PropertyEntry>();
            Type targetType = target.GetType();

            for (int i = 0; i < _customProperties.Count; i++)
            {
                var prop = _customProperties[i];
                FieldInfo field = FindField(targetType, prop.name);

                var entry = new PropertyEntry
                {
                    Property = prop,
                    Order = 0,
                    OriginalIndex = i,
                };

                if (field != null)
                {
                    var foldout = field.GetCustomAttribute<FoldoutGroupAttribute>();
                    if (foldout != null)
                    {
                        entry.FoldoutGroup = foldout.GroupName;
                        entry.FoldoutStyle = foldout.Style;
                        if (foldout.HasCustomColor)
                            entry.FoldoutColor = new Color(foldout.R, foldout.G, foldout.B);
                    }

                    var order = field.GetCustomAttribute<PropertyOrderAttribute>();
                    if (order != null) entry.Order = order.Order;
                }

                entries.Add(entry);
            }

            entries.Sort((a, b) =>
            {
                int c = a.Order.CompareTo(b.Order);
                return c != 0 ? c : a.OriginalIndex.CompareTo(b.OriginalIndex);
            });

            return entries;
        }

        private void DrawEntries(List<PropertyEntry> entries)
        {
            string currentFoldout = null;

            for (int i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                // Foldout group transitions
                if (entry.FoldoutGroup != currentFoldout)
                {
                    if (currentFoldout != null) EndFoldout(currentFoldout);

                    if (entry.FoldoutGroup != null)
                    {
                        if (!BeginFoldout(entry.FoldoutGroup, entry.FoldoutStyle, entry.FoldoutColor))
                        {
                            currentFoldout = entry.FoldoutGroup;
                            continue;
                        }
                    }

                    currentFoldout = entry.FoldoutGroup;
                }
                else if (currentFoldout != null)
                {
                    string key = target.GetType().FullName + "_foldout_" + currentFoldout;
                    if (FoldoutStates.ContainsKey(key) && !FoldoutStates[key])
                        continue;
                }

                // PropertyField handles DecoratorDrawers (SectionHeader, etc.) automatically
                EditorGUILayout.PropertyField(entry.Property, true);
            }

            if (currentFoldout != null) EndFoldout(currentFoldout);
        }

        private static FieldInfo FindField(Type type, string fieldName)
        {
            while (type != null)
            {
                FieldInfo field = type.GetField(fieldName, MemberFlags);
                if (field != null) return field;
                type = type.BaseType;
            }
            return null;
        }

        #endregion

        #region Foldout Helpers

        private bool BeginFoldout(string title, FoldoutGroupStyle style, Color? color)
        {
            string key = target.GetType().FullName + "_foldout_" + title;
            if (!FoldoutStates.ContainsKey(key)) FoldoutStates[key] = false;

            switch (style)
            {
                case FoldoutGroupStyle.Line:
                    FoldoutStates[key] = EditorGUILayout.Foldout(FoldoutStates[key], "", true);
                    var lineRect = GUILayoutUtility.GetLastRect();
                    lineRect.xMin += 12f;
                    var prevColor = GUI.color;
                    GUI.color = color ?? GUI.color;
                    EditorGUI.LabelField(lineRect, title, EditorStyles.boldLabel);
                    GUI.color = prevColor;
                    if (FoldoutStates[key])
                    {
                        Rect sep = EditorGUILayout.GetControlRect(false, 1f);
                        EditorGUI.DrawRect(sep, EditorGUIUtility.isProSkin
                            ? new Color(0.3f, 0.3f, 0.3f) : new Color(0.7f, 0.7f, 0.7f));
                    }
                    break;

                case FoldoutGroupStyle.CenterLine:
                    FoldoutStates[key] = EditorGUILayout.Foldout(FoldoutStates[key], "", true);
                    var clRect = GUILayoutUtility.GetLastRect();
                    string arrow = FoldoutStates[key] ? "\u25BC" : "\u25B6";
                    string display = $"{arrow}  {title}  {arrow}";
                    var titleStyle = new GUIStyle(EditorStyles.boldLabel)
                        { alignment = TextAnchor.MiddleCenter };
                    if (color.HasValue) titleStyle.normal.textColor = color.Value;
                    float textW = titleStyle.CalcSize(new GUIContent(display)).x;
                    float cx = clRect.x + clRect.width * 0.5f;
                    float ly = clRect.y + clRect.height * 0.5f;
                    Color lineCol = color ?? (EditorGUIUtility.isProSkin
                        ? new Color(0.4f, 0.4f, 0.4f) : new Color(0.6f, 0.6f, 0.6f));
                    EditorGUI.DrawRect(new Rect(clRect.x + 12f, ly,
                        cx - textW * 0.5f - clRect.x - 16f, 1f), lineCol);
                    EditorGUI.DrawRect(new Rect(cx + textW * 0.5f + 4f, ly,
                        clRect.xMax - cx - textW * 0.5f - 4f, 1f), lineCol);
                    EditorGUI.LabelField(new Rect(clRect.x + 12f, clRect.y,
                        clRect.width - 12f, clRect.height), display, titleStyle);
                    break;

                case FoldoutGroupStyle.Clean:
                    FoldoutStates[key] = EditorGUILayout.Foldout(FoldoutStates[key], "", true);
                    var cleanRect = GUILayoutUtility.GetLastRect();
                    cleanRect.xMin += 12f;
                    var cleanStyle = new GUIStyle(EditorStyles.boldLabel);
                    if (color.HasValue) cleanStyle.normal.textColor = color.Value;
                    EditorGUI.LabelField(cleanRect, title, cleanStyle);
                    break;

                default:
                    FoldoutStates[key] = EditorGUILayout.Foldout(
                        FoldoutStates[key], title, true, EditorStyles.foldoutHeader);
                    break;
            }

            if (FoldoutStates[key]) EditorGUI.indentLevel++;
            return FoldoutStates[key];
        }

        private void EndFoldout(string foldoutName)
        {
            string key = target.GetType().FullName + "_foldout_" + foldoutName;
            if (FoldoutStates.ContainsKey(key) && FoldoutStates[key])
                EditorGUI.indentLevel--;
            EditorGUILayout.Space(2);
        }

        #endregion

        #region ShowInInspector

        private struct ShowInInspectorEntry
        {
            public string Name;
            public Func<object> Getter;
            public bool RuntimeOnly;
        }

        private static List<ShowInInspectorEntry> CollectShowInInspector(UnityEngine.Object target)
        {
            var entries = new List<ShowInInspectorEntry>();
            if (target == null) return entries;

            Type type = target.GetType();

            foreach (var field in type.GetFields(MemberFlags))
            {
                var attr = field.GetCustomAttribute<ShowInInspectorAttribute>();
                if (attr == null) continue;
                var f = field;
                entries.Add(new ShowInInspectorEntry
                {
                    Name = ObjectNames.NicifyVariableName(f.Name),
                    Getter = () => f.GetValue(target),
                    RuntimeOnly = attr.RuntimeOnly
                });
            }

            foreach (var prop in type.GetProperties(MemberFlags))
            {
                var attr = prop.GetCustomAttribute<ShowInInspectorAttribute>();
                if (attr == null || !prop.CanRead) continue;
                var p = prop;
                entries.Add(new ShowInInspectorEntry
                {
                    Name = ObjectNames.NicifyVariableName(p.Name),
                    Getter = () => p.GetValue(target),
                    RuntimeOnly = attr.RuntimeOnly
                });
            }

            return entries;
        }

        private void DrawShowInInspector()
        {
            if (_showInInspectorEntries == null || _showInInspectorEntries.Count == 0)
                return;

            bool anyVisible = _showInInspectorEntries.Any(e =>
                !e.RuntimeOnly || Application.isPlaying);
            if (!anyVisible) return;

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Runtime Properties", EditorStyles.boldLabel);

            bool wasEnabled = GUI.enabled;
            GUI.enabled = false;

            foreach (var entry in _showInInspectorEntries)
            {
                if (entry.RuntimeOnly && !Application.isPlaying) continue;
                try
                {
                    object value = entry.Getter();
                    EditorGUILayout.TextField(entry.Name,
                        value != null ? value.ToString() : "(null)");
                }
                catch
                {
                    EditorGUILayout.TextField(entry.Name, "(error)");
                }
            }

            GUI.enabled = wasEnabled;
        }

        #endregion
    }
}