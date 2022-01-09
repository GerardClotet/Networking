using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenAnimationMovement : MonoBehaviour
{
    private Vector3 destination; 
    private Animator anim;
    private float time_to_reach_target = 0.4f;
    float t =0;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    public void SetDestPos(Vector3 dst)
    {
        t = 0;
        destination = dst;
        StartMovement();
    }

    public void SetIdleAnimation(bool state)
    {
        anim.SetBool("start", state);
    }

    public void StartMovement()
    {
        anim.SetBool("jump", true);
        StartCoroutine("MoveToPos");
        //anim.SetBool("jump",false);
    }

    IEnumerator MoveToPos()
    {
        float start_time = Time.time;
        t += Time.deltaTime / time_to_reach_target;

        while (transform.position.x != destination.x || transform.position.z != destination.z || transform.position.y <= destination.y)
        {
            float x = Mathf.MoveTowards(transform.position.x, destination.x, t);
            float z = Mathf.MoveTowards(transform.position.z, destination.z, t);
            float y = 3 * Mathf.Sin(Mathf.PI * ((Time.time - start_time) / time_to_reach_target));
            transform.position = new Vector3(x,y, z);
            yield return null;
        }
        transform.position = new Vector3(transform.position.x,destination.y,transform.position.z);
        anim.SetBool("jump", false);
    }


}
