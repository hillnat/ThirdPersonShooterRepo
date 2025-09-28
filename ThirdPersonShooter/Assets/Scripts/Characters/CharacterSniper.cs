using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSniper : PlayerControllerBase
{
    public override float moveSpeed => 2300f;

    public override float jumpForce => 1300f;

    public override int maxItems => 2;

    public override string characterNameInFile => "CharacterSniper";

    private void Awake()
    {
        List<WeaponBase> weapons = new List<WeaponBase>();
        weapons.Add(gameObject.AddComponent<WeaponSniper>());
        weapons.Add(gameObject.AddComponent<WeaponShotgun>());
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
