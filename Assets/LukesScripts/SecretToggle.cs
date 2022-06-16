using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SecretToggle : MonoBehaviour
{
    public void ToggleSecret(bool state)
    {
        SuperSecret.secretEnabled = state;
    }
}
