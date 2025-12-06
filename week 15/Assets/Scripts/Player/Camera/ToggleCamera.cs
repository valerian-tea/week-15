using UnityEngine;
using Yarn.Unity;

public class ToggleCamera : MonoBehaviour
{
    public GameObject playerCamera;
    public GameObject cutsceneCamera;

    [YarnCommand("activate_player_camera")]
    public void ActivatePlayerCam()
    {
        cutsceneCamera.SetActive(false);
        playerCamera.SetActive(true);
    }

    [YarnCommand("activate_cutscene_camera")]
    public void ActivateCutsceneCam()
    {
        playerCamera.SetActive(false);
        cutsceneCamera.SetActive(true);
    }
}
