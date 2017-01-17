using UnityEngine;
using UnityEngine.UI;

public class Student : MonoBehaviour {
    public GameObject face;
    private int id;
	
	void Update () {
        dealTransform();
        dealImage();
	}

    public void setID(int id) {
        this.id = id;
    }

    private void dealTransform() {
        Vector3 pos;
        Vector3 rot;
        if (Tracking.getTransform(id, out pos, out rot)) {
            transform.position = pos;
            transform.eulerAngles = rot;
        }
    }

    private void dealImage() {
        Texture2D texture;
        if (Capturing.getFrame(id, out texture)) {
            Destroy(face.GetComponent<RawImage>().texture);
            face.GetComponent<RawImage>().texture = texture;
        }
    }
}
