using System;
using UnityEngine;

[DisallowMultipleComponent]
public class EnvironmentVariantGroup : MonoBehaviour
{
    [Serializable]
    private class EnvironmentVariant
    {
        [SerializeField] private string variantId;
        [SerializeField] private GameObject variantObject;

        public string VariantId => variantId;
        public GameObject VariantObject => variantObject;
    }

    [Header("Variant Group")]
    [SerializeField] private string groupId;

    [Header("Variants")]
    [SerializeField] private EnvironmentVariant[] variants;

    public string GroupId => groupId;

    public bool SetVariant(string variantId)
    {
        if (variants == null || variants.Length == 0)
        {
            Debug.LogError(
                $"EnvironmentVariantGroup \"{groupId}\" on \"{gameObject.name}\" has no variants configured."
            );

            return false;
        }

        bool foundVariant = false;

        foreach (EnvironmentVariant variant in variants)
        {
            if (variant == null || variant.VariantObject == null)
            {
                continue;
            }

            bool shouldEnable = variant.VariantId == variantId;

            variant.VariantObject.SetActive(shouldEnable);

            if (shouldEnable)
            {
                foundVariant = true;
            }
        }

        if (!foundVariant)
        {
            Debug.LogError(
                $"EnvironmentVariantGroup \"{groupId}\" could not find variant \"{variantId}\"."
            );

            return false;
        }

        return true;
    }

    public void SetGroupEnabled(bool enabled)
    {
        if (variants == null)
        {
            return;
        }

        foreach (EnvironmentVariant variant in variants)
        {
            if (variant == null || variant.VariantObject == null)
            {
                continue;
            }

            variant.VariantObject.SetActive(enabled);
        }
    }
}