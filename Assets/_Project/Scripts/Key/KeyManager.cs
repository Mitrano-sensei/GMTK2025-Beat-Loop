using UnityEngine;
using UnityEngine.Serialization;

public class KeyManager : MonoBehaviour
{
    [SerializeField] GameObject _lock;
    [SerializeField] GameObject key;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            key.gameObject.SetActive(false);
            _lock.gameObject.SetActive(false);
        }
    }
}
