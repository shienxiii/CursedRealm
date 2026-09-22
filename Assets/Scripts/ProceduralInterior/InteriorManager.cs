using UnityEngine;

public class InteriorManager : MonoBehaviour
{
    private static InteriorManager _instance = null;
    public static InteriorManager Instance => _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this);
            return;
        }

        _instance = this;
    }

    
}
