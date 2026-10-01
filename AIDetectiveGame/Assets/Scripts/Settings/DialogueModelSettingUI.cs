using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueModelSettingUI : MonoBehaviour
{
    [Serializable]
    private class ModelOption
    {
        public string displayName;
        public string modelId;
    }

    private const string SelectedModelKey =
        "SelectedDialogueModel";

    [Header("UI")]
    [SerializeField]
    private TMP_Dropdown modelDropdown;

    [Header("Available Models")]
    [SerializeField]
    private List<ModelOption> modelOptions =
        new List<ModelOption>();

    [Header("Default")]
    [SerializeField]
    private string defaultModelId =
        "gpt-5.6-terra";


    private void Awake()
    {
        PopulateDropdown();
        LoadSavedSelection();

        if (modelDropdown != null)
        {
            modelDropdown.onValueChanged.AddListener(
                OnModelChanged);
        }
    }


    private void OnDestroy()
    {
        if (modelDropdown != null)
        {
            modelDropdown.onValueChanged.RemoveListener(
                OnModelChanged);
        }
    }


    private void PopulateDropdown()
    {
        if (modelDropdown == null)
        {
            return;
        }

        modelDropdown.ClearOptions();

        List<string> displayNames =
            new List<string>();

        foreach (ModelOption option in modelOptions)
        {
            displayNames.Add(option.displayName);
        }

        modelDropdown.AddOptions(displayNames);
    }


    private void LoadSavedSelection()
    {
        if (modelDropdown == null ||
            modelOptions == null ||
            modelOptions.Count == 0)
        {
            return;
        }

        string savedModelId =
            PlayerPrefs.GetString(
                SelectedModelKey,
                defaultModelId);

        int selectedIndex = 0;

        for (int i = 0; i < modelOptions.Count; i++)
        {
            if (modelOptions[i].modelId ==
                savedModelId)
            {
                selectedIndex = i;
                break;
            }
        }

        modelDropdown.SetValueWithoutNotify(
            selectedIndex);

        modelDropdown.RefreshShownValue();
    }


    private void OnModelChanged(int index)
    {
        if (modelOptions == null ||
            index < 0 ||
            index >= modelOptions.Count)
        {
            return;
        }

        string selectedModelId =
            modelOptions[index].modelId;

        PlayerPrefs.SetString(
            SelectedModelKey,
            selectedModelId);

        PlayerPrefs.Save();

        Debug.Log(
            "Dialogue model selected: " +
            selectedModelId);
    }


    public static string GetSelectedModelId()
    {
        return PlayerPrefs.GetString(
            SelectedModelKey,
            "gpt-5.6-terra");
    }
}