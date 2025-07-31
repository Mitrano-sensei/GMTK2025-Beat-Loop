using UnityEngine;

public class KeyManager : MonoBehaviour
{
    [SerializeField] GameObject _player;
    [SerializeField] GameObject _lock;
    [SerializeField] GameObject _key;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.Space))
        {
            _key.gameObject.SetActive(false);
            _lock.gameObject.SetActive(false);
        }
    }
}
