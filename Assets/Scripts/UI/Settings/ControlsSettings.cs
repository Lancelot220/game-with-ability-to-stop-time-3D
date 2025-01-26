using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlsSettings : MonoBehaviour
{
    public Toggle gamepadRumble;

    void Start()
    {
        //GAMEPAD RUMBLE
        gamepadRumble.isOn = PlayerPrefs.GetInt("gamepadRumble", 1) == 1 ? true : false;
    }

    //GAMEPAD RUMBLE
    public void GamepadRumble(bool value)
    {
        PlayerPrefs.SetInt("gamepadRumble", value ? 1 : 0);
    }
}
