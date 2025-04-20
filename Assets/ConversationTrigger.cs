using UnityEngine;

public class ConversationTrigger : MonoBehaviour
{
    public ConversationUIManager uiManager;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            uiManager.EnterConversationMode();
        }
    }
}
