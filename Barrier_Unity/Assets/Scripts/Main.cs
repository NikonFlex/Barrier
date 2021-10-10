﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ViewType
{
   Top,
   Iso,
   Free
};

public class Main : MonoBehaviour
{

   [SerializeField] GameObject _scenarioPanel;
   [SerializeField] GameObject _targetInfoPanel;
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
      _scenarioPanel.SetActive(Scenario.Instance.IsAlive);
      _targetInfoPanel.SetActive(Scenario.Instance.IsAlive);
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
               break;
            }
            case Scenario.Mode.Paused:
            {
               tb.EnableButton(Toolbar.ButtonID.PAUSE, true);
               tb.EnableButton(Toolbar.ButtonID.START, true);
               tb.EnableButton(Toolbar.ButtonID.STOP, true);
               break;
            }
            case Scenario.Mode.Finished:
            case Scenario.Mode.Stoped:
            {
               tb.EnableButton(Toolbar.ButtonID.PAUSE, false);
               tb.EnableButton(Toolbar.ButtonID.START, true);
               tb.EnableButton(Toolbar.ButtonID.STOP, false);
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