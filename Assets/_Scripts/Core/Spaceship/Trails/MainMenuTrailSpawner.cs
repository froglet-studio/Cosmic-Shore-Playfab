﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuTrailSpawner : MonoBehaviour
{
    public GameObject trail;
    public Transform head;
    public float offset = 1.5f;
    public float tailPeriod = .1f;
    public float lifeTime = 20;
    public float waitTime = .5f;

    [SerializeField]
    GameObject TailContainer;

    public bool useRandom = true;

    Vector3 randomScale;

    bool hasTail = true;
    private IEnumerator trailCoroutine;

    IEnumerator SpawnTrailCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(tailPeriod);
            var trailCopy = Instantiate<GameObject>(trail);

            trailCopy.transform.position = head.transform.position - head.transform.forward*offset;
            trailCopy.transform.rotation = head.transform.rotation;
            trailCopy.transform.localScale = new Vector3(randomScale.x,randomScale.y,randomScale.z);

            MainMenuTrail trailScript = trailCopy.GetComponent<MainMenuTrail>();
            trailScript.lifeTime = lifeTime;
            trailScript.waitTime = waitTime;


            trailCopy.transform.parent = TailContainer.transform;
        }
    }


    // Start is called before the first frame update
    void Start()
    {   
        if (useRandom == true)
        {
            randomScale = new Vector3(Random.Range(3, 50), Random.Range(.5f, 4), Random.Range(.5f, 2));
        }
        else { randomScale = new Vector3(3,.03f,.3f); }
        if (trailCoroutine != null)
        {
            StopCoroutine(trailCoroutine);
        }
        trailCoroutine = SpawnTrailCoroutine();
        StartCoroutine(trailCoroutine);

    }

    //// Update is called once per frame
    //void Update()
    //{
        
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        if (hasTail)
    //        {
    //            StopCoroutine(trailCoroutine);
    //            hasTail = false;
    //        }
    //        else if (!hasTail)
    //        {
    //            StartCoroutine(trailCoroutine);
    //            hasTail = true;
    //        }
            
    //    }
        
    //}
}