using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI Image için gerekli

public class ObjectSpawner : MonoBehaviour
{
    [SerializeField] private DragObject[] dragObjects;
    [SerializeField] private GameObject steakPrefab;
    [SerializeField] private float radius;

    [SerializeField] private Image xSkillImage; 
    [SerializeField] private Image cSkillImage; 

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private bool canUseXSkill = true; 
    private bool canUseCSkill = true; 

    
    private float cSkillCooldown = 60f;
    private float cSkillCooldownTimer = 0f;

    [SerializeField] private FruitContainer fruitContainer;

    void Start()
    {

        if (fruitContainer == null)
        {
            fruitContainer = FindObjectOfType<FruitContainer>();
            if (fruitContainer == null)
            {
                Debug.LogError("FruitContainer is not found in the scene! Please assign it in the Inspector.");
            }
        }

        // Ýlk spawn iþlemi
        SpawnObjects();
    }

    void Update()
    {
        // Eðer sahnede spawnlanan objelerin hepsi yok edilmiþse yeniden spawn et
        spawnedObjects.RemoveAll(obj => obj == null); // Listeyi null olan objelerden temizle
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
        }

        // X tuþuna basýldýðýnda Golden Apple spawn et ve X skillini 15sn boyunca devre dýþý býrak
        if (Input.GetKeyDown(KeyCode.X) && canUseXSkill)
        {
            SpawnSteak();
            StartCoroutine(DisableXSkillTemporarily());
        }

        // C tuþunun cooldown kontrolü
        if (cSkillCooldownTimer > 0)
        {
            cSkillCooldownTimer -= Time.deltaTime; // Cooldown süresi azalýyor
        }
        else
        {
            canUseCSkill = true; // C skillinin cooldown süresi bittiðinde tekrar kullanýlabilir
            // C skillinin UI görselini tekrar aktif et
            if (cSkillImage != null && !cSkillImage.gameObject.activeSelf)
            {
                cSkillImage.gameObject.SetActive(true);
            }
        }

        // C tuþuna basýldýðýnda skill aktifse, tüm objeleri yok et ve puan ver
        if (Input.GetKeyDown(KeyCode.C) && canUseCSkill && cSkillCooldownTimer <= 0)
        {
            ActivateCSkill();
        }
    }

    /// <summary>
    /// C skill (eski E): Tüm objeleri yok eder, her biri için 30 puan verir.
    /// </summary>
    private void ActivateCSkill()
    {
        canUseCSkill = false;
        cSkillCooldownTimer = cSkillCooldown; // Cooldown baþlat

        // C skill image'i devre dýþý býrak
        if (cSkillImage != null)
        {
            cSkillImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("C Skill Image is not assigned!");
        }

        // Tüm spawnlanmýþ objeleri yok et, puan ekle
        int objectCount = spawnedObjects.Count;

        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        // Listeyi temizle
        spawnedObjects.Clear();

        // FruitContainer'a objectCount * 30 kadar puan ekle
        if (fruitContainer != null)
        {
            fruitContainer.AddScore(objectCount * 30);
        }

        Debug.Log($"C skill aktif! {objectCount} nesne yok edildi. Toplam kazanýlan puan: {objectCount * 30}");
    }

    /// <summary>
    /// Rastgele meyveleri sahneye spawn eder.
    /// </summary>
    private void SpawnObjects()
    {
        foreach (var dragObject in dragObjects)
        {
            for (int i = 0; i < 2; i++) // Her prefab'dan 2 tane spawn et
            {
                var instance = Instantiate(dragObject.Prefab, transform);
                instance.transform.localPosition = GetRandomPosition();
                spawnedObjects.Add(instance); // Spawnlanan objeyi listeye ekle
            }
        }
    }

    /// <summary>
    /// X skill (eski W): Golden Apple spawn eder, 15 saniye cooldown'a girer.
    /// </summary>
    private void SpawnSteak()
    {
        if (steakPrefab != null)
        {
            for (int i = 0; i < 2; i++) // Golden Apple'dan 2 tane spawn et
            {
                var goldenAppleInstance = Instantiate(steakPrefab, transform);
                goldenAppleInstance.transform.localPosition = GetRandomPosition();
                spawnedObjects.Add(goldenAppleInstance); // Listeye ekle
            }
            Debug.Log("Golden Apple spawned!");
        }
        else
        {
            Debug.LogError("Golden Apple Prefab is not assigned!");
        }

        // X tuþuna basýldýðýnda görseli gizle
        if (xSkillImage != null)
        {
            xSkillImage.gameObject.SetActive(false); // Görseli gizle
        }
        else
        {
            Debug.LogError("X Skill Image is not assigned!");
        }
    }

    /// <summary>
    /// X skillin 15 saniye kapalý kalmasýný saðlar.
    /// </summary>
    private System.Collections.IEnumerator DisableXSkillTemporarily()
    {
        canUseXSkill = false; // X skillini geçici olarak devre dýþý býrak

        // 15 saniye bekle
        yield return new WaitForSeconds(15f);

        // Süre bitince X skillini tekrar kullanýlabilir yap
        canUseXSkill = true;

        // Görseli tekrar aktif et
        if (xSkillImage != null)
        {
            xSkillImage.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError("X Skill Image is not assigned!");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }

    private Vector3 GetRandomPosition()
    {
        Vector3 randomOffset = Random.insideUnitCircle * radius;
        var randomPosition = transform.position + randomOffset;
        return new Vector3(randomPosition.x, 2, randomPosition.y);
    }
}
