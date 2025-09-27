using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectGravitonGrasp : StatusEffectBase
{
    public override float lifeTime => 2.5f;

    public override string name => "Graviton Grasp";

    public override string displayParticlesPath => $"Particles/StatusEffects/StatusEffectParticles_GravitonGrasp";
}
