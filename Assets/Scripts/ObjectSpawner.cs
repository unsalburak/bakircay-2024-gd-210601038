using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rastgele objeler (dragObjects) spawn eder.
/// "Steak" butonuna t�klay�nca 2 steak �retir, 15 saniye cooldown.
/// "Yoket" butonuna t�klay�nca t�m objeleri yok eder, +30 puan/obje, 60 saniye cooldown.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [Header("Objeleri ve Prefablar�")]
    [SerializeField] private DragObject[] dragObjects;   // Farkl� t�r yiyeceklerin prefab'lar�n� tutan dizi
    [SerializeField] private GameObject steakPrefab;      // Steak (�nceden GoldenApple)

    [Header("Spawn Ayarlar�")]
    [SerializeField] private float radius = 5f; // Rastgele spawn alan� yar��ap�

    [Header("Buton Referanslar� (Steak & Yoket)")]
    [SerializeField] private Button steakButton;          // "Steak D���r" butonu
    [SerializeField] private Button yoketButton;          // "Yok Et" butonu

    [Header("G�rsel Referanslar (Opsiyonel)")]
    [SerializeField] private Image steakSkillImage;       // Steak skill i�in UI image g�stergesi
    [SerializeField] private Image yoketSkillImage;       // Yoket skill i�in UI image g�stergesi

    // Steak skill cooldown kontrol�
    private bool canUseSteakSkill = true;
    private float steakCooldown = 15f;

    // Yoket skill cooldown kontrol�
    private bool canUseYoketSkill = true;
    private float yoketCooldown = 60f;

    // Mevcut sahnedeki objeleri takip
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Skor art��� i�in FruitContainer'a eri�mek istiyoruz
    [Header("Skor Sistemi")]
    [SerializeField] private FruitContainer fruitContainer;

    private void Start()
    {
        // E�er FruitContainer atanmad�ysa sahnede ara
        if (fruitContainer == null)
        {
            fruitContainer = FindObjectOfType<FruitContainer>();
            if (fruitContainer == null)
            {
                Debug.LogError("FruitContainer not found in the scene!");
            }
        }

        // Sahne ba��nda objeleri spawn et
        SpawnObjects();

        // Butonlar�n OnClick event'ini Inspector'da ya da burada ekleyebilirsiniz.
        if (steakButton != null)
        {
            steakButton.onClick.AddListener(OnSteakButtonClicked);
        }
        if (yoketButton != null)
        {
            yoketButton.onClick.AddListener(OnYoketButtonClicked);
        }
    }

    private void Update()
    {
        // Yok olan objeleri listeden temizle
        spawnedObjects.RemoveAll(obj => obj == null);

        // Sahnedeki t�m objeler yok olmu�sa tekrar spawn et
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
        }
    }

    #region --- STEAK Skill ---

    /// <summary>
    /// Steak butonuna t�klan�nca �al��acak metod
    /// </summary>
    public void OnSteakButtonClicked()
    {
        if (canUseSteakSkill)
        {
            // 2 adet steak spawn
            SpawnSteak();
            // 15 saniye cooldown coroutini ba�lat
            StartCoroutine(SteakSkillCooldown());
        }
        else
        {
            Debug.Log("Steak skill �u an kullan�lamaz (cooldown devam ediyor).");
        }
    }

    private void SpawnSteak()
    {
        if (steakPrefab != null)
        {
            // 2 adet steak olu�tur
            for (int i = 0; i < 2; i++)
            {
                GameObject steak = Instantiate(steakPrefab, transform);
                steak.transform.localPosition = GetRandomPosition();
                spawnedObjects.Add(steak);
            }
            Debug.Log("Steak spawned!");
        }
        else
        {
            Debug.LogError("steakPrefab is not assigned!");
        }

        // �sterseniz steakSkillImage'i kapatabilirsiniz
        if (steakSkillImage != null)
        {
            steakSkillImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator SteakSkillCooldown()
    {
        // Skill kullan�lamaz
        canUseSteakSkill = false;

        // Buton gri olsun
        if (steakButton != null)
        {
            steakButton.interactable = false;
        }

        // 15 saniye bekle
        yield return new WaitForSeconds(steakCooldown);

        // Cooldown bitti
        canUseSteakSkill = true;

        // G�rseli tekrar a�
        if (steakSkillImage != null)
        {
            steakSkillImage.gameObject.SetActive(true);
        }

        // Buton tekrar aktif
        if (steakButton != null)
        {
            steakButton.interactable = true;
        }
    }

    #endregion

    #region --- YOKET Skill ---

    /// <summary>
    /// Yoket butonuna t�klan�nca �al��acak metod
    /// </summary>
    public void OnYoketButtonClicked()
    {
        if (canUseYoketSkill)
        {
            // T�m objeleri yok et + skor ekle
            YoketT�mObjeler();
            // 60 saniye cooldown coroutini ba�lat
            StartCoroutine(YoketSkillCooldown());
        }
        else
        {
            Debug.Log("Yoket skill �u an kullan�lamaz (cooldown devam ediyor).");
        }
    }

    private void YoketT�mObjeler()
    {
        int objectCount = spawnedObjects.Count;
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // Her obje i�in +30 puan
        if (fruitContainer != null)
        {
            fruitContainer.AddScore(objectCount * 30);
        }
        Debug.Log($"Yoket skill aktif! {objectCount} obje yok edildi, +{objectCount * 30} puan.");
    }

    private IEnumerator YoketSkillCooldown()
    {
        // Skill kullan�lamaz
        canUseYoketSkill = false;

        // Buton gri olsun
        if (yoketButton != null)
        {
            yoketButton.interactable = false;
        }

        // G�rseli kapat
        if (yoketSkillImage != null)
        {
            yoketSkillImage.gameObject.SetActive(false);
        }

        // 60 saniye bekle
        yield return new WaitForSeconds(yoketCooldown);

        // Cooldown bitti
        canUseYoketSkill = true;

        // G�rseli tekrar a�
        if (yoketSkillImage != null)
        {
            yoketSkillImage.gameObject.SetActive(true);
        }

        // Buton tekrar aktif
        if (yoketButton != null)
        {
            yoketButton.interactable = true;
        }
    }

    #endregion

    #region --- OBJELER� SPAWN ETME ---

    private void SpawnObjects()
    {
        // dragObjects dizisindeki her prefabdan 2 adet rastgele konumda olu�tur
        foreach (var dragObject in dragObjects)
        {
            for (int i = 0; i < 2; i++)
            {
                GameObject instance = Instantiate(dragObject.Prefab, transform);
                instance.transform.localPosition = GetRandomPosition();
                spawnedObjects.Add(instance);
            }
        }
    }

    private Vector3 GetRandomPosition()
    {
        // Belirtilen radius i�inde rastgele bir konum (2D daire)
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        // Bunu 3D pozisyona �evir
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
        // Y eksenini biraz yukar� alabiliriz
        spawnPos.y = 1.5f;
        return spawnPos;
    }

    private void OnDrawGizmosSelected()
    {
        // Edit�rde spawn alan�n� g�stermek i�in
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    #endregion
}
