using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class OutlineObject : MonoBehaviour
{
    // public Material MaskMaterial;
    public Material FillMaterial;

    private Material thisMaskMaterial;
    [HideInInspector] public Material thisFillMaterial;

    private Material[] objectsMaterial;
    private Color initColor;
    private Color InvisColor;
    private int materialInstance;
    public static OutlineObject Instance;
    public bool isOutlined;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        if (Instance != this)
            Destroy(this);


        thisFillMaterial = Instantiate(FillMaterial);
        thisFillMaterial.name = "FillMaterial instance";
    }

    public void ApplyOutline(Renderer[] renderers)
    {
        if (isOutlined) return;
        foreach (var renderer in renderers)
        {
            objectsMaterial = renderer.sharedMaterials;
            List<Material> materials = renderer.sharedMaterials.ToList();
            materials.Add(thisFillMaterial);

            renderer.materials = materials.ToArray();
        }
        isOutlined = true;
    }

    public void DisableOutline(Renderer[] renderers)
    {
        foreach (var renderer in renderers)
        {
            renderer.materials = objectsMaterial;
        }
        isOutlined = false;
    }
}