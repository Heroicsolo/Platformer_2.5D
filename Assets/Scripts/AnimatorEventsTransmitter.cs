using UnityEngine;

public class AnimatorEventsTransmitter : MonoBehaviour
{
    private void OnAttackPerformed()
    {
        gameObject.SendMessageUpwards("PerformAttack");
    }
}
