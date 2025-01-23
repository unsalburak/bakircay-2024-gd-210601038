using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Rastgele objeler (dragObjects) spawn eder.
/// "Steak" butonuna týklayýnca 2 steak üretir, 15 saniye cooldown.
/// "Yoket" butonuna týklayýnca tüm objeleri yok eder, +30 puan/obje, 60 saniye cooldown.
/// </summary>
public class ObjectSpawner : MonoBehaviour
{
    [Header("Objeleri ve Prefablarý")]
    [SerializeField] private DragObject[] dragObjects;   // Farklý tür yiyeceklerin prefab'larýný tutan dizi
    [SerializeField] private GameObject steakPrefab;      // Steak (önceden GoldenApple)

    [Header("Spawn Ayarlarý")]
    [SerializeField] private float radius = 5f; // Rastgele spawn alaný yarýçapý

    [Header("Buton Referanslarý (Steak & Yoket)")]
    [SerializeField] private Button steakButton;          // "Steak Düþür" butonu
    [SerializeField] private Button yoketButton;          // "Yok Et" butonu

    [Header("Görsel Referanslar (Opsiyonel)")]
    [SerializeField] private Image steakSkillImage;       // Steak skill için UI image göstergesi
    [SerializeField] private Image yoketSkillImage;       // Yoket skill için UI image göstergesi

    // Steak skill cooldown kontrolü
    private bool canUseSteakSkill = true;
    private float steakCooldown = 15f;

    // Yoket skill cooldown kontrolü
    private bool canUseYoketSkill = true;
    private float yoketCooldown = 60f;

    // Mevcut sahnedeki objeleri takip
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Skor artýþý için FruitContainer'a eriþmek istiyoruz
    [Header("Skor Sistemi")]
    [SerializeField] private FruitContainer fruitContainer;

    private void Start()
    {
        // Eðer FruitContainer atanmadýysa sahnede ara
        if (fruitContainer == null)
        {
            fruitContainer = FindObjectOfType<FruitContainer>();
            if (fruitContainer == null)
            {
                Debug.LogError("FruitContainer not found in the scene!");
            }
        }

        // Sahne baþýnda objeleri spawn et
        SpawnObjects();

        // Butonlarýn OnClick event'ini Inspector'da ya da burada ekleyebilirsiniz.
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

        // Sahnedeki tüm objeler yok olmuþsa tekrar spawn et
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
        }
    }

    #region --- STEAK Skill ---

    /// <summary>
    /// Steak butonuna týklanýnca çalýþacak metod
    /// </summary>
    public void OnSteakButtonClicked()
    {
        if (canUseSteakSkill)
        {
            // 2 adet steak spawn
            SpawnSteak();
            // 15 saniye cooldown coroutini baþlat
            StartCoroutine(SteakSkillCooldown());
        }
        else
        {
            Debug.Log("Steak skill þu an kullanýlamaz (cooldown devam ediyor).");
        }
    }

    private void SpawnSteak()
    {
        if (steakPrefab != null)
        {
            // 2 adet steak oluþtur
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

        // Ýsterseniz steakSkillImage'i kapatabilirsiniz
        if (steakSkillImage != null)
        {
            steakSkillImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator SteakSkillCooldown()
    {
        // Skill kullanýlamaz
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

        // Görseli tekrar aç
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
    /// Yoket butonuna týklanýnca çalýþacak metod
    /// </summary>
    public void OnYoketButtonClicked()
    {
        if (canUseYoketSkill)
        {
            // Tüm objeleri yok et + skor ekle
            YoketTümObjeler();
            // 60 saniye cooldown coroutini baþlat
            StartCoroutine(YoketSkillCooldown());
        }
        else
        {
            Debug.Log("Yoket skill þu an kullanýlamaz (cooldown devam ediyor).");
        }
    }

    private void YoketTümObjeler()
    {
        int objectCount = spawnedObjects.Count;
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null) Destroy(obj);
        }
        spawnedObjects.Clear();

        // Her obje için +30 puan
        if (fruitContainer != null)
        {
            fruitContainer.AddScore(objectCount * 30);
        }
        Debug.Log($"Yoket skill aktif! {objectCount} obje yok edildi, +{objectCount * 30} puan.");
    }

    private IEnumerator YoketSkillCooldown()
    {
        // Skill kullanýlamaz
        canUseYoketSkill = false;

        // Buton gri olsun
        if (yoketButton != null)
        {
            yoketButton.interactable = false;
        }

        // Görseli kapat
        if (yoketSkillImage != null)
        {
            yoketSkillImage.gameObject.SetActive(false);
        }

        // 60 saniye bekle
        yield return new WaitForSeconds(yoketCooldown);

        // Cooldown bitti
        canUseYoketSkill = true;

        // Görseli tekrar aç
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

    #region --- OBJELERÝ SPAWN ETME ---

    private void SpawnObjects()
    {
        // dragObjects dizisindeki her prefabdan 2 adet rastgele konumda oluþtur
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
        // Belirtilen radius içinde rastgele bir konum (2D daire)
        Vector2 randomOffset = Random.insideUnitCircle * radius;
        // Bunu 3D pozisyona çevir
        Vector3 spawnPos = transform.position + new Vector3(randomOffset.x, 0, randomOffset.y);
        // Y eksenini biraz yukarý alabiliriz
        spawnPos.y = 1.5f;
        return spawnPos;
    }

    private void OnDrawGizmosSelected()
    {
        // Editörde spawn alanýný göstermek için
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    #endregion
}
