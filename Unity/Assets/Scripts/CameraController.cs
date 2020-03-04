﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform leftBorder = null, rightBorder = null;
    [SerializeField] Transform target = null;
    [HideInInspector] public Vector3 pos = Vector3.zero;
    public float distanceUntilFollow = 2f;
    public float cameraSpeed = 2;
    float yPos = 0;
    float zPos = -10;
    private void Awake()
    {
        zPos = transform.position.z;
        yPos = transform.position.y;
    }
    void FixedUpdate()
    {
        if (target)
            pos = target.position;
        pos.z = zPos;
        pos.y = yPos;




        if (leftBorder && rightBorder) // Borders
        {
            float horz = Camera.main.orthographicSize * Screen.width / Screen.height;


            float minX = leftBorder.position.x + horz;
            float maxX = rightBorder.position.x - horz;

            if (pos.x < minX)
                pos.x = minX;
            if (pos.x > maxX)
                pos.x = maxX;
        }

        if (Vector2.Distance(transform.position, pos) > distanceUntilFollow)
            transform.position =
                new Vector3(Mathf.Lerp(transform.position.x, pos.x, Time.deltaTime * cameraSpeed), yPos, zPos);
    }

    private void OnEnable()
    {
        if (TryGetComponent<Camera>(out Camera camera))
            camera.enabled = true;
    }
    private void OnDisable()
    {
        if (TryGetComponent<Camera>(out Camera camera))
            camera.enabled = false;
    }
}