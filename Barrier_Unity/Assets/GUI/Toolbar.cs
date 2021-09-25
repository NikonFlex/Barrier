using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Toolbar : MonoBehaviour
{
  public enum ButtonID
  {
    START,
    STOP,
    PAUSE,
    SETTINGS,
    EXIT
  };

  public delegate void ToolbarEventHandler(ButtonID id, bool pressed);
  public event ToolbarEventHandler OnButtonClick;
  public delegate void ToolbarPanelEventHandler(bool active);
  public event ToolbarPanelEventHandler OnToolbarPanel;

  private GameObject[] m_panels = new GameObject[0];
  private ToolbarButton m_currentButton;

  void Start()
  {
    List<GameObject> panelList = new List<GameObject>();
    foreach (ToolbarButton g in FindObjectsOfType<ToolbarButton>())
    {
      if (g.transform.root != transform.root)
        continue;
      Button b = g.GetComponent<Button>();
      if (b == null)
        continue;

      b.onClick.AddListener(() => onButtonClick(b));
      //print ( "toolbar add button " + b );
      if (g.panel != null)
        panelList.Add(g.panel);


    }

    m_panels = panelList.ToArray();

  }

  void onButtonClick(Button b)
  {
    ToolbarButton tbb = b.gameObject.GetComponent<ToolbarButton>();
    if (tbb == null)
      return;


    GameObject currentPanel = tbb.panel;
    if (currentPanel != null)
    {
      bool clickOnOwnPanel = false;
      foreach (GameObject panel in m_panels)
      {
        if (panel == currentPanel && panel.activeSelf)
          clickOnOwnPanel = true;
        panel.SetActive(false);
        if (OnToolbarPanel != null) OnToolbarPanel(false);
      }
      if (!clickOnOwnPanel)
      {
        currentPanel.SetActive(true);
        currentPanel.SendMessage("OnSetActive", SendMessageOptions.DontRequireReceiver);
        if (OnToolbarPanel != null) OnToolbarPanel(true);
      }
    }

    if (tbb.pressedAvailable)
    {
      if (tbb != m_currentButton)
      {
        tbb.Pressed = true;
        if (m_currentButton != null)
          m_currentButton.Pressed = false;
      }
      else
        tbb.Pressed = !tbb.Pressed;
    }
    else if (m_currentButton != null)
      m_currentButton.Pressed = false;

    m_currentButton = tbb;
    if (OnButtonClick != null)
      OnButtonClick(tbb.id, m_currentButton.Pressed);
  }

  public void EnableButton(ButtonID id, bool enabled)
  {
    for (int i = 0; i < transform.childCount; ++i)
    {
      ToolbarButton b = transform.GetChild(i).GetComponent<ToolbarButton>();
      if (b != null && b.id == id)
      {
        b.Enabled = enabled;
        return;
      }
        
    }
  }
  public void SetButtonText(ButtonID id, string text)
  {
    for (int i = 0; i < transform.childCount; ++i)
    {
      ToolbarButton b = transform.GetChild(i).GetComponent<ToolbarButton>();
      if (b.id == id)
      {
        b.GetComponentInChildren<Text>().text = text;
        return;
      }
    }
  }

  void onPanelFinished(GameObject panelObject)
  {
    foreach (ToolbarButton g in FindObjectsOfType<ToolbarButton>())
    {
      if (g.panel == panelObject)
      {
        g.Pressed = false;
        if (m_currentButton == g)
          m_currentButton = null;

      }
    }
    if (OnToolbarPanel != null) OnToolbarPanel(false);
  }

  static public void ClosePanel(GameObject panel)
  {
    Toolbar tb = FindObjectOfType<Toolbar>();
    if (tb != null)
      tb.onPanelFinished(panel);
    panel.SetActive(false);
  }

  static public void CloseAllPanels()
  {
    Toolbar tb = FindObjectOfType<Toolbar>();
    if (tb == null)
      return;
    foreach( GameObject panel in tb.m_panels )
    {
      tb.onPanelFinished(panel);
      panel.SetActive(false);
    }
  }
}
