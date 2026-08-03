using UnityEngine;
using UnityEngine.EventSystems;

public class EventSystemEnforcer : MonoBehaviour
{
    private void Update()
    {
        Enforce();
    }

    public static void Enforce()
    {
        EventSystem[] eventSystems = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (eventSystems.Length > 1)
        {
            // Keep the first one, destroy the rest
            for (int i = 1; i < eventSystems.Length; i++)
            {
                if (eventSystems[i] != null && eventSystems[i].gameObject != null)
                {
                    Debug.Log($"[EventSystemEnforcer] Destroying duplicate EventSystem on GameObject: {eventSystems[i].gameObject.name}");
                    Destroy(eventSystems[i].gameObject);
                }
            }
        }
    }
}
