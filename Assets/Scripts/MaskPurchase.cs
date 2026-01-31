using UnityEngine;

[DisallowMultipleComponent]
public class MaskPurchase : MonoBehaviour
{
    [SerializeField] private int refundAmount;

    public int RefundAmount => refundAmount;

    public void SetRefundAmount(int amount)
    {
        refundAmount = Mathf.Max(0, amount);
    }
}
