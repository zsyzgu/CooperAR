using UnityEngine;

public class Student : MonoBehaviour {
    private int id;

	void Start () {
		
	}
	
	void Update () {
        dealTransform();
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
}
