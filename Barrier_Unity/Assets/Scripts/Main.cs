using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewType
{
   Top,
   Iso,
   FollowObject,
   Free,
   Torpedo
};

public class Main : MonoBehaviour
{

   [SerializeField] GameObject _scenarioPanel;
   [SerializeField] GameObject _targetInfoPanel;
   [SerializeField] GameObject _mapPanel;
   [SerializeField] GameObject _settingsPanel;
 
   Toolbar[] m_toolbars;
   private CameraController m_cameraController;
   // Start is called before the first frame update
   void Start()
   {
      m_toolbars = FindObjectsOfType<Toolbar>();
      foreach (var tb in m_toolbars)
         tb.OnButtonClick += OnToolbarButtonClicked;
      m_cameraController = FindObjectOfType<CameraController>();

      showSectors(false);
   }

   public void OnToolbarButtonClicked(Toolbar tb, Toolbar.ButtonID id, bool bPressed)
   {
      switch (id)
      {
         case Toolbar.ButtonID.START: Scenario.Instance.StartScenario(); break;
         case Toolbar.ButtonID.STOP:  Scenario.Instance.StopScenario(); break;
         case Toolbar.ButtonID.PAUSE:
         {
            if (bPressed)
               Scenario.Instance.PauseScenario();
            else
               Scenario.Instance.ResumeScenario();
            break;
         }
         case Toolbar.ButtonID.VIEW_ISO: changeView(ViewType.Iso); break;
         case Toolbar.ButtonID.VIEW_TOP: changeView(ViewType.Top); break;
         case Toolbar.ButtonID.SECTORS: showSectors(bPressed); break;
         case Toolbar.ButtonID.EXIT:
         {
            Application.Quit();
            break;
         }
      }
   }

   // Update is called once per frame
   void Update()
   {
      updateToolbarButtons();
      updatePanels();
   }
   void updatePanels()
   {
      if (_scenarioPanel) _scenarioPanel.SetActive(Scenario.Instance.IsAlive);
      if (_targetInfoPanel) _targetInfoPanel.SetActive(Scenario.Instance.IsAlive);
      if (_mapPanel) _mapPanel.SetActive(Scenario.Instance.IsAlive);
      if (Scenario.Instance.IsAlive && _settingsPanel.activeInHierarchy)
         _settingsPanel.SetActive(false);
   }

   void updateToolbarButtons()
   {
      foreach (var tb in m_toolbars)
      {
         switch (Scenario.Instance.CurrentMode)
         {
            case Scenario.Mode.Running:
            {
               tb.EnableButton(Toolbar.ButtonID.PAUSE, true);
               tb.EnableButton(Toolbar.ButtonID.START, false);
               tb.EnableButton(Toolbar.ButtonID.STOP, true);
               tb.EnableButton(Toolbar.ButtonID.SETTINGS, false);
               break;
            }
            case Scenario.Mode.Paused:
            {
               tb.EnableButton(Toolbar.ButtonID.PAUSE, true);
               tb.EnableButton(Toolbar.ButtonID.START, false);
               tb.EnableButton(Toolbar.ButtonID.STOP, true);
               tb.EnableButton(Toolbar.ButtonID.SETTINGS, false);
               break;
            }
            case Scenario.Mode.Finished:
            case Scenario.Mode.Stoped:
            {
               tb.EnableButton(Toolbar.ButtonID.PAUSE, false);
               tb.EnableButton(Toolbar.ButtonID.START, true);
               tb.EnableButton(Toolbar.ButtonID.STOP, false);
               tb.EnableButton(Toolbar.ButtonID.SETTINGS, true);
               break;
            }
         }

      }
   }

      void changeView(ViewType viewType)
   {
      m_cameraController.ChangeView(viewType);
   }

   void showSectors(bool show)
   {
      GetComponent<SectorDrawer>().ShowSectors(show);
   }
}
