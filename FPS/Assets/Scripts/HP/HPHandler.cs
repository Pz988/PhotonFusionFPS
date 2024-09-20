using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;
using UnityEngine.UI;

public class HPHandler : NetworkBehaviour
{
    [Networked(OnChanged = nameof(OnHPChanged))]

    byte HP { get; set; }

    [Networked(OnChanged = nameof(OnStateChanged))]

    public bool isDead { get; set; }

    bool isInitialized = false;

    const byte startingHP = 5;

    public Color uiOnHitColor;
    public Image uiOnHitImage;
    public MeshRenderer bodyMeshRenderer;
    Color defaultMeshBodyColor;

    private void Awake()
    {
        
    }

    // Start is called before the first frame update
    void Start()
    {
        HP = startingHP;
        isDead = false;

        defaultMeshBodyColor = bodyMeshRenderer.material.color;

        isInitialized = true;
    }

    IEnumerator OnHitCO()
    {
        bodyMeshRenderer.material.color = Color.white;

        if (Object.HasInputAuthority)
            uiOnHitImage.color = uiOnHitColor;

        yield return new WaitForSeconds(0.2f);

        bodyMeshRenderer.material.color = defaultMeshBodyColor;

        if(Object.HasInputAuthority && !isDead)
        {
            uiOnHitImage.color = new Color(0, 0, 0, 0);
        }
    }


    //Handled by Server to manage player damage
    public void OnTakeDamage()
    {
        if(isDead) //check is player is already dead, Only take damage while alive. 
            return; 

        HP -= 1;
        Debug.Log($"{Time.time} {transform.name} took damage, {HP} remaining");

        //Player has died/is dead
        if (HP <= 0)
        {
            Debug.Log($"{Time.time} {transform.name} has died");
            isDead = true;
        }
    }

    static void OnHPChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged value {changed.Behaviour.HP}");

        byte newHP = changed.Behaviour.HP;
        //load old value
        changed.LoadOld();
        byte oldHP = changed.Behaviour.HP;
        //check is hp increased or decreased
        if (newHP < oldHP)
        {
            changed.Behaviour.OnHPReduced();
        }

    }

    private void OnHPReduced()
    {
        if (!isInitialized)
            return;

        StartCoroutine(OnHitCO());
    }

    static void OnStateChanged(Changed<HPHandler> changed)
    {
        Debug.Log($"{Time.time} OnStateChanged isDead {changed.Behaviour.isDead}");
    }
}
       
    
