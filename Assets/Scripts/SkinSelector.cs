using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinSelector : MonoBehaviour
{
    public Renderer skinImage;
    public Material[] allSkins;
    public GameObject playerModel;
    public Button nextSkinBtn;
    public Button previousSkinBtn;

    private int skinIndex = 0;

    void Start()
    {
        // Set starting Skin selection
        skinImage.material = allSkins[skinIndex];
    }

    public void NextSkin()
    {

        skinIndex++;
        Debug.Log($"NextSkin, index {skinIndex}");

        if (skinIndex > allSkins.Length - 1)
        {
            skinIndex = 0;
        }

        SetSkin(skinIndex);
    }

    public void PreviousSkin()
    {
        skinIndex--;
        Debug.Log($"PrevSkin, index {skinIndex}");

        if (skinIndex < 0)
        {
            skinIndex = allSkins.Length - 1;
        }

        SetSkin(skinIndex);
    }

    void SetSkin(int index)
    {
        Debug.Log($"Setting : {skinIndex}");
        playerModel.GetComponent<Renderer>().material = allSkins[index];
    }
}
