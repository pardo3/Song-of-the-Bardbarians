using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CHazardDamage : CTriggerZone
{
    public float            m_fTimePerTick      = 1.0f;
    public int              m_iDamagePerTick    = 1;
    private float           m_fDamageTimer      = 0.0f;

    void Update()
    {
        if ( GetListCount() > 0 )
        {
            m_fDamageTimer += Time.deltaTime;
            if ( m_fDamageTimer > m_fTimePerTick )
            {
                for (int i = GetListCount(); i > 0; i--)
                {
                    int iActiveIndex = i - 1;
                    CHealth TargetRef = Get_ListGameObjectAtIndex(iActiveIndex).GetComponent<CHealth>();
                    CSoundBank.Instance.LavaTouch(TargetRef.gameObject);
                    TargetRef.TakeDamage( m_iDamagePerTick );
                }
                m_fDamageTimer = 0;
            }
        }
    }
}
