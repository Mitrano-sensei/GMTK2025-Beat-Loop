using UnityEngine;
using UnityEngine.Tilemaps;

public class MovingFloor : MonoBehaviour
{
    [SerializeField] private Tilemap _GroundTileMap;
    private int _moved = 0;
    

    void MovingTiles()
    {
        if (_moved != 3)
        {
            _moved += 1;
            _GroundTileMap.transform.position += (Vector3.left / 2);
        }
        else
        {
            _moved -= 3;
            _GroundTileMap.transform.position += (Vector3.right) * 1.5f;
        }
    }


    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            MovingTiles();
        }
    }            

}
