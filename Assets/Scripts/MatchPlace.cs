using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FruitContainer : MonoBehaviour
{
    private List<GameObject> _fruitsInContainer = new List<GameObject>(); // Kabýn içindeki nesneleri saklayan liste
    private int _score = 0; // Toplam puan
    private Vector3 fixedPosition = new Vector3(-0.1f, 0.11f, 0.3f); // Sabitlenecek nokta
    private Animator childAnimator;

    [SerializeField] private TMP_Text scoreText; // Puan göstermek için TextMesh Pro Text
    [SerializeField] private GameObject zSkillUI; // Canvas içindeki Z image referansý
    private bool isZActive = false; // Z skilli aktif mi?
    private bool canUseZ = true; // Z skilli kullanýlabilir mi?
    private float zMultiplier = 1; // Z skilli çarpaný (varsayýlan 1)

    private void Start()
    {
        Transform child = transform.Find("ChestV1_Top");
        if (child != null)
        {
            childAnimator = child.GetComponent<Animator>();
        }
        else
        {
            Debug.LogError("Child not found!");
        }

        // Baþlangýç puanýný UI'ya yaz
        UpdateScoreUI();

        // Z Skill UI baþlangýçta aktif veya pasif; projede istediðin duruma göre ayarla
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(true);
        }
    }

    private void Update()
    {
        // Z skilli aktif deðilse ve kullanýlabilir durumdaysa
        if (Input.GetKeyDown(KeyCode.Z) && canUseZ)
        {
            StartCoroutine(ActivateZSkill());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Eðer meyve ise listeye ekle ve kontrol et
        if (other.CompareTag("Moveable"))
        {
            // Nesneyi sabit pozisyona taþý
            other.transform.position = fixedPosition;
            Debug.Log($"'{other.name}' kabýn içine girdi ve konumu sabitlendi: {fixedPosition}");

            // Fiziði durdur
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            // Nesneyi listeye ekle ve kontrol et
            _fruitsInContainer.Add(other.gameObject);
            Debug.Log($"Sepete '{other.name}' eklendi.");
            CheckFruits(); // Meyve isimlerini kontrol et
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Meyve kabý terk ederse listeden çýkar
        if (other.CompareTag("Moveable"))
        {
            _fruitsInContainer.Remove(other.gameObject);
            Debug.Log($"'{other.name}' sepetten çýkarýldý.");
        }
    }

    private void CheckFruits()
    {
        // Eðer kapta 2 veya daha fazla nesne varsa kontrol et
        if (_fruitsInContainer.Count >= 2)
        {
            GameObject fruit1 = _fruitsInContainer[0];
            GameObject fruit2 = _fruitsInContainer[1];

            // Ýsimlerin baþ harflerini al
            char firstChar1 = fruit1.name[0];
            char firstChar2 = fruit2.name[0];

            if (firstChar1 == firstChar2)
            {
                // Baþ harfleri ayný ise puan ekle ve nesneleri yok et
                int points = GetPoints(firstChar1);
                childAnimator.SetTrigger("Close");
                _score += Mathf.RoundToInt(points * zMultiplier); // Çarpaný uygula
                UpdateScoreUI(); // UI'da puaný güncelle
                Debug.Log($"Baþ harfler ayný ('{fruit1.name}', '{fruit2.name}')! Puan: +{points}. Toplam puan: {_score}");
                Destroy(fruit1);
                Destroy(fruit2);
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
            else
            {
                // Baþ harfler farklý ise belirtilen noktalara ýþýnla
                Debug.Log($"Baþ harfler farklý ('{fruit1.name}', '{fruit2.name}')! Belirtilen konumlara ýþýnlanýyor.");
                TeleportToFixedPosition(fruit1, new Vector3(0, 0.426f, -2));
                TeleportToFixedPosition(fruit2, new Vector3(1, 0.426f, -2));
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
        }
    }

    private int GetPoints(char firstChar)
    {
        // Baþ harfe göre puan döndüren yöntem
        switch (firstChar)
        {
            case 'A': return 5;
            case 'B': return 10;
            case 'C': return 15;
            case 'D': return 20;
            case 'E': return 25;
            case 'H': return 30;
            case 'I': return 35;
            case 'S': return 200;
            default: return 0; // Diðer harfler için puan yok
        }
    }

    private void TeleportToFixedPosition(GameObject fruit, Vector3 targetPosition)
    {
        fruit.transform.position = targetPosition; // Meyveyi belirtilen pozisyona ýþýnla
        Debug.Log($"'{fruit.name}' {targetPosition} konumuna ýþýnlandý.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {_score}"; // Puaný TextMesh Pro Text'e yaz
        }
        else
        {
            Debug.LogError("ScoreText is not assigned!");
        }
    }

    private IEnumerator ActivateZSkill()
    {
        // Z skilli aktif ediliyor
        isZActive = true;
        zMultiplier = 2; // Çarpan 2 yapýlýyor
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(false); // Z skill UI'yi gizle
        }

        Debug.Log("Z skilli aktif! Puanlar 2x oldu.");

        canUseZ = false; // Z tuþu devre dýþý, skill aktif
        yield return new WaitForSeconds(10); // Z skilli 10 saniye aktif

        // Z skilli sona eriyor
        isZActive = false;
        zMultiplier = 1; // Çarpan normale dönüyor
        Debug.Log("Z skilli sona erdi.");

        // 10 saniye bekle (cooldown)
        yield return new WaitForSeconds(10);
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(true); // Z skill UI'yi tekrar göster
        }

        canUseZ = true; // Z tekrar kullanýlabilir
    }

    // Bu kýsým eklendi: Dýþarýdan kolayca puan eklemek için.
    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreUI();
    }
}
