using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealSphere : HurtSphere {

    private void OnTriggerEnter(Collider other)
    {
        Player player;
        if (ShouldHealPlayer(other, out player))
            StartHealOverTime(player, amount);
    }

    private void OnTriggerExit(Collider other)
    {
        Player player;
        if (ShouldHealPlayer(other, out player))
            StopHealOverTime(player);
    }

    private bool ShouldHealPlayer(Collider col, out Player player)
    {
        player = null;
        if (IsPlayerCollider(col))
        {
            player = col.GetComponentInParent<Player>();
            if (player.team == playerThatCreated.team)
                return true;
        }
        return false;
    }

    private Dictionary<uint, IEnumerator> currentHealSequences = new Dictionary<uint, IEnumerator>();

    public void StartHealOverTime(Player player, int healthPerSecond)
    {
        StopHealOverTime(player);
        IEnumerator healSequence = player.HealOverTime(healthPerSecond);

        IEnumerator existingSequence;
        if (currentHealSequences.TryGetValue(player.netId.Value, out existingSequence))
        {
            Debug.LogError("Key value pair already exists in heal sequence");
            currentHealSequences.Remove(player.netId.Value);
        }
        currentHealSequences.Add(player.netId.Value, healSequence);
        StartCoroutine(healSequence);
    }

    public void StopHealOverTime(Player player)
    {
        IEnumerator healSequence;
        if (currentHealSequences.TryGetValue(player.netId.Value, out healSequence))
        {
            StopCoroutine(healSequence);
            currentHealSequences.Remove(player.netId.Value);
        }
    }

    private void OnDisable()
    {
        // Stop all coroutines
        foreach (KeyValuePair<uint, IEnumerator> pair in currentHealSequences)
        {
            StopCoroutine(pair.Value);
        }
        // Clear dictionary
        currentHealSequences.Clear();
    }
}
