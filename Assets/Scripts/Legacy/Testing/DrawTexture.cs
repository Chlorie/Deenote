using System.IO;
using UnityEngine;

public class DrawTexture : MonoBehaviour
{
    private void Start()
    {
        Texture2D texture = new Texture2D(100, 100);

        for (int i = 0; i < 100; i++)
            for (int j = 0; j < 100; j++)
            {
                float ii = i - 50, jj = j - 50;
                float r = Mathf.Sqrt(ii * ii + jj * jj);
                if (r < 45) texture.SetPixel(i, j, Color.white);
                else if (r < 50) texture.SetPixel(i, j, new Color(1.0f, 1.0f, 1.0f, (50 - r) / 5.0f));
                else texture.SetPixel(i, j, new Color(0, 0, 0, 0));
            }

        byte[] png;
        png = texture.EncodeToPNG(); File.WriteAllBytes("NewTexture.png", png);
        Debug.Log("Texture generated");
    }
}
