using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Karsilastirma Paneli")]
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

        if (karsilastirmaButton != null)
        {
            bool herhangiBiri =
                PlayerPrefs.GetInt("Hex_Olculdu", 0) == 1 ||
                PlayerPrefs.GetInt("Tri_Olculdu", 0) == 1 ||
                PlayerPrefs.GetInt("Square_Olculdu", 0) == 1;
            karsilastirmaButton.interactable = herhangiBiri;
        }
    }

    // ─── SAHNE GEÇİŞLERİ ───────────────────────────────────────

    public void LoadSquare() { SceneManager.LoadScene("Square"); }
    public void LoadTrigonal() { SceneManager.LoadScene("Trigonal"); }
    public void LoadHexagonal() { SceneManager.LoadScene("Hexagonal"); }
    public void BackToMainMenu() { SceneManager.LoadScene("MainMenu"); }
    public void QuitGame() { Application.Quit(); }

    // ─── DOLAMBAC KARSILASTIRMASI ───────────────────────────────

    public void KarsilastirmayiGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        if (hexSonuc != null)
        {
            if (PlayerPrefs.GetInt("Hex_Olculdu", 0) == 1)
            {
                float rota = PlayerPrefs.GetFloat("Hex_RotaUzunlugu", 0);
                float duz = PlayerPrefs.GetFloat("Hex_DogruMesafe", 0);
                float dolam = PlayerPrefs.GetFloat("Hex_Dolambac", 0);
                hexSonuc.text =
                    $"<b>HEXAGONAL</b>\n" +
                    $"Rota: {rota:F1}m\n" +
                    $"Duz Hat: {duz:F1}m\n" +
                    $"Dolambac: <color=#FF6B6B>%{dolam:F1}</color>";
            }
            else hexSonuc.text = "<b>HEXAGONAL</b>\n<color=grey>Henuz olculmedi</color>";
        }

        if (triSonuc != null)
        {
            if (PlayerPrefs.GetInt("Tri_Olculdu", 0) == 1)
            {
                float rota = PlayerPrefs.GetFloat("Tri_RotaUzunlugu", 0);
                float duz = PlayerPrefs.GetFloat("Tri_DogruMesafe", 0);
                float dolam = PlayerPrefs.GetFloat("Tri_Dolambac", 0);
                triSonuc.text =
                    $"<b>TRIGONAL</b>\n" +
                    $"Rota: {rota:F1}m\n" +
                    $"Duz Hat: {duz:F1}m\n" +
                    $"Dolambac: <color=#FF6B6B>%{dolam:F1}</color>";
            }
            else triSonuc.text = "<b>TRIGONAL</b>\n<color=grey>Henuz olculmedi</color>";
        }

        if (squareSonuc != null)
        {
            if (PlayerPrefs.GetInt("Square_Olculdu", 0) == 1)
            {
                float rota = PlayerPrefs.GetFloat("Square_RotaUzunlugu", 0);
                float duz = PlayerPrefs.GetFloat("Square_DogruMesafe", 0);
                float dolam = PlayerPrefs.GetFloat("Square_Dolambac", 0);
                squareSonuc.text =
                    $"<b>SQUARE</b>\n" +
                    $"Rota: {rota:F1}m\n" +
                    $"Duz Hat: {duz:F1}m\n" +
                    $"Dolambac: <color=#FF6B6B>%{dolam:F1}</color>";
            }
            else squareSonuc.text = "<b>SQUARE</b>\n<color=grey>Henuz olculmedi</color>";
        }

        BelirleKazananDolambac();
    }

    void BelirleKazananDolambac()
    {
        if (kazananText == null) return;

        float hexD = PlayerPrefs.GetInt("Hex_Olculdu", 0) == 1 ? PlayerPrefs.GetFloat("Hex_Dolambac", float.MaxValue) : float.MaxValue;
        float triD = PlayerPrefs.GetInt("Tri_Olculdu", 0) == 1 ? PlayerPrefs.GetFloat("Tri_Dolambac", float.MaxValue) : float.MaxValue;
        float squareD = PlayerPrefs.GetInt("Square_Olculdu", 0) == 1 ? PlayerPrefs.GetFloat("Square_Dolambac", float.MaxValue) : float.MaxValue;

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
            $"<color=#00FF88>EN VERIMLI: {kazanan}</color>\n" +
            $"<size=80%>Dolambaç Oranı: %{enDusuk:F1}</size>";
    }

    // ─── ALAN VERİMLİLİĞİ ──────────────────────────────────────

    public void AlanVerimliliginiGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        if (hexSonuc != null) hexSonuc.text = "<b>HEXAGONAL</b>\nAlan Verimi: <color=#00FF88>%90+</color>\nYol Tasarrufu: <color=#00FF88>MAX</color>";
        if (triSonuc != null) triSonuc.text = "<b>TRIGONAL</b>\nAlan Verimi: <color=#FF6B6B>%47</color>\nYol Tasarrufu: <color=#FF6B6B>MIN</color>";
        if (squareSonuc != null) squareSonuc.text = "<b>SQUARE</b>\nAlan Verimi: <color=#FFB100>%64</color>\nYol Tasarrufu: <color=#FFB100>ORTA</color>";
        if (kazananText != null) kazananText.text = "<color=#00FF88>ALAN ŞAMPİYONU: HEXAGONAL</color>\n<size=80%>Matematiksel olarak kanıtlanmış en verimli alan</size>";
    }

    // ─── KARBON EMİSYONU ───────────────────────────────────────

    public void KarbonAnaliziniGoster()
    {
        if (karsilastirmaPanel == null) return;
        karsilastirmaPanel.SetActive(true);

        float hCO2 = PlayerPrefs.GetFloat("Hex_RealCO2", 0f);
        float tCO2 = PlayerPrefs.GetFloat("Tri_RealCO2", 0f);
        float sCO2 = PlayerPrefs.GetFloat("Square_RealCO2", 0f);

        Debug.Log($"Karbon Okuma -> Hex: {hCO2:F2}, Tri: {tCO2:F2}, Square: {sCO2:F2}");

        if (hexSonuc != null) hexSonuc.text = $"<b>HEXAGONAL</b>\nEmisyon: <color=#00FFFF>{(hCO2 > 0 ? hCO2.ToString("F2") + "kg" : "Ölçülmedi")}</color>";
        if (triSonuc != null) triSonuc.text = $"<b>TRIGONAL</b>\nEmisyon: <color=#00FFFF>{(tCO2 > 0 ? tCO2.ToString("F2") + "kg" : "Ölçülmedi")}</color>";
        if (squareSonuc != null) squareSonuc.text = $"<b>SQUARE</b>\nEmisyon: <color=#00FFFF>{(sCO2 > 0 ? sCO2.ToString("F2") + "kg" : "Ölçülmedi")}</color>";

        if (kazananText != null)
        {
            string kazanan = "";
            float enDusuk = float.MaxValue;

            if (hCO2 > 0 && hCO2 < enDusuk) { enDusuk = hCO2; kazanan = "HEXAGONAL"; }
            if (tCO2 > 0 && tCO2 < enDusuk) { enDusuk = tCO2; kazanan = "TRIGONAL"; }
            if (sCO2 > 0 && sCO2 < enDusuk) { enDusuk = sCO2; kazanan = "SQUARE"; }

            kazananText.text = kazanan != ""
                ? $"<color=#00FFFF>EN CEVRECI: {kazanan}</color>\n<size=80%>Emisyon: {enDusuk:F2}kg</size>"
                : "<color=grey>Henüz Ölçülme Yapılmadı</color>";
        }
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
            "Hex_Olculdu",    "Tri_Olculdu",    "Square_Olculdu",
            "Hex_RotaUzunlugu","Tri_RotaUzunlugu","Square_RotaUzunlugu",
            "Hex_DogruMesafe", "Tri_DogruMesafe", "Square_DogruMesafe",
            "Hex_Dolambac",    "Tri_Dolambac",    "Square_Dolambac",
            "Hex_RealCO2",     "Tri_RealCO2",     "Square_RealCO2"
        };
        foreach (string key in keys) PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();

        if (karsilastirmaButton != null) karsilastirmaButton.interactable = false;
        if (karsilastirmaPanel != null) karsilastirmaPanel.SetActive(false);
    }
}