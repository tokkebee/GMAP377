using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "PipeRuleTile", menuName = "2D/Tiles/Pipe Rule Tile")]
public class PipesConnection : RuleTile<PipesConnection.Neighbor> {
    public bool customField;
    public static Dictionary<Vector3Int, HashSet<Vector3Int>> Connections = new Dictionary<Vector3Int, HashSet<Vector3Int>>();

    public class Neighbor : RuleTile.TilingRule.Neighbor {
        public const int Connected = 3;
        public const int NotConnected = 4;
    }

    public override bool RuleMatches(TilingRule rule, Vector3Int position, ITilemap tilemap, ref Matrix4x4 transform)
    {
        for (int i = 0; i < rule.m_Neighbors.Count; i++)
        {
            Vector3Int offset = rule.m_NeighborPositions[i];
            Vector3Int neighborPos = position + offset;
            int ruleState = rule.m_Neighbors[i];

            bool isConnected = Connections.ContainsKey(position) && Connections[position].Contains(neighborPos);

            if (ruleState == Neighbor.Connected && !isConnected) return false;
            if (ruleState == Neighbor.NotConnected && isConnected) return false;
        }
        return true;
    }
}