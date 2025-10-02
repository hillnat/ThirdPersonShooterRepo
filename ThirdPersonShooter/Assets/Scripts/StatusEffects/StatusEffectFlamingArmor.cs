using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffectFlamingArmor : StatusEffectBase
{
    public override float lifeTime => 6f;

    public override string statusEffectName => "Flaming Armor";

    public override string displayParticlesPath => "Particles/StatusEffects/StatusEffectParticles_FlamingArmor";

    public override EStatusEffects eStatusEffect => EStatusEffects.FlamingArmor;
}
