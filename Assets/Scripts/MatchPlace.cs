using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // UI Button, Image vb. i�in

public class FruitContainer : MonoBehaviour
{
    private List<GameObject> _fruitsInContainer = new List<GameObject>(); // Kab�n i�indeki nesneleri saklayan liste
    private int _score = 0; // Toplam puan
    private Vector3 fixedPosition = new Vector3(-0.1f, 0.11f, 0.3f); // Kap i�inde sabitlenecek nokta
    private Animator childAnimator;

    [Header("Skor UI")]
    [SerializeField] private TMP_Text scoreText; // Puan g�stermek i�in TextMesh Pro Text

    [Header("Pair Destroy Skill (Yeni)")]
    [SerializeField] private Button pairDestroyButton;  // Bu skillin butonu (TextMeshPro da olabilir)
    [SerializeField] private float pairDestroyCooldown = 10f; // 10 sn cooldown
    private bool canUsePairDestroy = true;   // Skill �u an kullan�labilir mi
    private bool isPairDestroyActive = false;// Skill devrede mi (objeye t�klamay� bekliyoruz)

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

        // E�er butonu Inspector'da eklemediysen, OnClick() event'ini burada da ba�layabilirsin:
        if (pairDestroyButton != null)
        {
            pairDestroyButton.onClick.AddListener(OnPairDestroyButtonClicked);
        }
    }

    private void Update()
    {
        // E�er skill aktifse, kullan�c� mouse t�klamas�n� bekliyoruz
        if (isPairDestroyActive)
        {
            if (Input.GetMouseButtonDown(0))
            {
                // Sol t�klamada bir Raycast yap�p Moveable objeye denk geldiyse yok edelim
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f))
                {
                    // T�klanan obje Moveable ise
                    if (hit.collider.CompareTag("Moveable"))
                    {
                        GameObject clickedObj = hit.collider.gameObject;
                        DestroyPair(clickedObj);
                        // Skill tek seferlik, hemen devreden ��kal�m
                        isPairDestroyActive = false;
                        // PairDestroy cooldown ba�las�n
                        StartCoroutine(PairDestroyCooldown());
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Standart container mekani�i
        if (other.CompareTag("Moveable"))
        {
            other.transform.position = fixedPosition;
            Debug.Log($"'{other.name}' kab�n i�ine girdi: {fixedPosition}");

            // Fizik durdurma
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = true;
            }

            _fruitsInContainer.Add(other.gameObject);
            Debug.Log($"Sepete '{other.name}' eklendi.");
            CheckFruits(); // �ki obje geldiyse e�le�tirme
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Moveable"))
        {
            _fruitsInContainer.Remove(other.gameObject);
            Debug.Log($"'{other.name}' sepetten ��kt�.");
        }
    }

    /// <summary>
    /// Sepette 2 obje varsa, ilk harfleri ayn� m� kontrol eder.
    /// Ayn�ysa puan + yok et, farkl�ysa sahneye geri g�nder.
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
                Debug.Log($"E�le�ti ('{fruit1.name}', '{fruit2.name}') => +{points} puan, toplam: {_score}");

                Destroy(fruit1);
                Destroy(fruit2);
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
            else
            {
                // Farkl� harfler => geri g�nder
                Debug.Log($"Farkl� harf ('{fruit1.name}', '{fruit2.name}'). Geri g�nderiliyor.");
                TeleportToFixedPosition(fruit1, new Vector3(0, 0.426f, -2));
                TeleportToFixedPosition(fruit2, new Vector3(1, 0.426f, -2));
                _fruitsInContainer.Remove(fruit1);
                _fruitsInContainer.Remove(fruit2);
            }
        }
    }

    /// <summary>
    /// �nceki "Z skill" yerinde art�k "Pair Destroy Skill" var.
    /// Butona t�klay�nca aktif hale gelir, bir kez obje se�ilir.
    /// </summary>
    public void OnPairDestroyButtonClicked()
    {
        if (canUsePairDestroy)
        {
            // Skill aktif, objeye t�klamay� bekle
            isPairDestroyActive = true;
            // Butonu gri yap
            if (pairDestroyButton != null)
            {
                pairDestroyButton.interactable = false;
            }
            Debug.Log("PairDestroy skill aktif! T�klanacak objeyi bekliyor...");
        }
        else
        {
            Debug.Log("PairDestroy skill hen�z kullan�lamaz (cooldown)!");
        }
    }

    /// <summary>
    /// T�klanan objeyi ve ayn� harfle ba�layan di�er bir objeyi yok eder, puan ekler.
    /// </summary>
    private void DestroyPair(GameObject clickedObj)
    {
        // 1) T�klanan objenin ilk harfi
        char firstChar = clickedObj.name[0];
        int points = GetPoints(firstChar); // Harfin puan�

        // 2) Sahnedeki *ba�ka* Moveable objelerden ayn� harfli olan� bul
        // (Basit yakla��m: buldu�umuz ilk objeyi alal�m.)
        GameObject pairObj = FindPairObject(clickedObj, firstChar);

        if (pairObj != null)
        {
            // �ki obje birden yok
            Destroy(clickedObj);
            Destroy(pairObj);

            // Puan = points * 2 (��nk� 2 obje yok ettik)
            int totalPts = points * 2;
            _score += totalPts;
            UpdateScoreUI();

            Debug.Log($"'PairDestroy' => '{clickedObj.name}' + '{pairObj.name}' yok edildi. +{totalPts} puan (harf: {firstChar}). Toplam: {_score}");
        }
        else
        {
            // E�i yoksa sadece clickedObj'i yok edelim mi?  
            // Karar size kalm��, ben yok edece�im ve tek obje puan� veriyor.
            Destroy(clickedObj);
            _score += points;
            UpdateScoreUI();
            Debug.Log($"'PairDestroy' => Sadece '{clickedObj.name}' bulundu. +{points} puan. Toplam: {_score}");
        }

        // Sepet listesinden de ��karal�m
        _fruitsInContainer.Remove(clickedObj);
        _fruitsInContainer.Remove(pairObj);
    }

    /// <summary>
    /// Sahnede (veya sepette) clickedObj hari�, ayn� ilk harfe sahip bir obje bul.
    /// �stedi�inize g�re 'spawnedObjects' varsa oradan da bakabilirsiniz.
    /// Burada sadece sepet i�indekileri veya sahnedeki her "Moveable" � arayabilirsiniz.
    /// </summary>
    private GameObject FindPairObject(GameObject clickedObj, char firstChar)
    {
        // 1) Sadece sepet i�indekilerde aramak istersen:
        // return _fruitsInContainer.Find(o => o != null && o != clickedObj && o.name[0] == firstChar);

        // 2) Sahnedeki t�m Moveable objelerde aramak istersen:
        var allMoveables = GameObject.FindGameObjectsWithTag("Moveable");
        foreach (var m in allMoveables)
        {
            if (m != clickedObj && m.name.Length > 0 && m.name[0] == firstChar)
            {
                return m;
            }
        }

        // Bulunamad�
        return null;
    }

    /// <summary>
    /// PairDestroy cooldown coroutini (10 sn).
    /// Skill bir kere kullan�ld�ktan sonra 10 sn bekler, sonra tekrar aktifle�ir.
    /// </summary>
    private IEnumerator PairDestroyCooldown()
    {
        Debug.Log("PairDestroy i�lemi bitti, cooldown ba�l�yor (10sn).");
        yield return new WaitForSeconds(pairDestroyCooldown);

        canUsePairDestroy = true;
        if (pairDestroyButton != null)
        {
            pairDestroyButton.interactable = true;
        }
        Debug.Log("PairDestroy skill tekrar kullan�labilir.");
    }

    /// <summary>
    /// Char'a g�re puan tablosu (�rn. 'S' -> 200).
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
    /// Obje geri g�nderme (container mant���nda).
    /// </summary>
    private void TeleportToFixedPosition(GameObject fruit, Vector3 targetPosition)
    {
        fruit.transform.position = targetPosition;
        Debug.Log($"'{fruit.name}' {targetPosition} konumuna ���nland�.");
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {_score}";
        }
    }

    /// <summary>
    /// E�er ObjectSpawner vb. ba�ka kodlar skor eklemek isterse
    /// (�rne�in Yoket skillinde).
    /// </summary>
    public void AddScore(int amount)
    {
        _score += amount;
        UpdateScoreUI();
    }
}
