using UnityEngine;
using UnityEngine.Events;

public class BoolInverter : MonoBehaviour
{
    public UnityEvent<bool> invertedEvent;

    public void InvertBool(bool value)
    {
        invertedEvent.Invoke(!value);
    }
}