using UnityEngine;
using System.Collections.Generic;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] rooms;          // Префабы комнат
    public float spacing = 0f;          // Зазор (0 = комнаты вплотную)

    private float roomWidth;            // Реальная ширина комнаты
    private float roomHeight;           // Реальная высота комнаты

    void Start()
    {
        if (rooms == null || rooms.Length == 0)
        {
            Debug.LogError("Массив rooms пуст! Перетащите префабы.");
            return;
        }

        // Определяем реальные размеры комнаты (по первому префабу, предполагаем что все одинакового размера)
        GetRoomSize();

        // Генерируем связный кластер
        GenerateConnectedCluster();
    }

    void GetRoomSize()
    {
        GameObject temp = Instantiate(rooms[0], Vector3.zero, Quaternion.identity);
        temp.SetActive(false);
        Renderer rend = temp.GetComponent<Renderer>();
        if (rend != null)
        {
            roomWidth = rend.bounds.size.x;
            roomHeight = rend.bounds.size.y;
        }
        else
        {
            Collider2D col2d = temp.GetComponent<Collider2D>();
            if (col2d != null)
            {
                roomWidth = col2d.bounds.size.x;
                roomHeight = col2d.bounds.size.y;
            }
            else
            {
                Collider col3d = temp.GetComponent<Collider>();
                if (col3d != null)
                {
                    roomWidth = col3d.bounds.size.x;
                    roomHeight = col3d.bounds.size.y;
                }
                else
                {
                    roomWidth = roomHeight = 10f; // значение по умолчанию
                    Debug.LogWarning("Не удалось определить размер комнаты, используется 10");
                }
            }
        }
        DestroyImmediate(temp);

        // Добавляем зазор (если нужен)
        roomWidth += spacing;
        roomHeight += spacing;
    }

    void GenerateConnectedCluster()
    {
        int roomsToSpawn = Random.Range(4, 9); // 4..8
        // Множество занятых ячеек (координаты в сетке)
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();
        // Начинаем с центральной ячейки (0,0)
        Vector2Int start = Vector2Int.zero;
        occupied.Add(start);
        List<Vector2Int> frontier = new List<Vector2Int>(); // граничные ячейки для расширения

        // Пока не набрали нужное количество комнат
        while (occupied.Count < roomsToSpawn)
        {
            // Собираем все свободные соседние ячейки от всех занятых
            List<Vector2Int> candidates = new List<Vector2Int>();
            foreach (var cell in occupied)
            {
                Vector2Int[] neighbors = {
                    cell + Vector2Int.up,
                    cell + Vector2Int.down,
                    cell + Vector2Int.left,
                    cell + Vector2Int.right
                };
                foreach (var nb in neighbors)
                {
                    if (!occupied.Contains(nb))
                        candidates.Add(nb);
                }
            }
            // Если нет кандидатов (невозможно расширить), выходим
            if (candidates.Count == 0) break;
            // Выбираем случайного кандидата
            Vector2Int newCell = candidates[Random.Range(0, candidates.Count)];
            occupied.Add(newCell);
        }

        // Теперь создаём комнаты в каждой занятой ячейке
        foreach (var cell in occupied)
        {
            Vector3 pos = new Vector3(cell.x * roomWidth, cell.y * roomHeight, 0f);
            GameObject newRoom = Instantiate(rooms[Random.Range(0, rooms.Length)], pos, Quaternion.identity);
            // Удаляем скрипт с созданной комнаты, чтобы не было бесконечной рекурсии
            Destroy(newRoom.GetComponent<DungeonGenerator>());
        }
    }
}