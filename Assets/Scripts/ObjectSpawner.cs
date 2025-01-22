using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI Image i�in gerekli

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

        // �lk spawn i�lemi
        SpawnObjects();
    }

    void Update()
    {
        // E�er sahnede spawnlanan objelerin hepsi yok edilmi�se yeniden spawn et
        spawnedObjects.RemoveAll(obj => obj == null); // Listeyi null olan objelerden temizle
        if (spawnedObjects.Count == 0)
        {
            SpawnObjects();
        }

        // X tu�una bas�ld���nda Golden Apple spawn et ve X skillini 15sn boyunca devre d��� b�rak
        if (Input.GetKeyDown(KeyCode.X) && canUseXSkill)
        {
            SpawnSteak();
            StartCoroutine(DisableXSkillTemporarily());
        }

        // C tu�unun cooldown kontrol�
        if (cSkillCooldownTimer > 0)
        {
            cSkillCooldownTimer -= Time.deltaTime; // Cooldown s�resi azal�yor
        }
        else
        {
            canUseCSkill = true; // C skillinin cooldown s�resi bitti�inde tekrar kullan�labilir
            // C skillinin UI g�rselini tekrar aktif et
            if (cSkillImage != null && !cSkillImage.gameObject.activeSelf)
            {
                cSkillImage.gameObject.SetActive(true);
            }
        }

        // C tu�una bas�ld���nda skill aktifse, t�m objeleri yok et ve puan ver
        if (Input.GetKeyDown(KeyCode.C) && canUseCSkill && cSkillCooldownTimer <= 0)
        {
            ActivateCSkill();
        }
    }

    /// <summary>
    /// C skill (eski E): T�m objeleri yok eder, her biri i�in 30 puan verir.
    /// </summary>
    private void ActivateCSkill()
    {
        canUseCSkill = false;
        cSkillCooldownTimer = cSkillCooldown; // Cooldown ba�lat

        // C skill image'i devre d��� b�rak
        if (cSkillImage != null)
        {
            cSkillImage.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogError("C Skill Image is not assigned!");
        }

        // T�m spawnlanm�� objeleri yok et, puan ekle
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

        Debug.Log($"C skill aktif! {objectCount} nesne yok edildi. Toplam kazan�lan puan: {objectCount * 30}");
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

        // X tu�una bas�ld���nda g�rseli gizle
        if (xSkillImage != null)
        {
            xSkillImage.gameObject.SetActive(false); // G�rseli gizle
        }
        else
        {
            Debug.LogError("X Skill Image is not assigned!");
        }
    }

    /// <summary>
    /// X skillin 15 saniye kapal� kalmas�n� sa�lar.
    /// </summary>
    private System.Collections.IEnumerator DisableXSkillTemporarily()
    {
        canUseXSkill = false; // X skillini ge�ici olarak devre d��� b�rak

        // 15 saniye bekle
        yield return new WaitForSeconds(15f);

        // S�re bitince X skillini tekrar kullan�labilir yap
        canUseXSkill = true;

        // G�rseli tekrar aktif et
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
