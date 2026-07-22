using UnityEngine;

public class PlayerParticleController : MonoBehaviour
{
    [Header("Particles")]
    [SerializeField] private ParticleSystem dashChargingLoopPS;
    [SerializeField] private ParticleSystem dashReadyLoopPS;
    [SerializeField] private ParticleSystem dashBurstPS;

    [Header("Speeds")]
    [SerializeField] private float readySpeed = 50f;
    [SerializeField] private float burstSpeed = 20f;


    public void PlayDashChargingLoop()
    {
        dashChargingLoopPS.Play();
    }

    public void PlayDashReadyLoop()
    {
        dashReadyLoopPS.Play();
    }

    public void UpdateDashReadyDirection(Vector3 camForward)
    {
        if (!dashReadyLoopPS.isPlaying) return;

        Vector3 dir = -camForward.normalized * readySpeed;
        var vel = dashReadyLoopPS.velocityOverLifetime;
        vel.x = new ParticleSystem.MinMaxCurve(dir.x);
        vel.y = new ParticleSystem.MinMaxCurve(dir.y);
        vel.z = new ParticleSystem.MinMaxCurve(dir.z);
    }

    public void StopDashParticlesLoop()
    {
        if (dashReadyLoopPS == null) return;
        dashReadyLoopPS.Stop();
        //dashReadyLoopPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (dashChargingLoopPS == null) return;
        dashChargingLoopPS.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    public void PlayDashParticlesBurst(Vector3 camForward)
    {
        Vector3 dir = -camForward.normalized * burstSpeed;

        var main = dashBurstPS.main;
        main.loop = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var vel = dashBurstPS.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.World;
        vel.x = new ParticleSystem.MinMaxCurve(dir.x);
        vel.y = new ParticleSystem.MinMaxCurve(dir.y);
        vel.z = new ParticleSystem.MinMaxCurve(dir.z);

        dashBurstPS.Play();
    }
}