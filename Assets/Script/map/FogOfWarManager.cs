using UnityEngine;
using System.Collections.Generic;

public class FogOfWarManager : MonoBehaviour
{
    [Header("Settings")]
    public GameObject fogPrefab;
    public Transform fogParent;
    public float revealRadius = 5f;
    public Vector2Int gridSize = new Vector2Int(20, 20);
    public Vector2 cellSize = new Vector2(1, 1);

    private List<GameObject> fogTiles = new List<GameObject>();
    public Transform player;

    void Start()
    {
        GenerateFogGrid();
    }

    void GenerateFogGrid()
    {
        for (int x = 0; x < gridSize.x; x++)
        {
            for (int y = 0; y < gridSize.y; y++)
            {
                Vector3 position = new Vector3(
                    x * cellSize.x - gridSize.x * cellSize.x / 2,
                    y * cellSize.y - gridSize.y * cellSize.y / 2,
                    0);
                
                GameObject fog = Instantiate(fogPrefab, position, Quaternion.identity, fogParent);
                fogTiles.Add(fog);
            }
        }
    }

    void Update()
    {
        UpdateFog();
    }

    void UpdateFog()
    {
        foreach (GameObject fog in fogTiles)
        {
            float distance = Vector2.Distance(player.position, fog.transform.position);
            SpriteRenderer renderer = fog.GetComponent<SpriteRenderer>();
            Collider2D collider = fog.GetComponent<Collider2D>();
        
            float targetAlpha = distance < revealRadius ? 
                Mathf.Clamp01(distance / revealRadius) : 1f;
        
            float currentAlpha = Mathf.Lerp(
                renderer.color.a, 
                targetAlpha, 
                Time.deltaTime * 5f);
        
            renderer.color = new Color(1, 1, 1, currentAlpha);
            collider.enabled = currentAlpha > 0.7f;
        }
    }
}