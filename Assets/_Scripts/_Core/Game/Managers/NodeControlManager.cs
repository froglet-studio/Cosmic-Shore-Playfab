using StarWriter.Core;
using StarWriter.Utility.Singleton;
using System.Collections.Generic;
using UnityEngine;

// TODO: Rename to 'CellManager'
public class NodeControlManager : Singleton<NodeControlManager>
{
    [SerializeField] List<Node> Nodes;

    public Node GetNodeByPosition(Vector3 position)
    {
        foreach (var node in Nodes)
            if (node.ContainsPosition(position))
                return node;

        return null;
    }

    void OnEnable()
    {
        GameManager.onDeath += OutputNodeControl;
        GameManager.onGameOver += OutputNodeControl;
    }

    void OnDisable()
    {
        GameManager.onDeath -= OutputNodeControl;
        GameManager.onGameOver -= OutputNodeControl;
    }

    public void AddItem(NodeItem item)
    {
        foreach (var node in Nodes)
        {
            if (node.ContainsPosition(item.transform.position))
            {
                node.AddItem(item);
                break;
            }
        }
    }

    public void RemoveItem(NodeItem item)
    {
        foreach (var node in Nodes)
        {
            if (node.ContainsPosition(item.transform.position))
            {
                node.RemoveItem(item);
                break;
            }
        }
    }

    public void UpdateItem(NodeItem item)
    {
        foreach (var node in Nodes)
        {
            if (node.ContainsPosition(item.transform.position))
            {
                node.UpdateItem(item);
                break;
            }
        }
    }

    public void AddBlock(Teams team, string playerName, TrailBlockProperties blockProperties)
    {
        foreach (var node in Nodes)
        {
            if (node.ContainsPosition(blockProperties.position))
            {
                node.ChangeVolume(team, blockProperties.volume);
                break;
            }
        }
    }

    public void RemoveBlock(Teams team, string playerName, TrailBlockProperties blockProperties)
    {
        foreach (var node in Nodes)
        {
            if (node.ContainsPosition(blockProperties.position))
            {
                node.ChangeVolume(team, -blockProperties.volume);
                break;
            }
        }
    }

    public void OutputNodeControl()
    {
        foreach (var node in Nodes)
        {
            if (node.enabled)
                Debug.LogWarning($"Node Control - Node ID: {node.ID}, Controlling Team: {node.ControllingTeam}, Green Volume: {node.GetTeamVolume(Teams.Green)}, Red Volume: {node.GetTeamVolume(Teams.Red)}");
        }
    }
}