using UnityEngine;

public class CrusherAnimationEvents : MonoBehaviour
{
    [SerializeField] private CrusherController crusher;

    public void OnCrushContact()
    {
        crusher.CrushBox();
    }
}