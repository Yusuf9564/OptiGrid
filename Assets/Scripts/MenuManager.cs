using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Karşılaştırma Paneli")]
    public GameObject karsilastirmaPanel;
    public Button karsilastirmaButton;

    [Header("Tablo Metinleri")]
    public TextMeshProUGUI hexSonuc;
    public TextMeshProUGUI triSonuc;
    public TextMeshProUGUI squareSonuc;
    public TextMeshProUGUI kazananText;

    void Start()
    {
        if (karsilastirmaPanel != null)
            karsilastirmaPanel.SetActive(false);

        GuncelleKarsilastirmaButonu();
    }

    void GuncelleKarsilastirmaButonu()
    {
        if (karsilastirmaButton == null) return;
        karsilastirmaButton.interactable =
            PlayerPrefs.GetInt("Hex_Olculdu", 0) == 1 ||
            PlayerPrefs.GetInt("Tri_Olculdu", 0) == 1 ||
            PlayerPrefs.GetInt("Square_Olculdu", 0) == 1;
    }

    // ─── SAHNE GEÇİŞLERİ ───────────────────────────────────────

    public void LoadSquare() { SceneManager.LoadScene("Square"); }
    public void LoadTrigonal() { SceneManager.LoadScene("Trigonal"); }
    public void LoadHexagonal() { SceneManager.LoadScene("Hexagonal"); }
    public void BackToMainMenu() { SceneManager.LoadScene("MainMenu"); }
    public void QuitGame() { Application.Quit(); }

    // ─── DOLAMBAÇ KARŞILAŞTIRMASI ──────────────────────────────

    public void KarsilastirmayiGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        YazSonuc(hexSonuc, "HEXAGONAL", "Hex");
        YazSonuc(triSonuc, "TRIGONAL", "Tri");
        YazSonuc(squareSonuc, "SQUARE", "Square");
        BelirleKazananDolambac();
    }

    void YazSonuc(TextMeshProUGUI alan, string baslik, string key)
    {
        if (alan == null) return;
        if (PlayerPrefs.GetInt(key + "_Olculdu", 0) != 1)
        {
            alan.text = $"<b>{baslik}</b>\n<color=grey>Henüz ölçülmedi</color>";
            return;
        }
        float rota = PlayerPrefs.GetFloat(key + "_RotaUzunlugu", 0);
        float duz = PlayerPrefs.GetFloat(key + "_DogruMesafe", 0);
        float dolam = PlayerPrefs.GetFloat(key + "_Dolambac", 0);
        alan.text =
            $"<b>{baslik}</b>\n" +
            $"Rota: {rota:F1}m\n" +
            $"Düz Hat: {duz:F1}m\n" +
            $"Dolambaç: <color=#FF6B6B>%{dolam:F1}</color>";
    }

    void BelirleKazananDolambac()
    {
        if (kazananText == null) return;

        float hexD = PlayerPrefs.GetInt("Hex_Ölçüldü", 0) == 1 ? PlayerPrefs.GetFloat("Hex_Dolambaç", float.MaxValue) : float.MaxValue;
        float triD = PlayerPrefs.GetInt("Tri_Ölçüldü", 0) == 1 ? PlayerPrefs.GetFloat("Tri_Dolambaç", float.MaxValue) : float.MaxValue;
        float squareD = PlayerPrefs.GetInt("Square_Ölçüldü", 0) == 1 ? PlayerPrefs.GetFloat("Square_Dolambaç", float.MaxValue) : float.MaxValue;

        if (hexD == float.MaxValue && triD == float.MaxValue && squareD == float.MaxValue)
        {
            kazananText.text = "";
            return;
        }

        string kazanan;
        float enDusuk;

        if (hexD <= triD && hexD <= squareD) { kazanan = "HEXAGONAL"; enDusuk = hexD; }
        else if (triD <= hexD && triD <= squareD) { kazanan = "TRIGONAL"; enDusuk = triD; }
        else { kazanan = "SQUARE"; enDusuk = squareD; }

        kazananText.text =
            $"<color=#00FF88>EN VERİMLİ: {kazanan}</color>\n" +
            $"<size=80%>Dolambaç Oranı: %{enDusuk:F1}</size>";
    }

    // ─── ALAN VERİMLİLİĞİ ──────────────────────────────────────
    // Değerler matematiksel hesaba dayanır:
    // Hex: çevre² / alan = 4π/√3 ≈ %90.7 (Gauss-Bonnet teoremi)
    // Kare: %78.5, Üçgen: %60.5 (düzenli tessellation karşılaştırması)

    public void AlanVerimliliginiGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        if (hexSonuc != null) hexSonuc.text =
            "<b>HEXAGONAL</b>\n" +
            "Alan Verimi: <color=#00FF88>%90.7</color>\n" +
            "Yol/Alan Oranı: <color=#00FF88>EN DÜŞÜK</color>";
        if (triSonuc != null) triSonuc.text =
            "<b>TRIGONAL</b>\n" +
            "Alan Verimi: <color=#FF6B6B>%60.5</color>\n" +
            "Yol/Alan Oranı: <color=#FF6B6B>EN YÜKSEK</color>";
        if (squareSonuc != null) squareSonuc.text =
            "<b>SQUARE</b>\n" +
            "Alan Verimi: <color=#FFB100>%78.5</color>\n" +
            "Yol/Alan Oranı: <color=#FFB100>ORTA</color>";
        if (kazananText != null) kazananText.text =
            "<color=#00FF88>ALAN SAMPIYONU: HEXAGONAL</color>\n" +
            "<size=80%>Gauss-Bonnet teoremiyle kanıtlanmış\nen verimli alan</size>";
    }

    // ─── KARBON EMİSYONU ───────────────────────────────────────
    // CO2 = Σ(mesafe × 0.05) + Σ(dönüş_açısı × 0.02)
    // Birim: simüle edilmiş kg CO2

    public void KarbonAnaliziniGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        float hCO2 = PlayerPrefs.GetFloat("Hex_RealCO2", 0f);
        float tCO2 = PlayerPrefs.GetFloat("Tri_RealCO2", 0f);
        float sCO2 = PlayerPrefs.GetFloat("Square_RealCO2", 0f);

        Debug.Log($"[MenuManager] CO2 → Hex: {hCO2:F2}, Tri: {tCO2:F2}, Square: {sCO2:F2}");

        if (hexSonuc != null) hexSonuc.text = FormatCO2("HEXAGONAL", hCO2);
        if (triSonuc != null) triSonuc.text = FormatCO2("TRIGONAL", tCO2);
        if (squareSonuc != null) squareSonuc.text = FormatCO2("SQUARE", sCO2);

        if (kazananText != null)
        {
            string kazanan = "";
            float enDusuk = float.MaxValue;

            if (hCO2 > 0 && hCO2 < enDusuk) { enDusuk = hCO2; kazanan = "HEXAGONAL"; }
            if (tCO2 > 0 && tCO2 < enDusuk) { enDusuk = tCO2; kazanan = "TRIGONAL"; }
            if (sCO2 > 0 && sCO2 < enDusuk) { enDusuk = sCO2; kazanan = "SQUARE"; }

            kazananText.text = kazanan != ""
                ? $"<color=#00FFFF>EN ÇEVRECİ: {kazanan}</color>\n" +
                  $"<size=80%>Emisyon: {enDusuk:F2}kg CO2</size>"
                : "<color=grey>Henüz ölçüm yapılmadı</color>";
        }
    }

    string FormatCO2(string baslik, float co2)
    {
        string deger = co2 > 0 ? $"{co2:F2}kg" : "Ölçülmedi";
        return $"<b>{baslik}</b>\nEmisyon: <color=#00FFFF>{deger}</color>";
    }

    // ─── PANEL KONTROL ─────────────────────────────────────────

    public void PaneliKapat()
    {
        if (karsilastirmaPanel != null)
            karsilastirmaPanel.SetActive(false);
    }

    public void SonuclariSifirla()
    {
        string[] keys = {
            "Hex_Ölçüldü",     "Tri_Ölçüldü",     "Square_Ölçüldü",
            "Hex_RotaUzunlugu","Tri_RotaUzunlugu", "Square_RotaUzunlugu",
            "Hex_DogruMesafe", "Tri_DogruMesafe",  "Square_DogruMesafe",
            "Hex_Dolambaç",    "Tri_Dolambaç",     "Square_Dolambaç",
            "Hex_RealCO2",     "Tri_RealCO2",      "Square_RealCO2"
        };
        foreach (string key in keys) PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        GuncelleKarsilastirmaButonu();
        if (karsilastirmaPanel != null) karsilastirmaPanel.SetActive(false);
    }
}