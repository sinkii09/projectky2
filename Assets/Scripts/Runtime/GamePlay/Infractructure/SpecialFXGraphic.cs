using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialFXGraphic : MonoBehaviour
{
    [SerializeField]
    private float m_AutoShutdownTime = -1;
    private bool m_IsShutdown = false;

    private GameObject prefab;

    public void OnInitialized(GameObject prefab)
    {
        m_IsShutdown = false;
        this.prefab = prefab;
        StartCoroutine(CoroWaitForSelfDestruct());
    }
    void Shutdown()
    {

        if(!m_IsShutdown)
        {
            ParticlePool.Singleton.ReturnObject(gameObject,prefab);
            m_IsShutdown = true;
        }
    }
    private IEnumerator CoroWaitForSelfDestruct()
    {
        yield return new WaitForSeconds(m_AutoShutdownTime);
        //coroWaitForSelfDestruct = null;
        if (!m_IsShutdown)
        {
            Shutdown();
        }
    }
}
