using UnityEngine;

public class StudentsManager : MonoBehaviour {
    const int MAX_STUDENTS = 10;

    public GameObject studentPrefab;

    private GameObject[] students;

	void Start () {
        students = new GameObject[MAX_STUDENTS];
	}
	
	void Update () {
        for (int i = 0; i < MAX_STUDENTS; i++) {
            if (shouldExist(i)) {
                if (students[i] == null) {
                    students[i] = Instantiate(studentPrefab);
                    students[i].GetComponent<Student>().setID(i);
                }
            } else {
                if (students[i] != null) {
                    Destroy(students[i]);
                    students[i] = null;
                }
            }
        }
	}

    private bool shouldExist(int id) {
        Vector3 pos;
        Vector3 rot;
        return Tracking.getTransform(id, out pos, out rot);
    }
}
