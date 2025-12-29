using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class Attack1 : MonoBehaviour
{
    Attack2 attack2;
    //Input
    Controls ctrls;
    InputAction attack;
    InputAction block;
    InputAction do360;
    InputAction frontflipAttack;
    InputAction backflip;
    InputAction skill;
    void Awake() { ctrls = new Controls(); }
    void OnEnable()
    {
        attack = ctrls.Player.Attack;
        attack.Enable();
        attack.performed += Attack;

        block = ctrls.Player.Block;
        block.Enable();

        do360 = ctrls.Player._360;
        do360.Enable();

        frontflipAttack = ctrls.Player.FrontflipAttack;
        frontflipAttack.Enable();

        backflip = ctrls.Player.BackFlip;
        backflip.Enable();

        skill = ctrls.Player.Skill;
        skill.Enable();
    }
    void OnDisable()
    { 
        attack.Disable();
        block.Disable();
        do360.Disable();
        frontflipAttack.Disable();
        backflip.Disable();
        skill.Disable();
    }

    void Start()
    {
        attack2 = GetComponentInChildren<Attack2>();
        attack2.m = GetComponent<Movement>(); 
        attack2.rb = GetComponent<Rigidbody>();
        attack2.animator = GetComponentInChildren<Animator>();
        attack2.playerTransform = transform;
    }

    void Update()
    {
        attack2.block = block.ReadValue<float>();
        attack2.do360 = do360.ReadValue<Vector2>();
        attack2.frontflipAttack = frontflipAttack.ReadValue<Vector2>();
        attack2.backflip = backflip.ReadValue<Vector2>();    

        attack2.defaultSpeed = attack2.m.defaultSpeed;
        attack2.jumpForce = attack2.m.jumpForce;
        attack2.onGround = attack2.m.onGround;
    }

    void Attack(InputAction.CallbackContext context)
    {
        attack2.Attack();
    }
}
