using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHazardSlowing : CTriggerZone
{
    public float            m_fMoveSpeedMultiplier = 0.5f;

    public override void    OnTriggerEnter ( Collider other ) 
    {
        base.OnTriggerEnter( other );
        if ( Get_ListContainsGameObject(other.gameObject) == true)
        {
            other.gameObject.GetComponent<CStatusEffects>().SetMoveSpeedMultiplier( m_fMoveSpeedMultiplier );
        }
    }
    public override void    OnTriggerExit ( Collider other )
    {
        if ( Get_ListContainsGameObject(other.gameObject) == true )
        {
            other.gameObject.GetComponent<CStatusEffects>().ResetMoveSpeedMultiplier();
        }
        base.OnTriggerExit( other );
    }
}
