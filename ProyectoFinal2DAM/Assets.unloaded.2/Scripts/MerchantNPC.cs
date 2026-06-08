using UnityEngine;

public class MerchantNPC : MonoBehaviour
{
    public GameObject promptUI;
    public ShopUI shopUI;
    private bool playerInRange = false;

    void Start()
    {
        FindReferences();

        // Initial state
        if (promptUI != null) promptUI.SetActive(false);
        if (shopUI != null) shopUI.gameObject.SetActive(false);
    }

    private void FindReferences()
    {
        if (shopUI == null || promptUI == null)
        {
            // Search all loaded scenes
            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;

                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root.name == "UI_Canvas")
                    {
                        if (shopUI == null)
                        {
                            Transform t = root.transform.Find("ShopUI_Panel");
                            if (t != null) shopUI = t.GetComponent<ShopUI>();
                        }

                        if (promptUI == null)
                        {
                            Transform t = root.transform.Find("CheckpointPrompt");
                            if (t != null) promptUI = t.gameObject;
                        }
                    }
                    
                    if (shopUI == null && root.name == "ShopUI_Panel")
                    {
                        shopUI = root.GetComponent<ShopUI>();
                    }
                }
            }
        }
    }

    void Update()
    {
        if (playerInRange)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                Debug.Log("[MerchantNPC] 'T' pressed.");
                ToggleShop();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        if (isPlayer)
        {
            playerInRange = true;
            if (promptUI != null) 
            {
                promptUI.SetActive(true);
                Vector3 localScale = promptUI.transform.localScale;
                localScale.x = Mathf.Abs(localScale.x);
                promptUI.transform.localScale = localScale;
            }
            Debug.Log("[MerchantNPC] Player entered range via trigger.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool isPlayer = other.CompareTag("Player") || (other.attachedRigidbody != null && other.attachedRigidbody.CompareTag("Player"));
        
        if (isPlayer)
        {
            playerInRange = false;
            if (promptUI != null) promptUI.SetActive(false);
            
            if (shopUI != null && shopUI.gameObject.activeSelf) 
            {
                shopUI.gameObject.SetActive(false);
                Time.timeScale = 1f;
            }
            Debug.Log("[MerchantNPC] Player left range via trigger.");
        }
    }

    void ToggleShop()
    {
        if (shopUI == null) FindReferences();

        if (shopUI != null)
        {
            bool isActive = !shopUI.gameObject.activeSelf;
            shopUI.gameObject.SetActive(isActive);
            Debug.Log($"[MerchantNPC] Shop Panel toggled to: {isActive}");
            
            if (isActive)
            {
                shopUI.transform.SetAsLastSibling();
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = 1f;
            }
        }
        else
        {
            Debug.LogError("[MerchantNPC] Shop UI reference is missing!");
        }
    }
}
