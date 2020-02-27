﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
    public Player player;
    public Slot slot;
    public LayerMask colliderMask;
    Vector2 _ColliderSize;
    public GameObject oppositeRoom;
    [Header("Debug")]
    public Collider2D _Collider;
    public Item item = null;


    private void FixedUpdate()
    {
        if (TryGetComponent<BoxCollider2D>(out BoxCollider2D box))
        {
            _ColliderSize = box.size;
            _Collider = Physics2D.OverlapBox(transform.position, _ColliderSize, 0, colliderMask);
            if (_Collider)
            {
                if (CheckPerspective(_Collider.gameObject) && _Collider.GetComponent<Item>())
                {
                    item = _Collider.GetComponent<Item>();
                    UIController.Get.Interact(_Collider.gameObject, true);
                }
            }
        }
    }

    public bool PickUpAllowed
    {
        get
        {
            if (slot)
            {

                if (slot.ItemSlot == null && item != null)
                {
                    if (UIController.Get.caller != item.gameObject)
                        return false;
                    return true;
                }
            }
            return false;
        }
    }
    public bool DropAllowed
    {
        get
        {
            if (slot)
                if (slot.ItemSlot != null)
                    return true;
            return false;
        }
    }
    Perspective ParentPerspective(Transform transform)
    {
        transform.root.TryGetComponent<ThisPerspective>(out ThisPerspective thisPerspective);
        if (thisPerspective)
            return thisPerspective.perspective;
        else
            return Perspective.None;
    }
    bool CheckPerspective(GameObject gameObject)
    {
        if (TryGetComponent<ThisPerspective>(out ThisPerspective playerPerspective) && playerPerspective.perspective == GameController.Get.CurrentPerspective)
        {
            if (ParentPerspective(gameObject.transform) == GameController.Get.CurrentPerspective)
            {
                return true;
            }
            if (ParentPerspective(gameObject.transform) == Perspective.None)
            {
                return true;
            }
        }
        return false;
    }

    public void PickUpItem(Item item = null)
    {

        if (item != null && slot.ItemSlot == null)
        {
            slot.ItemSlot = item;
            slot.GetComponent<UnityEngine.UI.Image>().sprite = item.parent.GetComponent<SpriteRenderer>().sprite;
            item.parent.localPosition = new Vector2(222, 222);
            item.parent.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
            item.GetComponent<Collider2D>().enabled = true;
            return;
        }
        if (PickUpAllowed == false)
            return;

        item = this.item;
        //print("Take1");
        slot.ItemSlot = item;

        slot.GetComponent<UnityEngine.UI.Image>().sprite = item.parent.GetComponent<SpriteRenderer>().sprite;
        item.parent.localPosition = new Vector2(222, 222);
        item.parent.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;
        item.GetComponent<Collider2D>().enabled = true;

        //itembutton = Instantiate(itembuttonPrefab, inventory.slots[i].transform, false);
    }
    public void PlaceItem(Transform transform)
    {
        if (transform.TryGetComponent<ItemHolder>(out ItemHolder itemHolder))
        {
            if (itemHolder.HeldItem && slot.ItemSlot == null) // PickUp
            {
                PickUpItem(itemHolder.HeldItem);
                itemHolder.HeldItem = null;
                return;
            }
            else
            {
                if (!DropAllowed)
                    return;
                if (itemHolder.HeldItem != null)
                    return;

                itemHolder.HeldItem = slot.ItemSlot;
            }
        }

        if (!DropAllowed)
            return;


        Vector3 newPosition = transform.position;

        Item item = slot.ItemSlot;
        item.GetComponent<Collider2D>().enabled = false;
        item.parent.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeAll;

        item.parent.position = newPosition;
        newPosition = item.parent.position;
        newPosition.z = -1;
        item.parent.localPosition = newPosition;

        slot.GetComponent<UnityEngine.UI.Image>().sprite = null;
        //print("Place");
        slot.ItemSlot = null;

    }
    public void DropItem()
    {
        if (!DropAllowed)
            return;
        Vector3 newPosition = transform.GetComponent<Transform>().localPosition;
        newPosition.z = -1;

        Item item = slot.ItemSlot;

        item.GetComponent<Collider2D>().enabled = true;
        item.parent.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        item.parent.localPosition = newPosition;
        slot.GetComponent<UnityEngine.UI.Image>().sprite = null;
        slot.ItemSlot = null;
    }
    public void TransferItem(Transform transform)
    {
        if (!DropAllowed)
            return;

        Vector3 newPosition = this.transform.GetComponent<Transform>().localPosition;
        newPosition.z = -1;
        newPosition.x += 1;

        Item item = slot.ItemSlot;
        item.GetComponent<Collider2D>().enabled = true;
        item.parent.parent = oppositeRoom.transform;
        item.parent.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.None;
        item.parent.GetComponent<Transform>().localPosition = newPosition;
        slot.GetComponent<UnityEngine.UI.Image>().sprite = null;
        slot.ItemSlot = null;
    }
}