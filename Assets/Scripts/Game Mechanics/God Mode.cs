using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GodMode : MonoBehaviour
{
    public GameObject FileEditorPanel;
    public GameObject FileEditor;
    public GameObject ListInfoPrefab;
    public GameObject VarInfoPrefab;
    public GameObject GoBackPrefab;

    public List<GameObject> firstLists;
    public List<GameObject> secondLists;
    public List<GameObject> thirdLists;

    public List<GameObject> changableItems;
    public PlayerDatas pd;

    private void EnterClass(object obj, Transform parent)
    {
        Debug.Log("enterin to new GO");
        Type type = obj.GetType();
        Debug.Log($"Scanning obj: {type.Name}");

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
        foreach (FieldInfo field in fields) { Debug.Log("field: " + field.Name); }

        GameObject nGo = SetNewGO(parent);
        GameObject newWindow;
        if (!parent.Find("in: " + type.Name))
        {
            newWindow = Instantiate(nGo, parent);
            newWindow.transform.name = "in: " + type.Name;
            foreach (Transform item in parent) item.gameObject.SetActive(false);
            newWindow.SetActive(true);
            CreateGoBack(newWindow.transform);
            ScanClass(obj, newWindow.transform);
        }
        else
        {
            foreach (Transform item in parent) item.gameObject.SetActive(false);
            parent.Find("in: " + obj).gameObject.SetActive(true);
            /*if (parent.Find("in: " + obj))
            {
                foreach (Transform tr in parent.Find("in: " + obj)) { Destroy(tr.gameObject); Debug.Log("destroyed object: " + tr.name); }
                foreach (Transform item in parent) item.gameObject.SetActive(false);
                CreateGoBack(parent.Find("in: " + type.Name));
                parent.Find("in: " + obj).gameObject.SetActive(true);
                ScanClass(obj, parent.Find("in: " + type.Name));
            }
            else Debug.Log("named class not found");*/
        }
    }

    private void EnterList(IList list, Transform parent)
    {
        GameObject nGo = SetNewGO(parent);

        GameObject newWindow;
        if (!parent.Find("in: " + list))
        {
            newWindow = Instantiate(nGo, parent);
            newWindow.transform.name = "in: " + list;
            foreach (Transform item in parent) item.gameObject.SetActive(false);
            newWindow.SetActive(true);
            CreateGoBack(newWindow.transform);
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Console.WriteLine($"[{i}] = {item}");

                // If list items are themselves complex classes, recurse into them
                if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
                    ScanClass(item, newWindow.transform);
            }
        }
        else
        {
            foreach (Transform item in parent) item.gameObject.SetActive(false);
            parent.Find("in: " + list).gameObject.SetActive(true);
            /*CreateGoBack(parent.Find("in: " + list));
            for (int i = 0; i < list.Count; i++)
            {
                var item = list[i];
                Console.WriteLine($"[{i}] = {item}");

                // If list items are themselves complex classes, recurse into them
                if (item != null && !item.GetType().IsPrimitive && item.GetType() != typeof(string))
                {
                    if (parent.Find("in: " + list))
                    {
                        foreach (Transform tr in parent) tr.gameObject.SetActive(false);
                        parent.Find("in: " + list).gameObject.SetActive(true);
                        foreach (Transform tr in parent.Find("in: " + list)) { Destroy(tr.gameObject); Debug.Log("destroyed object: " + tr.name); }
                        ScanClass(item, parent.Find("in: " + list));
                    }
                    else Debug.Log("named list not found");
                }
            }*/
        }
    }

    private GameObject SetNewGO(Transform p)
    {
        GameObject nGo = new();
        Destroy(nGo.GetComponent<Transform>());
        nGo.AddComponent<RectTransform>();
        nGo.AddComponent<Image>();
        nGo.AddComponent<VerticalLayoutGroup>();

        nGo.GetComponent<RectTransform>().sizeDelta = p.GetComponent<RectTransform>().sizeDelta;
        Debug.Log("sizeDelta set as Parent's");

        VerticalLayoutGroup from = p.GetComponent<VerticalLayoutGroup>();
        VerticalLayoutGroup to = nGo.GetComponent<VerticalLayoutGroup>();

        if (from != null && to != null)
        {
            to.spacing = from.spacing;
            to.padding = from.padding;
            to.childAlignment = from.childAlignment;
            to.childControlHeight = from.childControlHeight;
            to.childControlWidth = from.childControlWidth;
            to.childForceExpandHeight = from.childForceExpandHeight;
            to.childForceExpandWidth = from.childForceExpandWidth;
        }
        Debug.Log("vlg set as Parent's");
        return nGo;
    }

    public void UpdateFile()
    {
        var proto = StaticDatas.PlayerData;
        pd = proto.Clone();

        UpdateObjectFields(pd, "");
        //StaticDatas.PlayerData = pd;
        //StaticDatas.SaveDatas();
    }

    private void UpdateObjectFields(object obj, string parentPath)
    {
        if (obj == null) return;
        Type type = obj.GetType();
        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            string fieldPath = string.IsNullOrEmpty(parentPath)
                ? field.Name
                : parentPath + "." + field.Name;
            Debug.Log("fieldPath = " + fieldPath);

            object fieldValue = field.GetValue(obj);
            // Check if this field matches a UI item
            foreach (var item in changableItems)
            {
                string uiFieldName = GetStringName(item);
                if (uiFieldName == fieldPath)
                {
                    UpdateFieldFromUI(field, obj, item);
                    goto NextField;
                }
            }

            // If it's a class or list, dive deeper recursively
            if (fieldValue != null && !field.FieldType.IsPrimitive && field.FieldType != typeof(string))
            {
                if (typeof(IList).IsAssignableFrom(field.FieldType))
                {
                    Debug.Log($"it's list");
                    IList list = (IList)fieldValue;
                    for (int i = 0; i < list.Count; i++)
                    {
                        UpdateObjectFields(list[i], fieldPath + $"[{i}]");
                    }
                }
                else
                {
                    Debug.Log($"it's class {field.Name}");
                    UpdateObjectFields(fieldValue, fieldPath);
                }
            }

        NextField:;
        }
    }

    private void UpdateFieldFromUI(FieldInfo field, object obj, GameObject uiItem)
    {
        Transform cv = uiItem.transform.Find("Current Value/Current Value");
        int.TryParse(cv.GetComponent<TextMeshProUGUI>().text, out int currentValue);

        Transform setValue = uiItem.transform.Find("Set Value/Set Value IF/Text Area/Text");
        TextMeshProUGUI t = setValue.GetComponent<TextMeshProUGUI>();
        string textValue = t.text.Trim();

        Debug.Log($"Updating {field.Name}: {textValue}");

        if (field.FieldType == typeof(string))
        {
            if (!string.IsNullOrEmpty(textValue))
                field.SetValue(obj, textValue);
        }
        else
        {
            textValue = Regex.Replace(textValue, @"[^\d\.\-]", ""); // clean invalid chars
            if (int.TryParse(textValue, out int intValue))
                field.SetValue(obj, intValue);
            else if (float.TryParse(textValue, out float floatValue))
                field.SetValue(obj, floatValue);
            else if (double.TryParse(textValue, out double doubleValue))
                field.SetValue(obj, doubleValue);
            else if (bool.TryParse(textValue, out bool boolValue))
                field.SetValue(obj, boolValue);
        }
    }

    private void UpdateFieldFromCI()
    {

    }

    private string GetStringName(GameObject go)
    {
        // Example: "PlayerInfos.Water.amount"
        Transform vName = go.transform.Find("Variable Name/Variable Text");
        return vName.GetComponent<TextMeshProUGUI>().text.Trim();
    }

    public void OpenFileEditor()
    {
        foreach (Transform item in FileEditor.transform) Destroy(item.gameObject);
        FileEditorPanel.SetActive(true);
        CreateGoBack(FileEditor.transform);
        ScanClass(StaticDatas.PlayerData, FileEditor.transform);
    }

    private void ScanClass(object obj, Transform ph)
    {
        Type type = obj.GetType();
        Debug.Log($"Scanning class: {type.Name}");

        FieldInfo[] fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);

        foreach (FieldInfo field in fields)
        {
            if(field.Name != "saveVersion")
            {
                object value = field.GetValue(obj);
                Type fieldType = field.FieldType;
                Debug.Log($"value = {field.Name} = {value}");
                if (value is IList list)
                {
                    Debug.Log($"{field.Name} is a List with {list.Count} items:");

                    GameObject d_list = Instantiate(ListInfoPrefab, ph);
                    d_list.transform.name = field.Name;
                    Transform lname = d_list.transform.Find("List Name");
                    lname.GetComponent<TextMeshProUGUI>().text = field.Name;

                    Button button = d_list.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    Debug.Log("field " + field.Name + " added to Button as List");
                    button.onClick.AddListener(() => EnterList(list, ph));

                    d_list.AddComponent<ChangableItem>();
                    ChangableItem ci = d_list.GetComponent<ChangableItem>();
                    ci.item = value;
                    ci.category = ItemCat.List;
                }
                else if (fieldType.IsClass && fieldType != typeof(string))
                {
                    Debug.Log($"{field.Name} is a Class: {fieldType.Name}");

                    GameObject d_class = Instantiate(ListInfoPrefab, ph);
                    d_class.transform.name = field.Name;
                    Transform lname = d_class.transform.Find("List Name");
                    lname.GetComponent<TextMeshProUGUI>().text = field.Name;

                    Button button = d_class.GetComponent<Button>();
                    button.onClick.RemoveAllListeners();
                    Debug.Log("field " + field.Name + " added to Button as Class");
                    button.onClick.AddListener(() => EnterClass(value, ph));

                    d_class.AddComponent<ChangableItem>();
                    ChangableItem ci = d_class.GetComponent<ChangableItem>();
                    ci.item = value;
                    ci.category = ItemCat.Class;
                }
                else
                {
                    GameObject d_var = Instantiate(VarInfoPrefab, ph);
                    d_var.transform.name = field.Name;
                    Transform v_name = d_var.transform.Find("Variable Name");
                    Transform vn_text = v_name.Find("Variable Text"); ;
                    vn_text.GetComponent<TextMeshProUGUI>().text = field.Name;

                    Transform cv = d_var.transform.Find("Current Value");
                    Transform cv_v = cv.Find("Current Value");
                    cv_v.GetComponent<TextMeshProUGUI>().text = value.ToString();

                    Transform setValue = d_var.transform.Find("Set Value");
                    Transform inField = setValue.Find("Set Value IF");

                    if (fieldType.IsEnum) inField.GetComponent<TMP_InputField>().enabled = false;
                    else changableItems.Add(d_var);

                    d_var.AddComponent<ChangableItem>();
                    ChangableItem ci = d_var.GetComponent<ChangableItem>();
                    ci.item = value;
                    ci.category = ItemCat.Class;
                }
            }
        }
        RectTransform rts = ph.GetComponent<RectTransform>();
        VerticalLayoutGroup vlg = ph.GetComponent<VerticalLayoutGroup>();
        rts.sizeDelta = new Vector2(750, (ph.transform.childCount * 100) + ((ph.transform.childCount - 1) * vlg.spacing) + vlg.padding.top + vlg.padding.bottom);
    }

    private void CreateGoBack(Transform ph)
    {
        GameObject goback = Instantiate(GoBackPrefab, ph);
        Transform gbb = goback.transform.Find("Go Back Button");
        Button button = gbb.GetComponent<Button>();
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => GoBackMenu(ph));
    }

    private void GoBackMenu(Transform ph)
    {
        foreach (Transform item in ph.parent) if(!item.name.StartsWith("in: ")) item.gameObject.SetActive(true);
        ph.gameObject.SetActive(false);
    }
}