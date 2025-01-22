using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FruitContainer : MonoBehaviour
{
    private List<GameObject> _fruitsInContainer = new List<GameObject>(); // Kab�n i�indeki nesneleri saklayan liste
    private int _score = 0; // Toplam puan
    private Vector3 fixedPosition = new Vector3(-0.1f, 0.11f, 0.3f); // Sabitlenecek nokta
    private Animator childAnimator;

    [SerializeField] private TMP_Text scoreText; // Puan g�stermek i�in TextMesh Pro Text
    [SerializeField] private GameObject zSkillUI; // Canvas i�indeki Z image referans�
    private bool isZActive = false; // Z skilli aktif mi?
    private bool canUseZ = true; // Z skilli kullan�labilir mi?
    private float zMultiplier = 1; // Z skilli �arpan� (varsay�lan 1)

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

        // Ba�lang�� puan�n� UI'ya yaz
        UpdateScoreUI();

        // Z Skill UI ba�lang��ta aktif veya pasif; projede istedi�in duruma g�re ayarla
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(true);
        }
    }

    private void Update()
    {
        // Z skilli aktif de�ilse ve kullan�labilir durumdaysa
        if (Input.GetKeyDown(KeyCode.Z) && canUseZ)
        {
            StartCoroutine(ActivateZSkill());
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // E�er meyve ise listeye ekle ve kontrol et
        if (other.CompareTag("Moveable"))
        {
            // Nesneyi sabit pozisyona ta��
            other.transform.position = fixedPosition;
            Debug.Log($"'{other.name}' kab�n i�ine girdi ve konumu sabitlendi: {fixedPosition}");

            // Fizi�i durdur
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
        // Meyve kab� terk ederse listeden ��kar
        if (other.CompareTag("Moveable"))
        {
            _fruitsInContainer.Remove(other.gameObject);
            Debug.Log($"'{other.name}' sepetten ��kar�ld�.");
        }
    }

    private void CheckFruits()
    {
        // E�er kapta 2 veya daha fazla nesne varsa kontrol et
        if (_fruitsInContainer.Count >= 2)
        {
            GameObject fruit1 = _fruitsInContainer[0];
            GameObject fruit2 = _fruitsInContainer[1];

            // �simlerin ba� harflerini al
            char firstChar1 = fruit1.name[0];
            char firstChar2 = fruit2.name[0];

            if (firstChar1 == firstChar2)
            {
                // Ba� harfleri ayn� ise puan ekle ve nesneleri yok et
                int points = GetPoints(firstChar1);
                childAnimator.SetTrigger("Close");
                _score += Mathf.RoundToInt(points * zMultiplier); // �arpan� uygula
                UpdateScoreUI(); // UI'da puan� g�ncelle
                Debug.Log($"Ba� harfler ayn� ('{fruit1.name}', '{fruit2.name}')! Puan: +{points}. Toplam puan: {_score}");
                Destroy(fruit1);
                Destroy(fruit2);
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
            else
            {
                // Ba� harfler farkl� ise belirtilen noktalara ���nla
                Debug.Log($"Ba� harfler farkl� ('{fruit1.name}', '{fruit2.name}')! Belirtilen konumlara ���nlan�yor.");
                TeleportToFixedPosition(fruit1, new Vector3(0, 0.426f, -2));
                TeleportToFixedPosition(fruit2, new Vector3(1, 0.426f, -2));
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
        }
    }

    private int GetPoints(char firstChar)
    {
        // Ba� harfe g�re puan d�nd�ren y�ntem
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
            default: return 0; // Di�er harfler i�in puan yok
        }
    }

    private void TeleportToFixedPosition(GameObject fruit, Vector3 targetPosition)
    {
        fruit.transform.position = targetPosition; // Meyveyi belirtilen pozisyona ���nla
        Debug.Log($"'{fruit.name}' {targetPosition} konumuna ���nland�.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {_score}"; // Puan� TextMesh Pro Text'e yaz
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
        zMultiplier = 2; // �arpan 2 yap�l�yor
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(false); // Z skill UI'yi gizle
        }

        Debug.Log("Z skilli aktif! Puanlar 2x oldu.");

        canUseZ = false; // Z tu�u devre d���, skill aktif
        yield return new WaitForSeconds(10); // Z skilli 10 saniye aktif

        // Z skilli sona eriyor
        isZActive = false;
        zMultiplier = 1; // �arpan normale d�n�yor
        Debug.Log("Z skilli sona erdi.");

        // 10 saniye bekle (cooldown)
        yield return new WaitForSeconds(10);
        if (zSkillUI != null)
        {
            zSkillUI.SetActive(true); // Z skill UI'yi tekrar g�ster
        }

        canUseZ = true; // Z tekrar kullan�labilir
    }

    // Bu k�s�m eklendi: D��ar�dan kolayca puan eklemek i�in.
    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreUI();
    }
}
