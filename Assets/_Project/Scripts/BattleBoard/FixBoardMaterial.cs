using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class FixBoardMaterial : MonoBehaviour
{
    public Texture2D texture;

    void Start()
    {
        if (texture == null)
        {
            texture = GetComponent<Renderer>().sharedMaterial.mainTexture as Texture2D;
        }

        if (texture != null)
        {
            float aspect = (float)texture.width / texture.height;

            Material mat = GetComponent<Renderer>().material;

            mat.mainTextureScale = new Vector2(aspect, 1f);

            Debug.Log($"Board ajustado para aspect {aspect}, sem mexer no Transform.");
        }
        else
        {
            Debug.LogError("Nenhuma textura encontrada no material!");
        }
    }
}
