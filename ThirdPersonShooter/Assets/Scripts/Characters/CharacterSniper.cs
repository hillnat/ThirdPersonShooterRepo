using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterSniper : PlayerController
{
    public override float moveSpeed => 2000f;

    public override float jumpForce => 1500f;

    public override int maxItems => 2;

    public override string characterNameInFile => "CharacterSniper";

    private void Awake()
    {
        List<Weapon> weapons = new List<Weapon>();
        weapons.Add(gameObject.AddComponent<Sniper>());
        weapons.Add(gameObject.AddComponent<M1911>());
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
