using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct next_y
{
    public float y_to;
    public float time_to;

    public next_y(float y, float time)
    {
        y_to = y;
        time_to = time;
    }
}



public class CameraFollowing : MonoBehaviour
{
    public GameObject player;
    [SerializeField] public float angle_on_player_delta = 0.05f;
    private next_y next_y_pos;
    private float y_range;
    private float dist_to_player;
    private float angle_on_player = 0f;
    private float y_delta;

    void Start()
    {
        next_y_pos = new next_y(transform.position.y, 0f);
        y_range = 4 * (transform.position.y);
        Vector3 vect = new Vector3(transform.position.x, 0, transform.position.z);
        dist_to_player = vect.magnitude;
    }

    void Update()
    {
        if (next_y_pos.y_to - 0.5f < transform.position.y && transform.position.y < next_y_pos.y_to + 0.5f)
        {
            next_y_pos = new next_y(Random.Range(0f, y_range), Random.Range(2f, 5f));
            y_delta = (next_y_pos.y_to - transform.position.y) / (next_y_pos.time_to / Time.deltaTime);
            Debug.Log(y_delta);
        }

        Vector3 next_camera_pos;
        next_camera_pos.x = player.transform.position.x + Mathf.Cos(Mathf.PI / 180.0f * angle_on_player) * dist_to_player;
        next_camera_pos.y = transform.position.y;// + y_delta;
        next_camera_pos.z = player.transform.position.z + Mathf.Sin(Mathf.PI / 180.0f * angle_on_player) * dist_to_player;
        transform.position = next_camera_pos;
        //angle_on_player += angle_on_player_delta;
        transform.LookAt(player.transform.position);
    }
}
