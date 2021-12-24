using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCameraSwitcher : MonoBehaviour
{
    public Cinemachine.CinemachineVirtualCamera[] vCameras;
    public Cinemachine.CinemachineBrain cBrain;
    private int cameraID = 0;
    // Start is called before the first frame update


    public void CameraSwitcher(int cameraID)
    {
        if (cameraID == 1)
        {
            MouseOrbitImproved mOrbit = vCameras[cameraID].GetComponent<MouseOrbitImproved>();
            if (mOrbit != null) mOrbit.InitCameraPosition();
        }
        vCameras[cameraID].MoveToTopOfPrioritySubqueue();
    }

}
