using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    public static SkinSelector instance;

    private void Awake()
    {
        instance = this;
    }

    public Renderer skinImage;
    public Material[] allSkins;
    public GameObject playerModel;
    public Button nextSkinBtn;
    public Button previousSkinBtn;

    //private int skinIndex = 0;
    public int SkinIndex { get; private set; }

    void Start()
    {
        SkinIndex = 0;
        // Set starting Skin selection
        skinImage.material = allSkins[SkinIndex];
    }

    public void NextSkin()
    {

        SkinIndex++;
        Debug.Log($"NextSkin, index {SkinIndex}");

        if (SkinIndex > allSkins.Length - 1)
        {
            SkinIndex = 0;
        }

        SetSkin(SkinIndex);
    }

    public void PreviousSkin()
    {
        SkinIndex--;
        Debug.Log($"PrevSkin, index {SkinIndex}");

        if (SkinIndex < 0)
        {
            SkinIndex = allSkins.Length - 1;
        }

        SetSkin(SkinIndex);
    }

    void SetSkin(int index)
    {
        Debug.Log($"Setting : {SkinIndex}");
        playerModel.GetComponent<Renderer>().material = allSkins[index];
    }
}
