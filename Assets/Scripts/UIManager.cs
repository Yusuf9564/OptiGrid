using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class UIManager_OptiGrid : MonoBehaviour
{
    [Header("Ayarlar")]
    public Navigator activeNavigator; // Kod bunu otomatik bulacak

    [Header("Metin Alanları")]
    public TextMeshProUGUI woodText;
    public TextMeshProUGUI metalText;
    public TextMeshProUGUI benchText;
    public TextMeshProUGUI co2Text;
    public TextMeshProUGUI statusMessage;

    [Header("Görsel Barlar")]
    public Image woodFillBar;
    public Image metalFillBar;

    void Update()
    {
        // Sahnedeki aracı otomatik bulma (Dinamik)
        if (activeNavigator == null)
        {
            activeNavigator = Object.FindFirstObjectByType<Navigator>();
            if (activeNavigator == null)
            {
                if (statusMessage) statusMessage.text = "Araç Bekleniyor...";
                return;
            }
        }

        // Verileri Güncelle
        woodText.text = $"Odun: {activeNavigator.woodCount} / {Navigator.targetWood}";
        metalText.text = $"Metal: {activeNavigator.metalCount} / {Navigator.targetMetal}";
        benchText.text = $"Üretilen Bank: {activeNavigator.totalBenchesProduced}";

        if (co2Text) co2Text.text = $"Karbon Salınımı: {activeNavigator.GetCO2():F2} kg";

        // Barları Doldur
        if (woodFillBar) woodFillBar.fillAmount = (float)activeNavigator.woodCount / Navigator.targetWood;
        if (metalFillBar) metalFillBar.fillAmount = (float)activeNavigator.metalCount / Navigator.targetMetal;

        // Mesaj Sistemi
        if (activeNavigator.woodCount >= Navigator.targetWood && activeNavigator.metalCount >= Navigator.targetMetal)
        {
            statusMessage.text = "<color=green>HEDEF TAMAM! 'O' İLE ÜRET</color>";
            statusMessage.transform.localScale = Vector3.one * (1f + Mathf.PingPong(Time.time, 0.1f));
        }
        else
        {
            statusMessage.text = "Atık Toplanıyor...";
            statusMessage.transform.localScale = Vector3.one;
        }
    }
}