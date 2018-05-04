using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundImageSetter : MonoBehaviour
{
    public static BackgroundImageSetter Instance { get; private set; }
    [SerializeField] private Image _image;
    public IEnumerator SetBackgroundImagePath(string path)
    {
        Texture2D texture = new Texture2D(0, 0, TextureFormat.DXT1, false);
        using (WWW www = new WWW("file:///" + path))
        {
            yield return www;
            www.LoadImageIntoTexture(texture);
            Sprite sprite = Sprite.Create(texture, new Rect(0.0f, 0.0f, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            _image.sprite = sprite;
            _image.type = Image.Type.Simple;
            _image.preserveAspect = true;
        }
    }
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(this);
            Debug.LogError("Error: Unexpected multiple instances of BackgroundImageSetter");
        }
    }
}
