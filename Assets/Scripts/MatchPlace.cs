using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // UI Button, Image vb. için

public class FruitContainer : MonoBehaviour
{
    private List<GameObject> _fruitsInContainer = new List<GameObject>(); // Kabýn içindeki nesneleri saklayan liste
    private int _score = 0; // Toplam puan
    private Vector3 fixedPosition = new Vector3(-0.1f, 0.11f, 0.3f); // Kap içinde sabitlenecek nokta
    private Animator childAnimator;

    [Header("Skor UI")]
    [SerializeField] private TMP_Text scoreText; // Puan göstermek için TextMesh Pro Text

    [Header("Pair Destroy Skill (Yeni)")]
    [SerializeField] private Button pairDestroyButton;  // Bu skillin butonu (TextMeshPro da olabilir)
    [SerializeField] private float pairDestroyCooldown = 10f; // 10 sn cooldown
    private bool canUsePairDestroy = true;   // Skill þu an kullanýlabilir mi
    private bool isPairDestroyActive = false;// Skill devrede mi (objeye týklamayý bekliyoruz)

    private void Start()
    {
        // Kapak animasyonu (ChestV1_Top var ise)
        Transform child = transform.Find("ChestV3_Top");
        if (child != null)
        {
            childAnimator = child.GetComponent<Animator>();
        }
        else
        {
            Debug.LogWarning("Child 'ChestV1_Top' not found! Animator won't work.");
        }

        UpdateScoreUI();

        // Eðer butonu Inspector'da eklemediysen, OnClick() event'ini burada da baðlayabilirsin:
        if (pairDestroyButton != null)
        {
            pairDestroyButton.onClick.AddListener(OnPairDestroyButtonClicked);
        }
    }

