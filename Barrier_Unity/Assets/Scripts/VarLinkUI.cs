using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VarLinkUI : MonoBehaviour
{
   enum UIType
   {
      Undefined,
      Numeric,
      Toggle,
      Dropdown
   }
   //private UIType _uiType = UIType.Undefined;
   private Toggle _toggle;
   private InputField _input;
   private Dropdown _dropdown;
   private TMP_Text _tmpText;
   private Text     _text;
   public  VarName V;
   void Start()
    {
      _toggle = GetComponentInChildren<Toggle>();
      if (_toggle != null)
      {
         //_uiType = UIType.Toggle;
         _toggle.onValueChanged.AddListener((value) => onToggleValueChanged(_toggle, value));
      }

      _input = GetComponentInChildren<InputField>();
      if (_input != null)
      {
         //_uiType = UIType.Numeric;
         _input.onValueChanged.AddListener((value) => onInputValueChanged(_input, value));
      }

      _dropdown = GetComponentInChildren<Dropdown>();
      if (_dropdown != null)
      {
         //_uiType = UIType.Dropdown;
         _dropdown.onValueChanged.AddListener((value) => onDropdownValueChanged(_dropdown, value));
      }

      _text = GetComponent<Text>();
      _tmpText = GetComponent<TMP_Text>();

      updateUI();
      VarSync.OnVariableUpdate += onVariableUpdate;
   }

   private void onToggleValueChanged(Toggle _toggle, bool value)
   {
      VarSync.Set(V, value);
   }

   private void onInputValueChanged(InputField _input, string value)
   {
      VarSync.Set(V, float.Parse(value));
   }

   private void onDropdownValueChanged(Dropdown _dropdown, int value)
   {
      VarSync.Set(V, value);
   }

   private void updateUI()
   {
      string formatStr = $"F{V.GetFormatPrecision()}";
      string strVal = "";
      if (V.GetVarType() == VarType.Float)
         strVal = VarSync.GetFloat(V).ToString(formatStr);
      else if (V.GetVarType() == VarType.String)
         strVal = VarSync.GetString(V);

      if (_input != null)
         _input.text = strVal;
      else if (_toggle != null)
         _toggle.isOn = VarSync.GetBool(V);
      else if (_dropdown != null)
         _dropdown.value = VarSync.GetInt(V);
      else if (_tmpText != null)
         _tmpText.text = strVal;
      else if (_text != null)
         _text.text = strVal;

   }

   private void onVariableUpdate(VarName v, object value)
   {
      if (v == V)
         updateUI();

   }

}
