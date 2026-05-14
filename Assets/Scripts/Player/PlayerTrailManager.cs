using UnityEngine;
using System.Collections.Generic;

public class PlayerTrailManager : MonoBehaviour
{
    [Header("Trail Settings")]
    public float checkpointInterval = 1f;
    public float checkpointDuration = 10f;
    public int maxCheckpoints = 10;

    [Header("Gizmos")]
    public bool showGizmos = true;
    public Color checkpointColor = Color.green;
    public float checkpointGizmoSize = 0.3f;

    private List<Checkpoint> checkpoints = new List<Checkpoint>();
    private float checkpointTimer = 0f;

    private class Checkpoint
    {
        public Vector3 position;
        public float createTime;
    }

    private void Update()
    {
        checkpointTimer += Time.deltaTime;

        if (checkpointTimer >= checkpointInterval)
        {
            AddCheckpoint(transform.position);
            checkpointTimer = 0f;
        }

        CleanupOldCheckpoints();
    }

    private void AddCheckpoint(Vector3 position)
    {
        Checkpoint checkpoint = new Checkpoint
        {
            position = position,
            createTime = Time.time
        };

        checkpoints.Add(checkpoint);

        if (checkpoints.Count > maxCheckpoints)
        {
            checkpoints.RemoveAt(0);
        }
    }

    private void CleanupOldCheckpoints()
    {
        for (int i = checkpoints.Count - 1; i >= 0; i--)
        {
            if (Time.time - checkpoints[i].createTime >= checkpointDuration)
            {
                checkpoints.RemoveAt(i);
            }
        }
    }

    public List<Vector3> GetCheckpoints()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var checkpoint in checkpoints)
        {
            positions.Add(checkpoint.position);
        }
        return positions;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        Gizmos.color = checkpointColor;

        if (checkpoints.Count > 0)
        {
            for (int i = 0; i < checkpoints.Count - 1; i++)
            {
                Gizmos.DrawLine(checkpoints[i].position, checkpoints[i + 1].position);
            }

            foreach (var checkpoint in checkpoints)
            {
                Gizmos.DrawSphere(checkpoint.position, checkpointGizmoSize);
            }
        }
    }
}