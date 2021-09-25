using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ToolbarButton : MonoBehaviour 
{

  public GameObject panel;
  public Toolbar.ButtonID  id;
  public bool pressedAvailable = true;
  public string TooltipText;

  private Text m_tooltip;
  Text  ButtonText;
  Image ButtonImg;

  bool m_bEnabled = true;
  bool m_bPressed = false;
  bool m_bHover = false;

  public bool Enabled
  {
    get { return m_bEnabled; }
    set 
    { 
      if ( m_bEnabled == value )
        return;
      m_bEnabled = value;
      updateButton();
    }
  }

  public bool Pressed
  {
    get { return m_bPressed; }
    set 
    { 
      if ( m_bPressed == value )
        return;
      m_bPressed = value;
      updateButton();
    }
  }

  Color pressedColor   = new Color (1.0f,0.75f,0.015f);
  Color hoverColor     = new Color (1.0f,0.85f,0.5f);
  Color disabledColor  = Color.gray;
  Color normalColor    = Color.white;

  void Start()
  {
    ButtonText= transform.Find("Text").GetComponent<Text>();
    ButtonImg = transform.Find("Image").GetComponent<Image>();

    EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
    addEventTrigger( trigger, hover, EventTriggerType.PointerEnter );
    addEventTrigger( trigger, hoverExit, EventTriggerType.PointerExit );

    //m_tooltip = GameObject.Find("Tooltip" ).GetComponent<Text>();
  }

  private void addEventTrigger(EventTrigger trigger,  UnityAction action, EventTriggerType triggerType)
  {
    // Create a nee TriggerEvent and add a listener
    EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
    triggerEvent.AddListener((eventData) => action()); // you can capture and pass the event data to the listener
    
    // Create and initialise EventTrigger.Entry using the created TriggerEvent
    EventTrigger.Entry entry = new EventTrigger.Entry() { callback = triggerEvent, eventID = triggerType };

    if ( trigger.triggers == null )
      trigger.triggers = new System.Collections.Generic.List<EventTrigger.Entry>();
    // Add the EventTrigger.Entry to delegates list on the EventTrigger
    trigger.triggers.Add(entry);
  }


  void hover ()
  {
    m_bHover = true;
    updateButton();
    if(m_tooltip != null)
      m_tooltip.text = TooltipText;
  }
  
  void hoverExit ()
  {
    m_bHover = false;
    updateButton();
   if (m_tooltip != null)
      m_tooltip.text = "";
  }
  void press ()
  {
    if (ButtonText!= null ) ButtonText.color = pressedColor;
    if (ButtonImg!= null )  ButtonImg.color = pressedColor;

    GetComponent<Image>().sprite = GetComponent<Button>().spriteState.pressedSprite;
  }

  void updateButton()
  {
    Color clr = normalColor;

    if ( m_bEnabled )
    {
      if ( m_bHover )
        clr = m_bPressed ? pressedColor  : hoverColor;
      else
        clr = m_bPressed ? pressedColor  : normalColor;
    }
    else
      clr = disabledColor;

    if (ButtonText != null ) ButtonText.color = clr;
    if (ButtonImg != null )  ButtonImg.color = clr;

    GetComponent<Button>().interactable = m_bEnabled;
    GetComponent<Image>().sprite = m_bPressed ? GetComponent<Button>().spriteState.pressedSprite : GetComponent<Button>().spriteState.highlightedSprite;
  }

}

