using System.Collections;
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
//      print("Toolbar button clicked " + id);
      switch (id)
      {
         case Toolbar.ButtonID.START:
         {
            tb.EnableButton(Toolbar.ButtonID.PAUSE, true);
            tb.EnableButton(Toolbar.ButtonID.START, false);
            tb.EnableButton(Toolbar.ButtonID.STOP, true);
            break;
         }
         case Toolbar.ButtonID.STOP:
         {
            tb.EnableButton(Toolbar.ButtonID.PAUSE, false);
            tb.EnableButton(Toolbar.ButtonID.START, true);
            break;
         }
         case Toolbar.ButtonID.PAUSE:
         {
            tb.EnableButton(Toolbar.ButtonID.PAUSE, false);
            tb.EnableButton(Toolbar.ButtonID.START, true);
            break;
         }
         case Toolbar.ButtonID.VIEW_ISO:
         {
            changeView(ViewType.Iso);
            break;
         }
         case Toolbar.ButtonID.VIEW_TOP:
         {
            changeView(ViewType.Top);
            break;
         }
         case Toolbar.ButtonID.SECTORS:
         {
            showSectors(bPressed);
            break;
         }
         //case Toolbar.ButtonID.EXIT: Instructor.ShutDown(); break;
      }
   }

   // Update is called once per frame
   void Update()
   {

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
