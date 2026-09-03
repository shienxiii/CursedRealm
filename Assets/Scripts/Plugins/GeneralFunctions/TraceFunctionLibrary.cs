using UnityEngine;

public static class TraceFunctionLibrary
{
    #region Capsule
    public static bool CapsuleTrace(Vector3 startPoint, Vector3 direction, float maxDistance, CapsuleCollider template, LayerMask ignoreLayer)
    {
        return CapsuleTrace(startPoint, direction, maxDistance, template.radius, template.height / 2.0f, ignoreLayer);
    }

    public static bool CapsuleTrace(Vector3 startPoint, Vector3 direction, float maxDistance, out RaycastHit outHit, CapsuleCollider template, LayerMask ignoreLayer)
    {
        return CapsuleTrace(startPoint, direction, maxDistance, out outHit, template.radius, template.height / 2.0f, ignoreLayer);
    }

    public static bool CapsuleTrace(Vector3 startPoint, Vector3 direction, float maxDistance, float radius, float halfHeight, LayerMask ignoreLayer)
    {
        Vector3 p1 = startPoint + (Vector3.up * (halfHeight - radius));
        Vector3 p2 = startPoint + (Vector3.down * (halfHeight - radius));

        return (Physics.CapsuleCast(p1, p2, radius, direction, maxDistance, ignoreLayer) || Physics.CheckCapsule(p1, p2, radius, ignoreLayer));
    }

    public static bool CapsuleTrace(Vector3 startPoint, Vector3 direction, float maxDistance, out RaycastHit outHit, float radius, float halfHeight, LayerMask ignoreLayer)
    {
        Vector3 p1 = startPoint + (Vector3.up * (halfHeight - radius));
        Vector3 p2 = startPoint + (Vector3.down * (halfHeight - radius));

        return (Physics.CapsuleCast(p1, p2, radius, direction, out outHit, maxDistance, ignoreLayer) || Physics.CheckCapsule(p1, p2, radius, ignoreLayer));
    }
    #endregion

    public static bool BoxTrace(Vector3 startPoint, Vector3 size, Vector3 direction, Quaternion orientation, float maxDistance, out RaycastHit[] outHit, LayerMask ignoreLayer)
    {
        outHit = Physics.BoxCastAll(startPoint, size / 2, direction, orientation, maxDistance, ignoreLayer);
        return outHit.Length > 0;
    }
}
