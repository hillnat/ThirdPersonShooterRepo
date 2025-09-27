using System;
using UnityEngine;
[Serializable]
public abstract class StatusEffectBase
{
    public enum EStatusEffects { PhotonDecay, GravitonGrasp }

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
            default:
                break;
        }
        return null;
    }
    public float startTime = 0f;

    public abstract float lifeTime { get; }
    public abstract string name { get; }
    public abstract string displayParticlesPath { get; }
    public int displayParticlesUniqueID = int.MinValue;
    private GameObject[] displayParticles
    {
        get { if (_displayParticles==null) { _displayParticles = Resources.LoadAll<GameObject>(displayParticlesPath); } return _displayParticles; }
        set { _displayParticles = value; }
    }
    private GameObject[] _displayParticles = null;
    public GameObject GetRandomDisplayParticle()
    {
        return displayParticles[UnityEngine.Random.Range(0, displayParticles.Length)];
    }
}

