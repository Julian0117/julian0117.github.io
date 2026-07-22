using UnityEngine;

public class GravityAreaPlanet : GravityArea
{
    public override Vector3 GetGravityDirection(GravityBody gravityBody)
    {
        return (transform.position - gravityBody.transform.position).normalized;
    }
}