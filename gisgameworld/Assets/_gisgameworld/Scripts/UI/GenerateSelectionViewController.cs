using System.Collections.Generic;
using UnityEngine;

public class GenerateSelectionViewController : MonoBehaviour
{
    public GameObject selectionViewItemPrefab;
    public Transform selectionViewContentTransform;

    public void ClearView()
    {
        foreach (Transform child in selectionViewContentTransform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    public void PopulateView(LocationData data, UIManager uiManager, GameManager manager)
    {
        GameObject currentItem = Instantiate(selectionViewItemPrefab, selectionViewContentTransform);
        SelectionViewItem currentLocationSelectionViewItem = currentItem.GetComponent<SelectionViewItem>();
        currentLocationSelectionViewItem.label.text = "Use Current Location";
        currentLocationSelectionViewItem.currentLocation = true;
        currentLocationSelectionViewItem.button.onClick.AddListener(() => { manager.StartCoroutine(uiManager.GenerateBuildings(true)); });

        if (data == null)
        {
            return;
        }

        List<Location> locations = data.locations;

        foreach(Location location in locations)
        {
            currentItem = Instantiate(selectionViewItemPrefab, selectionViewContentTransform);

            SelectionViewItem selectionViewItem = currentItem.GetComponent<SelectionViewItem>();
            selectionViewItem.location = location;
            selectionViewItem.label.text = location.name;
            selectionViewItem.lat = location.coord.latitude;
            selectionViewItem.lon = location.coord.longitude;
            selectionViewItem.button.onClick.AddListener(() => { manager.StartCoroutine(uiManager.GenerateBuildings(false, location)); });
        }
    }
}
