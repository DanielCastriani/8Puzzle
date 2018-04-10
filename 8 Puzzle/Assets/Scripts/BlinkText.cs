using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlinkText : MonoBehaviour {
    public Text tx;
    // Use this for initialization
    void Start() {
        StartCoroutine(blink());
    }
    private IEnumerator blink() {
        while (true) {
            tx.enabled = true;
            yield return new WaitForSeconds(1f);
            tx.enabled = false;
            yield return new WaitForSeconds(0.5f);
        }
    }

        // Update is called once per frame
        void Update() {


        }
    }
