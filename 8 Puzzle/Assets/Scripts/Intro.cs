
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Intro : MonoBehaviour {


    float time = 14.2f;

    void Start() {

    }

    // Update is called once per frame
    void Update() {
        time -= Time.deltaTime;
        if (time <= 0 || Input.GetKeyDown(KeyCode.Space))
            SceneManager.LoadScene("Main");
    }
}
