using System;
using NUnit.Framework;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    public bool isLaser;
    public bool isGlass;
    public bool isPickeUP;
    

    private void OnMouseUpAsButton()
    {
        if (PlayerInteractable.Instance.itemInHand) return;
        PlayerInteractable player = FindFirstObjectByType<PlayerInteractable>();

        if (player != null && player.IsItemNearby(this.transform))
        {
            if (!isPickeUP)
            {
                this.isPickeUP = true;
                player.OnItemClicked(this);
                GetComponent<BoxCollider>().enabled = false;
            }
            
        }
    }
}