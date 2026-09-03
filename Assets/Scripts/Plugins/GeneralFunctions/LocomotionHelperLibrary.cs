using UnityEngine;

public static class LocomotionHelperLibrary
{
    public static Transform GetViewTransform()
    {
        return Camera.main.transform;
    }

    public static Vector3 GetViewForward(bool xzOnly = true)
    {
        Vector3 value = GetViewTransform().forward;
        if (xzOnly)
        {
            value.y = 0.0f;
            value.Normalize();
        }

        return value;
    }

    public static Vector3 GetViewRight(bool xzOnly = true)
    {
        Vector3 value = GetViewTransform().right;
        if (xzOnly)
        {
            value.y = 0.0f;
            value.Normalize();
        }

        return value;
    }
}
