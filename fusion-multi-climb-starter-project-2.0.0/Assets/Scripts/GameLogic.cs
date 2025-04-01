using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion;


public class GameLogic : NetworkBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private NetworkPrefabRef playerPrefab;
    [Networked, Capacity(12)] private NetworkDictionary<PlayerRef, Player> Players => default;

    [Networked] private PlayerRef CurrentItPlayer { get; set; }

    [Networked] private bool HasAssignedIt { get; set; }

    public void PlayerJoined(PlayerRef player)
    {
        if (HasStateAuthority)
        {
            NetworkObject playerObject = Runner.Spawn(playerPrefab, Vector3.up, Quaternion.identity, player);
            Player playerComponent = playerObject.GetComponent<Player>();
            Players.Add(player, playerComponent);

            if (Players.Count == 1)
            {
                SetPlayerAsIt(player, playerComponent);
            }
            else if (!HasAssignedIt)
            {
                AssignRandomPlayerAsIt();
            }
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
        if (!HasStateAuthority)
        {
            return;
        }

        if (Players.TryGet(player, out Player playerBehaviour))
        {
            bool wasIt = player == CurrentItPlayer;

            Players.Remove(player);
            Runner.Despawn(playerBehaviour.Object);

            if (wasIt && Players.Count > 0)
            {
                AssignRandomPlayerAsIt();
            }
        }
    }

    private void SetPlayerAsIt(PlayerRef playerRef, Player player)
    {
        if (CurrentItPlayer != default && Players.TryGet(CurrentItPlayer, out Player previousItPlayer))
        {
            previousItPlayer.IsIt = false;
        }

        CurrentItPlayer = playerRef;
        player.IsIt = true;
        HasAssignedIt = true;
    }

    private void AssignRandomPlayerAsIt()
    {
        if (Players.Count == 0 || !HasStateAuthority)
            return;

        int randomIndex = Random.Range(0, Players.Count);

        int currentIndex = 0;
        foreach (KeyValuePair<PlayerRef, Player> playerPair in Players)
        {
            if (currentIndex == randomIndex)
            {
                SetPlayerAsIt(playerPair.Key, playerPair.Value);
                break;
            }
            currentIndex++;
        }
    }
}
