using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardMovement : MonoBehaviour
{
    private Vector3 destination;
    private const float TIME_TO_REACH_TARGET = 0.5f;
    private float t = 0;
    private const float MAX_TIME = 2f;
    public void SetDestinationPos(Vector3 destination)
    {
        this.destination = destination;
        StartCoroutine("MoveToPos");
    }
    IEnumerator MoveToPos()
    {
        float start_time = Time.time;
        t += Time.deltaTime / TIME_TO_REACH_TARGET;
        float tmp_time = 0;
        while (transform.position != destination)
        {
            if (tmp_time > MAX_TIME)
                break;

            tmp_time += Time.deltaTime;
            float x = Mathf.MoveTowards(transform.position.x, destination.x, t);
            float z = Mathf.MoveTowards(transform.position.z, destination.z, t);
            float y = 3 * Mathf.Sin(Mathf.PI * ((Time.time - start_time) / TIME_TO_REACH_TARGET));
            transform.position = new Vector3(x, y, z);
            yield return null;
        }

        while(transform.position.y != destination.y)
        {
            float y = Mathf.MoveTowards(transform.position.y, destination.y, t);
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            yield return null;
        }
        transform.position = destination;
    }
}
