using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if WINDOWS_UWP
using UnityEngine.VR.WSA.Input;
#else
#endif

public class CameraCalibration : MonoBehaviour {
    public GameObject cameraContainer;

#if WINDOWS_UWP
    void Start() {
        InteractionManager.SourcePressed += onPressed;
    }

    void OnDestroy() {
        InteractionManager.SourcePressed -= onPressed;
    }

    void onPressed(InteractionSourceState state) {
        calibrate();
    }
#else
    void Update () {
        if (Input.GetKeyDown(KeyCode.C)) {
            calibrate();
        }
    }
#endif
    
    void calibrate() {
        if (cameraContainer != null) {
            cameraContainer.transform.position = -transform.localPosition;
        }
    }
}
