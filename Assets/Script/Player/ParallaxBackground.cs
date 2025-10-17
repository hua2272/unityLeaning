using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    private GameObject cam;
    [SerializeField] private float parallaxEffect;
    private float xPosition;
    private float length;

    void Start()
    {
        cam = GameObject.Find("Main Camera");
        xPosition = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void Update()
    {
        float distanceToMove = cam.transform.position.x * parallaxEffect;
        transform.position = new Vector3(xPosition + distanceToMove, transform.position.y);
        
        float distaceMoved = cam.transform.position.x * (1 - parallaxEffect);
        if (distaceMoved > xPosition + length)
        {
            xPosition = xPosition + length;
        } else if (distaceMoved < xPosition - length)
        {
            xPosition = xPosition - length;
        }
    }
}