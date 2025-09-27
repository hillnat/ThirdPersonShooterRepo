using System;

[Serializable]
public class StatusEffectPhotonDecay : StatusEffectBase
{
    public override float lifeTime => 2.5f;

    public override string name => "Photon Decay";
    public override string displayParticlesPath => $"Particles/StatusEffects/StatusEffectParticles_PhotonDecay";
}