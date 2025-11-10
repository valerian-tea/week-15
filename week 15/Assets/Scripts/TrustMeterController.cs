using UnityEngine;
using UnityEngine.UI;

public class TrustMeterController : MonoBehaviour
{
    public RectTransform trustMeterFill;
    public float maxTrust = 100f;
    private float currentTrust;
    private float originalWidth;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentTrust = maxTrust;
        originalWidth = trustMeterFill.sizeDelta.x;
        UpdateTrustMeter();
    }

    public void GainTrust()
    {
        currentTrust = Mathf.Min(currentTrust + 10f, maxTrust);
        UpdateTrustMeter();
    }

    public void LoseTrust()
    {
        currentTrust = Mathf.Max(currentTrust - 10f, 0f);
        UpdateTrustMeter();
    }

    // Update is called once per frame
    void UpdateTrustMeter()
    {
        float fillAmount = currentTrust / maxTrust;
        trustMeterFill.sizeDelta = new Vector2(originalWidth * fillAmount, trustMeterFill.sizeDelta.y);
    }
}
