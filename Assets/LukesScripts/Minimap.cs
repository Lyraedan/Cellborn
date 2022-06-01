using LukesScripts.AI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Minimap : MonoBehaviour
{
    public static Minimap instance;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this);
    }

    public MinimapBlip[] blips = new MinimapBlip[4] {
        new MinimapBlip(GridCell.GridFlag.WALKABLE, Color.clear),
        new MinimapBlip(GridCell.GridFlag.OCCUPIED, Color.gray),
        new MinimapBlip(GridCell.GridFlag.WALL, Color.black),
        new MinimapBlip(GridCell.GridFlag.CORNER, Color.black)
    };

    public Color playerBlip = Color.white;

    public MinimapBlip[,] map;

    private GridCell playerLast;

    private List<AI> entities = new List<AI>();

    private bool generated = false;

    [HideInInspector] public Image canvas;
    private Texture2D texture;

    public Action OnMapGenerated;

    private void Update()
    {
        if (!generated)
            return;

        Vector3 playerPosition = PositionAsGridCoordinates(WeaponManager.instance.player.transform);
        GridCell player = RoomGenerator.instance.navAgent.GetGridCellAt((int) playerPosition.x, (int) playerPosition.y, (int) playerPosition.z);

        if (player == null)
            return;

        if (playerLast == null)
        {
            playerLast = player;
            texture.SetPixel((int)player.position.x, (int)player.position.z, playerBlip);
            texture.Apply();
            return;
        }

        if (playerLast.position != player.position)
        {
            GridCell last = RoomGenerator.instance.navAgent.GetGridCellAt((int)playerLast.position.x, 0, (int) playerLast.position.z);
            MinimapBlip blip = GetBlip(last.flag);
            texture.SetPixel((int)playerLast.position.x, (int)playerLast.position.z, blip.color);
            texture.SetPixel((int)player.position.x, (int)player.position.z, playerBlip);
            texture.Apply();
        }
        playerLast = player;
    }

    public void AddEntity(AI ai)
    {
        ai.OnMinimapUpdated += UpdateEntityOnMinimap;
        Vector3 position = ai.transform.position;
        GridCell cell = RoomGenerator.instance.navAgent.GetGridCellAt((int)position.x, 0, (int)position.z);
        if(texture != null)
            texture.SetPixel((int)cell.position.x, (int)cell.position.z, ai.minimapBlip);
        entities.Add(ai);
    }

    public void RemoveEntity(AI ai)
    {
        ai.OnMinimapUpdated -= UpdateEntityOnMinimap;
        Vector3 position = ai.transform.position;
        GridCell cell = RoomGenerator.instance.navAgent.GetGridCellAt((int) position.x, 0, (int) position.z);
        MinimapBlip blip = GetBlip(cell.flag);
        if (texture != null)
            texture.SetPixel((int) cell.position.x, (int) cell.position.z, blip.color);
        entities.Remove(ai);
    }

    void UpdateEntityOnMinimap(AI instance, GridCell current, GridCell last)
    {
        try
        {
            if (last != current)
            {
                MinimapBlip blip = GetBlip(last.flag);
                texture.SetPixel((int)last.position.x, (int)last.position.z, blip.color);
                texture.SetPixel((int)current.position.x, (int)current.position.z, instance.minimapBlip);
                texture.Apply();
            }
        } catch(Exception e)
        {

        }
    }

    public void ClearMinimap(Grid grid)
    {
        int width = (int)grid.cells.x;
        int height = (int)grid.cells.z;

        for (int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                texture.SetPixel(x, y, new Color(0, 0, 0, 0));
            }
        }
    }

    public void GenerateMinimap(Grid grid)
    {
        int width = (int) grid.cells.x;
        int height = (int) grid.cells.z;

        Debug.Log($"Minimap: {width} x {height}");
        map = new MinimapBlip[width, height];

        texture = new Texture2D(width, height);

        for(int y = 0; y < height; y++)
        {
            for(int x = 0; x < width; x++)
            {
                GridCell cell = RoomGenerator.instance.navAgent.GetGridCellAt(x, 0, y);
                MinimapBlip blip = GetBlip(cell.flag);
                texture.SetPixel(x, y, blip.color);
            }
        }
        
        texture.filterMode = FilterMode.Point;
        texture.Apply();
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100.0f);
        canvas.sprite = sprite;

        int mapScale = 4; // As long as this number is even scaling is fine
        int mapWidth = width * mapScale;
        int mapHeight = height * mapScale;
        canvas.rectTransform.sizeDelta = new Vector2(mapWidth, mapHeight);
        canvas.rectTransform.anchorMin = new Vector2(1, 1);
        canvas.rectTransform.anchorMax = new Vector2(1, 1);
        canvas.rectTransform.anchoredPosition = new Vector2((-mapWidth / 2) - 10, (-mapHeight / 2) - 10);
        OnMapGenerated?.Invoke();
        generated = true;
    }

    MinimapBlip GetBlip(GridCell.GridFlag flag)
    {
        switch(flag)
        {
            case GridCell.GridFlag.WALKABLE:
                return blips[0];
            case GridCell.GridFlag.OCCUPIED:
                return blips[1];
            case GridCell.GridFlag.WALL:
                return blips[2];
            case GridCell.GridFlag.CORNER:
                return blips[3];
            default:
                return blips[0];
        }
    }

    public Vector3 PositionAsGridCoordinates(Transform transform)
    {
        var x = Mathf.RoundToInt(transform.position.x / RoomGenerator.instance.grid.cellSize.x);
        var y = Mathf.RoundToInt(transform.position.y / RoomGenerator.instance.grid.cellSize.y);
        var z = Mathf.RoundToInt(transform.position.z / RoomGenerator.instance.grid.cellSize.z);
        return new Vector3(x, y, z);
    }
}

[System.Serializable]
public class MinimapBlip
{
    public GridCell.GridFlag flag = GridCell.GridFlag.WALKABLE;
    public Color color = Color.white;

    public MinimapBlip(GridCell.GridFlag flag, Color color)
    {
        this.flag = flag;
        this.color = color;
    }
}
