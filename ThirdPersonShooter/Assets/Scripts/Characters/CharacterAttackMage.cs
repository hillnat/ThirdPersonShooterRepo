using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterAttackMage : PlayerControllerBase
{
    public override float moveSpeed => 2700f;

    public override float jumpForce => 1500f;

    public override int maxItems => 2;

    public override string characterNameInFile => "CharacterAttackMage";

    private void Awake()
    {
        List<WeaponBase> weapons = new List<WeaponBase>();
        weapons.Add(gameObject.AddComponent<WeaponDarkMatterSpellbook>());
        weapons.Add(gameObject.AddComponent<WeaponPhotonSpellbook>());
        allItems = weapons.ToArray();
        base.Awake();
    }

    void Start()
    {
        base.Start();
    }

    void Update()
    {
        base.Update();
    }
    private void FixedUpdate()
    {
        base.FixedUpdate();
    }
    private void LateUpdate()
    {
        //base.LateUpdate();
    }
}
