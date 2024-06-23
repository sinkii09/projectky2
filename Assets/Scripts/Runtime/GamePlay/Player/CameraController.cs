using Cinemachine;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private CinemachineVirtualCamera m_MainCamera;
    private void Start()
    {
        AttachCamera();
    }
    private void AttachCamera()
    {
        m_MainCamera = FindObjectOfType<CinemachineVirtualCamera>();
       
        if (m_MainCamera)
        {
            m_MainCamera.Follow = transform;
        }
    }
}
