using System;
using UnityEngine;

public enum EStatusEffects { PhotonDecay, GravitonGrasp, FlamingArmor }


[Serializable]
public abstract class StatusEffectBase
{
    public static int GetRandomUniqueId()
    {
        return UnityEngine.Random.Range(0, int.MaxValue);
    }
    public static StatusEffectBase GetStatusEffectTypeFromEnum(EStatusEffects eStatusEffect)
    {
        switch (eStatusEffect)
        {
            case EStatusEffects.PhotonDecay:
                return new StatusEffectPhotonDecay();
                break;
            case EStatusEffects.GravitonGrasp:
                return new StatusEffectGravitonGrasp();
                break;
            case EStatusEffects.FlamingArmor:
                return new StatusEffectFlamingArmor();
                break;
            default:
                break;
        }
        return null;
    }
    public float startTime = 0f;

    public abstract float lifeTime { get; }
    public abstract string statusEffectName { get; }
    public abstract string displayParticlesPath { get; }
    public int displayParticlesUniqueID = int.MinValue;
    public abstract EStatusEffects eStatusEffect { get; }
    private GameObject[] displayParticles
    {
        get { if (_displayParticles==null) { _displayParticles = Resources.LoadAll<GameObject>(displayParticlesPath); } return _displayParticles; }
        set { _displayParticles = value; }
    }
    private GameObject[] _displayParticles = null;
    public GameObject GetRandomDisplayParticle()
    {
        if(displayParticles.Length==0){ return null;}
        return displayParticles[UnityEngine.Random.Range(0, displayParticles.Length)];
    }
}