    private void Update()
    {
        // Eðer skill aktifse, kullanýcý mouse týklamasýný bekliyoruz
        if (isPairDestroyActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Sol týklamada bir Raycast yapýp Moveable objeye denk geldiyse yok edelim
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    // Týklanan obje Moveable ise
                    if (hit.collider.CompareTag("Moveable"))
                    {
                        GameObject clickedObj = hit.collider.gameObject;
                        DestroyPair(clickedObj);
                        // Skill tek seferlik, hemen devreden çýkalým
                        isPairDestroyActive = false;
                        // PairDestroy cooldown baþlasýn
                        StartCoroutine(PairDestroyCooldown());
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Standart container mekaniði
        if (other.CompareTag("Moveable"))
        {
            other.transform.position = fixedPosition;
            Debug.Log($"'{other.name}' kabýn içine girdi: {fixedPosition}");

            // Fizik durdurma
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            _fruitsInContainer.Add(other.gameObject);
            Debug.Log($"Sepete '{other.name}' eklendi.");
            CheckFruits(); // Ýki obje geldiyse eþleþtirme
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Moveable"))
        {
            _fruitsInContainer.Remove(other.gameObject);
            Debug.Log($"'{other.name}' sepetten çýktý.");
        }
    }

    /// <summary>
    /// Sepette 2 obje varsa, ilk harfleri ayný mý kontrol eder.
    /// Aynýysa puan + yok et, farklýysa sahneye geri gönder.
    /// </summary>
    private void CheckFruits()
    {
        if (_fruitsInContainer.Count >= 2)
        {
            GameObject fruit1 = _fruitsInContainer[0];
            GameObject fruit2 = _fruitsInContainer[1];

            char firstChar1 = fruit1.name[0];
            char firstChar2 = fruit2.name[0];

            if (firstChar1 == firstChar2)
            {
                int points = GetPoints(firstChar1);
                if (childAnimator != null) childAnimator.SetTrigger("Close");

                _score += points;
                UpdateScoreUI();
                Debug.Log($"Eþleþti ('{fruit1.name}', '{fruit2.name}') => +{points} puan, toplam: {_score}");

                Destroy(fruit1);
                Destroy(fruit2);
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
            else
            {
                // Farklý harfler => geri gönder
                Debug.Log($"Farklý harf ('{fruit1.name}', '{fruit2.name}'). Geri gönderiliyor.");
                TeleportToFixedPosition(fruit1, new Vector3(0, 0.426f, -2));
                TeleportToFixedPosition(fruit2, new Vector3(1, 0.426f, -2));
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
        }
    }

    /// <summary>
    /// Önceki "Z skill" yerinde artýk "Pair Destroy Skill" var.
    /// Butona týklayýnca aktif hale gelir, bir kez obje seçilir.
    /// </summary>
    public void OnPairDestroyButtonClicked()
    {
        if (canUsePairDestroy)
        {
            // Skill aktif, objeye týklamayý bekle
            isPairDestroyActive = true;
            // Butonu gri yap
            if (pairDestroyButton != null)
            {
                pairDestroyButton.interactable = false;
            }
            Debug.Log("PairDestroy skill aktif! Týklanacak objeyi bekliyor...");
        }
        else
        {
            Debug.Log("PairDestroy skill henüz kullanýlamaz (cooldown)!");
        }
    }

    /// <summary>
    /// Týklanan objeyi ve ayný harfle baþlayan diðer bir objeyi yok eder, puan ekler.
    /// </summary>
    private void DestroyPair(GameObject clickedObj)
    {
        // 1) Týklanan objenin ilk harfi
        char firstChar = clickedObj.name[0];
        int points = GetPoints(firstChar); // Harfin puaný

        // 2) Sahnedeki *baþka* Moveable objelerden ayný harfli olaný bul
        // (Basit yaklaþým: bulduðumuz ilk objeyi alalým.)
        GameObject pairObj = FindPairObject(clickedObj, firstChar);

        if (pairObj != null)
        {
            // Ýki obje birden yok
            Destroy(clickedObj);
            Destroy(pairObj);

            // Puan = points * 2 (çünkü 2 obje yok ettik)
            int totalPts = points * 2;
            _score += totalPts;
            UpdateScoreUI();

            Debug.Log($"'PairDestroy' => '{clickedObj.name}' + '{pairObj.name}' yok edildi. +{totalPts} puan (harf: {firstChar}). Toplam: {_score}");
        }
        else
        {
            // Eþi yoksa sadece clickedObj'i yok edelim mi?  
            // Karar size kalmýþ, ben yok edeceðim ve tek obje puaný veriyor.
            Destroy(clickedObj);
            _score += points;
            UpdateScoreUI();
            Debug.Log($"'PairDestroy' => Sadece '{clickedObj.name}' bulundu. +{points} puan. Toplam: {_score}");
        }

        // Sepet listesinden de çýkaralým
        _fruitsInContainer.Remove(clickedObj);
        _fruitsInContainer.Remove(pairObj);
    }

    /// <summary>
    /// Sahnede (veya sepette) clickedObj hariç, ayný ilk harfe sahip bir obje bul.
    /// Ýstediðinize göre 'spawnedObjects' varsa oradan da bakabilirsiniz.
    /// Burada sadece sepet içindekileri veya sahnedeki her "Moveable" ý arayabilirsiniz.
    /// </summary>
    private GameObject FindPairObject(GameObject clickedObj, char firstChar)
    {
        // 1) Sadece sepet içindekilerde aramak istersen:
        // return _fruitsInContainer.Find(o => o != null && o != clickedObj && o.name[0] == firstChar);

        // 2) Sahnedeki tüm Moveable objelerde aramak istersen:
        var allMoveables = GameObject.FindGameObjectsWithTag("Moveable");
        foreach (var m in allMoveables)
        {
            if (m != clickedObj && m.name.Length > 0 && m.name[0] == firstChar)
            {
                return m;
            }
        }

        // Bulunamadý
        return null;
    }

    /// <summary>
    /// PairDestroy cooldown coroutini (10 sn).
    /// Skill bir kere kullanýldýktan sonra 10 sn bekler, sonra tekrar aktifleþir.
    /// </summary>
    private IEnumerator PairDestroyCooldown()
    {
        Debug.Log("PairDestroy iþlemi bitti, cooldown baþlýyor (10sn).");
        yield return new WaitForSeconds(pairDestroyCooldown);

        canUsePairDestroy = true;
        if (pairDestroyButton != null)
        {
            pairDestroyButton.interactable = true;
        }
        Debug.Log("PairDestroy skill tekrar kullanýlabilir.");
    }

    /// <summary>
    /// Char'a göre puan tablosu (örn. 'S' -> 200).
    /// </summary>
    private int GetPoints(char c)
    {
        switch (c)
        {
            case 'A': return 5;
            case 'B': return 10;
            case 'C': return 15;
            case 'D': return 20;
            case 'E': return 25;
            case 'H': return 30;
            case 'I': return 35;
            case 'S': return 200;
            default: return 0;
        }
    }

    /// <summary>
    /// Obje geri gönderme (container mantýðýnda).
    /// </summary>
    private void TeleportToFixedPosition(GameObject fruit, Vector3 targetPosition)
    {
        fruit.transform.position = targetPosition;
        Debug.Log($"'{fruit.name}' {targetPosition} konumuna ýþýnlandý.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {_score}";
        }
    }

    /// <summary>
    /// Eðer ObjectSpawner vb. baþka kodlar skor eklemek isterse
    /// (örneðin Yoket skillinde).
    /// </summary>
    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreUI();
    }
}
