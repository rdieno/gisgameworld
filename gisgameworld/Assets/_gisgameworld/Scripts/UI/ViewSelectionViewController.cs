using System.Collections.Generic;
using UnityEngine;

public class ViewSelectionViewController : MonoBehaviour
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
        if (data == null)
        {
            return;
        }

        List<Location> locations = data.locations;

        foreach(Location location in locations)
        {
            GameObject currentItem = Instantiate(selectionViewItemPrefab, selectionViewContentTransform);

            SelectionViewItem selectionViewItem = currentItem.GetComponent<SelectionViewItem>();
            selectionViewItem.location = location;
            selectionViewItem.label.text = location.name;
            selectionViewItem.lat = location.coord.latitude;
            selectionViewItem.lon = location.coord.longitude;
            selectionViewItem.button.onClick.AddListener(() => { manager.StartCoroutine(uiManager.LoadBuildings(location.id)); });
        }
    }
}
