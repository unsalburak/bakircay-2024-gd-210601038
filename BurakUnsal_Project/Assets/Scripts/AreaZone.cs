using System.Collections;
using UnityEngine;

public class AreaZone : MonoBehaviour
{
    public GameObject parentCube;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("nesne"))
        {
            GetComponent<Collider>().isTrigger = false;
            SnapObjectToArea(other.gameObject);
        }
    }

    private void SnapObjectToArea(GameObject obj)
    {
        StartCoroutine(SmoothSnapAndDestroy(obj));
    }

    private IEnumerator SmoothSnapAndDestroy(GameObject obj)
    {
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Vector3 startPosition = obj.transform.position;
        Vector3 targetPosition = transform.position;
        targetPosition.y = parentCube.transform.position.y + 1f;

        float time = 0f;
        float duration = 0.5f;

        while (time < duration)
        {
            obj.transform.position = Vector3.Lerp(startPosition, targetPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        obj.transform.position = targetPosition;

        if (rb != null)
        {
            rb.isKinematic = false;
        }

        yield return new WaitForSeconds(3f);

        Destroy(obj);

        GetComponent<Collider>().isTrigger = true;
    }
}
