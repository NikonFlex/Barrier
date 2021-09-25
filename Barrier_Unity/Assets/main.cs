using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour
{
   [SerializeField] Toolbar m_toolbar;
   // Start is called before the first frame update
   void Start()
    {
      if (m_toolbar != null)
         m_toolbar.OnButtonClick += OnToolbarButtonClicked;
    }

   public void OnToolbarButtonClicked(Toolbar.ButtonID id, bool bPressed)
   {
      print("Toolbar button clicked " + id);
      switch (id)
      {
//         case Toolbar.ButtonID.START: Instructor.StartExercise(); break;
//         case Toolbar.ButtonID.STOP: Instructor.StopExercise(); break;
         case Toolbar.ButtonID.PAUSE:
            {
               m_toolbar.EnableButton(Toolbar.ButtonID.PAUSE, false);
               m_toolbar.EnableButton(Toolbar.ButtonID.START, true);
//               Instructor.PauseExercise();
               break;
            }
         //case Toolbar.ButtonID.EXIT: Instructor.ShutDown(); break;
      }
   }

   // Update is called once per frame
   void Update()
    {
        
    }
}
