using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNoiseSystem : MonoBehaviour
{
    static LayerMask m_LayerMask;
    // Start is called before the first frame update
    void Start()
    {
        m_LayerMask = CLayers.GetLayerMask_Enemy();
    }

    public static void Noise(Vector3 noisePos, float noiseRadius)
    {
        bool hasEnemies = Physics.CheckSphere(noisePos, noiseRadius, m_LayerMask); //, QueryTriggerInteraction.Ignore
        if ( hasEnemies )
        {
            Collider[] AIColliders = Physics.OverlapSphere(noisePos, noiseRadius, m_LayerMask); //, QueryTriggerInteraction.Ignore
            for (int i = 0; i < AIColliders.Length; ++i)
            {
                if ( AIColliders[i].gameObject.activeInHierarchy == true )
                {
                    AIColliders[i].gameObject.GetComponent<CSenses>().Listen(noisePos);
                } 
            }
        }
    }

    public static void Alert(Vector3 originPos, float noiseRadius)
    {
        bool hasEnemies = Physics.CheckSphere(originPos, noiseRadius, m_LayerMask); //, QueryTriggerInteraction.Ignore
        if ( hasEnemies )
        {
            Collider[] AIColliders = Physics.OverlapSphere(originPos, noiseRadius, m_LayerMask); //, QueryTriggerInteraction.Ignore
            Vector3 PlayerPos = CPlayerControlls.GetPlayer().transform.position;
            for (int i = 0; i < AIColliders.Length; ++i)
            {
                if ( AIColliders[i].gameObject.activeInHierarchy == true )
                {
                    AIColliders[i].GetComponent<CSenses>().Listen(PlayerPos);
                    AIColliders[i].GetComponent<CAI>().SetCombatPreviousFrame(true); //avoid playing alert message
                } 
            }
        }
    }
}
