#if UNITY_EDITOR
using ParrelSync;
#endif
using UnityEngine;

    public class EditorApplicationController : MonoBehaviour
    {
        public ApplicationController m_Controller;


        public void Start()
        {
#if UNITY_EDITOR

            if (ClonesManager.IsClone())
            {
                var argument = ClonesManager.GetArgument();
                if (argument == "server")
                    m_Controller.OnParrelSyncStarted(true, "server");
                else if (argument == "client")
                {
                    m_Controller.OnParrelSyncStarted(false, "client");
                }
            }
            else
                m_Controller.OnParrelSyncStarted(false, "client");
#endif
        }
    }
