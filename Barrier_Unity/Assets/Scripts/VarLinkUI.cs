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
   public VarNameSelector V;
   private VarName _v => V.Name.ToVarName();
   
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
      VarSync.Set(_v, value);
   }

   private void onInputValueChanged(InputField _input, string value)
   {
      VarSync.Set(_v, float.Parse(value));
   }

   private void onDropdownValueChanged(Dropdown _dropdown, int value)
   {
      VarSync.Set(_v, value);
   }

   private void updateUI()
   {
      string formatStr = $"F{_v.GetFormatPrecision()}";
      string strVal = "";
      if (_v.GetVarType() == VarType.Float)
         strVal = VarSync.GetFloat(_v).ToString(formatStr);
      else if (_v.GetVarType() == VarType.String)
         strVal = VarSync.GetString(_v);

      if (_input != null)
         _input.text = strVal;
      else if (_toggle != null)
         _toggle.isOn = VarSync.GetBool(_v);
      else if (_dropdown != null)
         _dropdown.value = VarSync.GetInt(_v);
      else if (_tmpText != null)
         _tmpText.text = strVal;
      else if (_text != null)
         _text.text = strVal;
   }

   private void onVariableUpdate(VarName v, object value)
   {
      if (_v == v)
         updateUI();
   }

}
