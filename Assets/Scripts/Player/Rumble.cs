using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.InputSystem;

public class Rumble : MonoBehaviour
{
    private static Gamepad pad;
    public static IEnumerator RumblePulse(float lowFreq, float highFreq, float duration)
    {
        if(IsGamepadActive() && PlayerPrefs.GetInt("gamepadRumble", 1) == 1)
        {
            pad = Gamepad.current;

            if(pad != null)
            {
                pad.SetMotorSpeeds(lowFreq, highFreq);
            }

            yield return new WaitForSecondsRealtime(duration);

            if(pad != null)
            {
                pad.SetMotorSpeeds(0, 0);
            }
        }
    }

    private static bool IsGamepadActive()
    {
        var playerInput = GameObject.FindObjectOfType<PlayerInput>();
        return playerInput != null && playerInput.currentControlScheme == "Gamepad";
    }
}
