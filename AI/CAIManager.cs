using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class CAIManager : MonoBehaviour
{

    // Start is called before the first frame update
    public static List<CAI> m_List_AllAI                = new List<CAI>();
    public static List<CAI> m_List_ActiveAI             = new List<CAI>();
    public static List<CAI> m_List_Combat               = new List<CAI>();
    public static List<CAI> m_List_Investigate          = new List<CAI>();
    public float            m_fAIUpdateRange            = 100.0f;
    float                   m_fUpdateActiveAIFrequency  = 5.0f;  
    float                   m_fUpdateTimer              = 0.0f;
    //LayerMask               m_LayerMask;
    
    //AI calls it on spawn
    public static void AddAI_AllAIList( CAI AI )        {   m_List_AllAI.Add( AI );      }
    public static void AddAI_ActiveAIList( CAI AI )     {   m_List_ActiveAI.Add( AI );   }    

    //AI calls it on death
    public static void RemoveAI_BothLists( CAI AI )    
    {   
        m_List_AllAI.Remove( AI );
        if (m_List_ActiveAI.Contains( AI ))
        {
            m_List_ActiveAI.Remove( AI );
        }
    }

    public static void AddToCombatList(CAI AI)
    {
        if (m_List_Combat.Contains(AI) == false)
        {
            m_List_Combat.Add(AI);
            //CSoundBank.Instance.AIEnterCombat(AI.GetAIType(), AI.gameObject);
        }
    }

    public static void RemoveFromCombatList(CAI AI)
    {
        if(m_List_Combat.Contains(AI) == true)
            m_List_Combat.RemoveAt(m_List_Combat.LastIndexOf(AI));
    }

    public static void AddToInvestigateList(CAI AI)
    {
        if (m_List_Investigate.Contains(AI) == false)
        {
            m_List_Investigate.Add(AI);
            CSoundBank.Instance.AIAlerted(AI.GetAIType(), AI.gameObject);
        }
    }

    public static void RemoveFromInvestigateList(CAI AI)
    {
        if (m_List_Investigate.Contains(AI) == true)
            m_List_Investigate.RemoveAt(m_List_Investigate.LastIndexOf(AI));
    }

    public static void OnExit()
    {
        m_List_AllAI.Clear();
        m_List_ActiveAI.Clear();
        m_List_Combat.Clear();
        m_List_Investigate.Clear();
    }

    private void Start()
    {
        if (m_fAIUpdateRange < 80.0f)
        {
            m_fAIUpdateRange = 80.0f;
        }
        else if (m_fAIUpdateRange > 160.0f)
        {
            m_fAIUpdateRange = 160.0f;
        }
    }

    // Update is called once per frame
    void Update()
    {
        m_fUpdateTimer += Time.deltaTime;
        if ( m_fUpdateTimer > m_fUpdateActiveAIFrequency )
        {
            SetActiveAI();
            m_fUpdateTimer = 0;
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < m_List_ActiveAI.Count; ++i)
            {
                m_List_ActiveAI[i].GetComponent<CAIAttack>().Cleanup();
                Vector3 dir = m_List_ActiveAI[i].transform.position - CPlayerControlls.GetPlayer().transform.position;
                m_List_ActiveAI[i].GetComponent<CStatusEffects>().Knockback(dir, 1500);
                //m_ListActiveAI[i].GetComponent<CHealth>().TakeDamage(1000);
            }
            SetActiveAI();
        }
    }

    void MoveToCursor()
    {
        for (int i = 0; i < m_List_AllAI.Count; ++i)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                m_List_AllAI[i].GetComponent<NavMeshAgent>().SetDestination(hit.point);
            }
        }
    }

    public static CAI GetAIAtIndex_AllAI(int index)
    {
        if (m_List_AllAI.Count > index - 1)
        {
            return m_List_AllAI[index];
        }
        return null;
    }

    public static CAI GetAIAtIndex_ActiveAI( int index )
    {
        if (m_List_ActiveAI.Count > index - 1)
        {
            return m_List_ActiveAI[index];
        }
        return null;
    }
    
    void SetActiveAI()
    {
        m_List_ActiveAI.Clear();
        Vector3 playerPos = CPlayerControlls.GetPlayer().transform.position;
        for (int i = 0; i < m_List_AllAI.Count; i++)
        {
            if (Vector3.Distance( m_List_AllAI[i].transform.position, playerPos ) < m_fAIUpdateRange )
            {
               m_List_ActiveAI.Add(m_List_AllAI[i]);
               m_List_AllAI[i].AISetActive( true );
            }
            else 
            {
                m_List_AllAI[i].AISetActive( false );
            }
        }
    }

    
}
