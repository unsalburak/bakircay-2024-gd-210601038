using System;
using UnityEngine;
using Common;

public class DetectObject : MonoBehaviour
{
    private Camera _mainCamera;
    private Rigidbody _selectedRigidbody = null;
    private Vector3 _dragOffset; // Fare ile nesne aras�ndaki ba�lang�� ofseti
    private bool _isDragging = false;
    private float _liftHeight = -0.15f; // Yerden y�kseklik

    private void Start()
    {
        _mainCamera = Camera.main;

        // TouchManager olaylar�na abone ol
        TouchManager.Instance.OnMouseDown += OnMouseDown;
        TouchManager.Instance.OnMouseDrag += OnMouseDrag;
        TouchManager.Instance.OnMouseUp += OnMouseUp;
    }

    private void OnDestroy()
    {
        // TouchManager olaylar�ndan ayr�l
        if (TouchManager.Instance == null)
            return;

        TouchManager.Instance.OnMouseDown -= OnMouseDown;
        TouchManager.Instance.OnMouseDrag -= OnMouseDrag;
        TouchManager.Instance.OnMouseUp -= OnMouseUp;
    }

    private void OnMouseDown(MouseData data)
    {
        RaycastHit hit;
        Ray ray = _mainCamera.ScreenPointToRay(data.position);

        if (Physics.Raycast(ray, out hit))
        {
            if (hit.rigidbody != null)
            {
                _selectedRigidbody = hit.rigidbody;
                _selectedRigidbody.isKinematic = true; // Fizi�i devre d��� b�rak
                _isDragging = true;

                // Ba�lang�� ofsetini hesapla
                Plane groundPlane = new Plane(Vector3.up, hit.transform.position.y);
                float distance;
                if (groundPlane.Raycast(ray, out distance))
                {
                    Vector3 hitPoint = ray.GetPoint(distance);
                    _dragOffset = hit.transform.position - hitPoint; // Fare ile nesne aras�ndaki ba�lang�� ofseti
                }
            }
        }
    }

    private void OnMouseDrag(MouseData data)
    {
        if (_selectedRigidbody != null && _isDragging)
        {
            Ray ray = _mainCamera.ScreenPointToRay(data.position);
            Plane groundPlane = new Plane(Vector3.up, _liftHeight); // Yerden y�kseklik

            float distance;
            if (groundPlane.Raycast(ray, out distance))
            {
                Vector3 targetPosition = ray.GetPoint(distance) + _dragOffset; // Hedef pozisyonu hesapla
                _selectedRigidbody.MovePosition(targetPosition); // Nesneyi hedef pozisyona ta��
            }
        }
    }

    private void OnMouseUp(MouseData data)
    {
        if (_selectedRigidbody != null)
        {
            _selectedRigidbody.isKinematic = false; // Fizi�i yeniden aktif et
            _isDragging = false;
            _selectedRigidbody = null; // Se�imi temizle
        }
    }
}
