using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class ScrollViewAdapter : MonoBehaviour
{

    public RectTransform prefab;
    public TMP_InputField countText;
    public ScrollRect scrollView;
    public RectTransform content;

    List<ExampleItemView> views = new List<ExampleItemView>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void UpdateItems(){
        int newCount;
        int.TryParse(countText.text, out newCount);
        StartCoroutine(FetchItemModelsFromServer(newCount, results => OnReceiveNewModels(results)));
    }

    void OnReceiveNewModels(ExampleItemModel[] models){
        foreach(Transform child in content.transform){
            Destroy(child.gameObject);
        }

        views.Clear();

        int i = 0;

        foreach(var model in models){
            GameObject instance  = Instantiate(prefab.gameObject);
            instance.transform.SetParent(content, false);
            var view = InitializeItemView(instance, model);
            views.Add(view);
            ++i;
        }
    }

    ExampleItemView InitializeItemView(GameObject viewGameObject, ExampleItemModel model){
        ExampleItemView view = new ExampleItemView(viewGameObject.transform);

        view.titleText.text = model.title;
        return view;
    }



    IEnumerator FetchItemModelsFromServer(int count, Action<ExampleItemModel[]> onDone){
        yield return new WaitForSeconds(2f);

        var results = new ExampleItemModel[count];
        for (int i = 0; i < count; i++)
        {
            results[i] = new ExampleItemModel{
                title = $"Item {i}"
            };
        }

        onDone(results);
    }

    public class ExampleItemView{
        public TextMeshProUGUI titleText;

        // public ExampleItemModel Model{
        //     set {
        //         if (value == null){
        //             titleText.text = value.title;
        //         }
        //     }
        // }

        public ExampleItemView(Transform rootView){
            titleText = rootView.Find("TitlePanel/TitleText").GetComponent<TextMeshProUGUI>();
        }
    }

    public class ExampleItemModel{
        public string title;
    }
}
