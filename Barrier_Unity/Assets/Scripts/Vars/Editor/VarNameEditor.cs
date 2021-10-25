using UnityEngine;
using UnityEditor;
using System;
using System.Linq;


[CustomPropertyDrawer(typeof(VarNameSelector))]
public class VarNameEditor : PropertyDrawer
{
   const string propertyName = "Name";

   public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
   {
      SerializedProperty sp = property.FindPropertyRelative(propertyName);

      string[] options = Enum.GetNames(typeof(VarName)).Where(v => !v.ToVarName().IsHideInInspector()).ToArray();

      EditorGUI.BeginProperty(position, label, property);

      int index = Array.FindIndex(options, o => o == sp.stringValue);
      if (index < 0)
         index = 0;

      Rect pl = EditorGUI.PrefixLabel(position, label);
      index = EditorGUI.Popup(pl, index, options);

      sp.stringValue = index == 0 ? "" : options[index];

      property.serializedObject.ApplyModifiedProperties();

      EditorGUI.EndProperty();
   }
}