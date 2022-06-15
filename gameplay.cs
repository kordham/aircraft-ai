using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameplay : MonoBehaviour
{
    public GameObject scene_orig;
    public int num_clones_row;
    // Start is called before the first frame update
    void Start()
    {
        create_clones(num_clones_row);
    }

    void create_clones(int num)
    {
        for (int j = 0; j < num; j++)
        {
           GameObject scene_clone = Instantiate(scene_orig, new Vector3(2000 * j, 0,0 ), scene_orig.transform.rotation);
        }
    }
}
