using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneResetter : MonoBehaviour
{
    public GameObject m_UiObject;  
    private bool isTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))  
        {
            CarController carController = other.GetComponent<CarController>();
            if (carController != null)
            {
                carController.isDisabled = true; 
            }
            m_UiObject.SetActive(true);
            isTriggered = true;
            StartCoroutine(ResetScene());
        }
    }

    IEnumerator ResetScene()
    {
        yield return new WaitForSeconds(2);
        if (isTriggered)  
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);  
        }
    }
}
