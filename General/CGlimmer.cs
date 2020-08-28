using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGlimmer : MonoBehaviour
{
    //TODO flash duration fix
    [SerializeField] List<GameObject>               m_ListGlimmerParticleGO     = new List<GameObject>();
    Material                                        m_CharacterMaterialOriginal = null;
    Material                                        m_WeaponMaterialOriginal    = null;
    [SerializeField] Material                       m_CharacterMaterialGlimmer  = null;     
    [SerializeField] Material                       m_WeaponMaterialGlimmer     = null;
    [SerializeField] Material                       m_MaterialOnDamageTaken     = null;

    [SerializeField] SkinnedMeshRenderer            m_CharacterMeshRenderer     = null;     //For on damage taken effect
    [SerializeField] MeshRenderer                   m_WeaponMeshRenderer        = null;     //For weapon glimmer or statues

    bool    m_bWillFlashOnDamageTaken   = false;    //Is dependent on skinned mesh renderer not being null
    bool    m_bDamageTakenFlashing      = false;    //Is currently flashing
    float   m_fDamageFlashTimer         = 0.0f;
    float   m_fDamageFlashDuration      = 0.0f;

    bool    m_bHasGlimmerMaterials      = false;
    bool    m_bHasGlimmerParticles      = false;
    bool    m_bCanGlimmer               = false;

    bool                GetCanGlimmer()             {   return m_bCanGlimmer;           }
    SkinnedMeshRenderer GetCharacterMeshRenderer()  {   return m_CharacterMeshRenderer; }
    MeshRenderer        GetWeaponMeshRenderer()     {   return m_WeaponMeshRenderer;    }



    void Start()
    {

        if ( GetCharacterMeshRenderer() != null )
        {
            m_CharacterMaterialOriginal = GetCharacterMeshRenderer().material;
            if ( m_CharacterMaterialOriginal != null && m_CharacterMaterialGlimmer != null ) 
            {
                m_bHasGlimmerMaterials = true;
            }
            if ( m_CharacterMaterialOriginal != null && m_MaterialOnDamageTaken != null )
            {
                m_bWillFlashOnDamageTaken = true;
            }
        }
        if ( GetWeaponMeshRenderer() != null )
        {
            m_WeaponMaterialOriginal = GetWeaponMeshRenderer().material;
            if ( m_WeaponMaterialOriginal != null && m_WeaponMaterialGlimmer != null ) 
            {
                m_bHasGlimmerMaterials = true;
            }
        }

        if ( m_ListGlimmerParticleGO.Count > 0 )
        {
            m_bHasGlimmerParticles = true;
        }

        if (m_bHasGlimmerParticles || m_bHasGlimmerMaterials)
        {
            m_bCanGlimmer = true;
        }
    }

    private void Update()
    {
        if ( m_bDamageTakenFlashing == true )
        {
            m_fDamageFlashTimer += Time.deltaTime;
            if (m_fDamageFlashTimer > m_fDamageFlashDuration )
            {
                DisableDamageTakenEffect();
            }
        }
    }

    public void OnDamageTakenFlash( float fFlashDuration )
    {
        if ( m_bWillFlashOnDamageTaken == true )
        {
            m_fDamageFlashDuration = fFlashDuration;
            EnableDamageTakenEffect();
        }
    }

    public void StartGlimmerAfterDuration( float fStartGlimmerAfterSeconds, float fGlimmerDuration )
    {   
        if( GetCanGlimmer() == true )
        {
            CancelInvoke();
            Invoke( "EnableGlimmerEffect", fStartGlimmerAfterSeconds );
            Invoke( "DisableGlimmerEffect", fStartGlimmerAfterSeconds + fGlimmerDuration );
        }
    }

    public void StartGlimmerImmediately( float fGlimmerDuration = 0.0f )
    {   
        if( GetCanGlimmer() == true )
        {
            CancelInvoke();
            EnableGlimmerEffect();
            if (fGlimmerDuration > 0.0f)
            {
                Invoke( "DisableGlimmerEffect",  fGlimmerDuration );
            }
        }
    }

    void EnableGlimmerEffect()
    {
        for ( int i = 0; i < m_ListGlimmerParticleGO.Count; ++i )
        {
            if ( m_ListGlimmerParticleGO[i] != null )
            {
                m_ListGlimmerParticleGO[i].SetActive(true);
            }
        }
        //additional effects will also be activated here
        
        if ( GetWeaponMeshRenderer() != null && m_bHasGlimmerMaterials == true )
        {

            GetWeaponMeshRenderer().material = m_WeaponMaterialGlimmer;
        }
        else if ( GetCharacterMeshRenderer() != null && m_bHasGlimmerMaterials == true )
        {
            GetCharacterMeshRenderer().material = m_CharacterMaterialGlimmer;
        }

    }

    public void DisableGlimmerEffect()
    {
        for ( int i = 0; i < m_ListGlimmerParticleGO.Count; ++i )
        {
            if ( m_ListGlimmerParticleGO[i] != null )
            {
                m_ListGlimmerParticleGO[i].SetActive(false);
            }
        }
        if ( m_bHasGlimmerMaterials == true )
        {
            if ( GetWeaponMeshRenderer() != null )
            {
                GetWeaponMeshRenderer().material = m_WeaponMaterialOriginal;
            }
            else if ( GetCharacterMeshRenderer() != null )
            {
                GetCharacterMeshRenderer().material = m_CharacterMaterialOriginal;
            }
        }
    }

    void EnableDamageTakenEffect()
    {
        if ( GetCharacterMeshRenderer() != null )
        {
            GetCharacterMeshRenderer().material = m_MaterialOnDamageTaken;
            m_bDamageTakenFlashing = true;
        }
    }

    void DisableDamageTakenEffect()
    {
        if ( GetCharacterMeshRenderer() != null )
        {
            GetCharacterMeshRenderer().material = m_CharacterMaterialOriginal;
            m_bDamageTakenFlashing = false;
            m_fDamageFlashTimer = 0.0f;
        }
    }

    private void OnDestroy()
    {
        CancelInvoke();
    }
}
