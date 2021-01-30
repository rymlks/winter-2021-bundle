using UnityEngine;
using UnityEngine.UI;

public class FadeTheButton : MonoBehaviour
{
    private Image Image;
    private Color color;
    private bool fadingTheImage;
    private void Awake() {
        Image = this.GetComponent<Image>();
        color = Image.color;
    }

    public void FadeTheImage() {
        fadingTheImage = !fadingTheImage;

        if (fadingTheImage) {
            color.a = 0.5f; //if fadingTheImage == true then set the opacity to half
        } else {
            color.a = 1f; //else the opacity is full
        }

        Image.color = color; //set the color on the Image to the color modified in this script.
    }
}
