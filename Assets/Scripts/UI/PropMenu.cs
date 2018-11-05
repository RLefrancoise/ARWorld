using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class PropMenu : Singleton<PropMenu> {

	public int ItemHeight = 200;

	public GameObject OpenButton;

	[SerializeField]
	private GameObject closeButton;

	public GameObject ItemPrefab;

	public List<GameObject> Props;

	[SerializeField]
	private GameObject list;

	public GameObject SelectedProp { get; private set; }

	public void Close()
	{
		gameObject.SetActive(false);
		OpenButton.SetActive(true);

        IEnumerable<Transform> currentProps = list.GetComponentsInChildren<Transform>(true).Where(x => x.gameObject != list);
        foreach (var prop in currentProps) Destroy(prop.gameObject);
	}

	public void Open()
	{
		gameObject.SetActive(true);
        OpenButton.SetActive(false);

		foreach(var prop in Props)
		{
			GameObject propButton = Instantiate(ItemPrefab, list.transform);
			propButton.name = prop.name;

			propButton.GetComponentInChildren<Text>().text = prop.name;
			propButton.GetComponent<Button>().onClick.AddListener(() => {
				SelectedProp = prop;
				Debug.LogFormat("Selected Prop: {0}", prop.name);
				Close();
			});
		}

        Vector2 size = GetComponent<RectTransform>().sizeDelta;
        size.y = Props.Count * ItemHeight;
        GetComponent<RectTransform>().sizeDelta = size;

        size = list.GetComponent<RectTransform>().sizeDelta;
        size.y = Props.Count * ItemHeight;
        list.GetComponent<RectTransform>().sizeDelta = size;

        size = closeButton.GetComponent<RectTransform>().sizeDelta;
        size.y = Props.Count * ItemHeight;
        closeButton.GetComponent<RectTransform>().sizeDelta = size;
	}
}
