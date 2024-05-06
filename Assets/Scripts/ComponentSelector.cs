using UnityEngine;

public class ComponentSelector : MonoBehaviour
{
    public GameObject ECS_frontend;
    public GameObject Mirror_frontend;

    public GameObject NFGO_frontend;
    public GameObject NFGO_NetworkManeger;

    public enum DebugComponent
    {
        ECS,
        Mirror,
        NFGO_frontend, 
        off

    }

    public DebugComponent debugComponentSelection = DebugComponent.ECS; // Set default value here

    void Start()
    {
        if (!Application.isEditor)
        {
            // Check for command line arguments
            string[] args = System.Environment.GetCommandLineArgs();
            foreach (string arg in args)
            {   
                if (arg == "-ecs")
                {
                    SetComponentSelection(DebugComponent.ECS);
                    return; // Exit the loop if a valid argument is found
                }
                else if (arg == "-mirror")
                {
                    SetComponentSelection(DebugComponent.Mirror);
                    return; // Exit the loop if a valid argument is found
                }
                else if (arg == "-nfgo")
                {
                    SetComponentSelection(DebugComponent.NFGO_frontend);
                    return; // Exit the loop if a valid argument is found
                }
                
            }
        }
        
        // Default behavior if no command line argument is provided or if it's in the Unity Editor
        SetComponentSelection(debugComponentSelection);
    }

    void SetComponentSelection(DebugComponent selection)
    {
        switch (selection)
        {
            case DebugComponent.ECS:
                ECS_frontend.SetActive(true);
                Mirror_frontend.SetActive(false);
                NFGO_frontend.SetActive(false);
                NFGO_NetworkManeger.SetActive(false);
                break;
            case DebugComponent.Mirror:
                ECS_frontend.SetActive(false);
                Mirror_frontend.SetActive(true);
                NFGO_frontend.SetActive(false);
                NFGO_NetworkManeger.SetActive(false);
                break;
            
            case DebugComponent.NFGO_frontend:
                ECS_frontend.SetActive(false);
                Mirror_frontend.SetActive(false);
                NFGO_frontend.SetActive(true);  
                NFGO_NetworkManeger.SetActive(true);
                break;
            case DebugComponent.off:
                ECS_frontend.SetActive(false);
                Mirror_frontend.SetActive(false);
                NFGO_frontend.SetActive(false);
                NFGO_NetworkManeger.SetActive(false);
                break;
        }
    }
}
