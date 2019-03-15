﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistrotObject : MonoBehaviour {
    bool _isDistorting = false;
    float _scaleRatio = 1f;

    public void StartDistort()
    {
        if (!_isDistorting)
            StartCoroutine(DistrotSelf());
    }

    protected IEnumerator DistrotSelf()
    {
        _isDistorting = true;

        float startTime = Time.time;
        float timeChecker = 0;
        Vector3 origin = transform.localScale;
        // 크게
        while (timeChecker <= 0.05f)
        {
            timeChecker = Time.time - startTime;
            transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime * _scaleRatio,
                transform.localScale.y + Time.deltaTime * _scaleRatio,
                transform.localScale.z + Time.deltaTime * _scaleRatio);
            yield return null;
        }
        startTime = Time.time;
        timeChecker = 0;
        // 작게
        while (timeChecker <= 0.1f)
        {
            timeChecker = Time.time - startTime;
            transform.localScale = new Vector3(transform.localScale.x - Time.deltaTime * _scaleRatio,
                transform.localScale.y - Time.deltaTime * _scaleRatio,
                transform.localScale.z - Time.deltaTime * _scaleRatio);
            yield return null;
        }
        startTime = Time.time;
        timeChecker = 0;
        // 크게
        while (timeChecker <= 0.05f)
        {
            timeChecker = Time.time - startTime;
            transform.localScale = new Vector3(transform.localScale.x + Time.deltaTime * _scaleRatio,
                transform.localScale.y + Time.deltaTime * _scaleRatio,
                transform.localScale.z + Time.deltaTime * _scaleRatio);
            yield return null;
        }

        transform.localScale = origin;
        _isDistorting = false;
    }

}