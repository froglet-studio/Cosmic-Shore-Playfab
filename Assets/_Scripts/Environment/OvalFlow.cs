﻿using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "OvalFlowData", menuName = "ScriptableObjects/OvalFlow", order = 2)]
[System.Serializable] public class OvalFlow : FlowFieldSO
{

    public OvalFlow()
    {
        fieldThickness = 200;
        fieldWidth = 200;
        fieldHeight = 700;
        fieldMax = .7f;
        
    }

    override public Vector3 FlowVector(Transform node)
    {
        //float AspectRatio = (float)fieldHeight / (float)fieldWidth;
        float StraightLength = fieldWidth - fieldHeight;

        if (node.position.x > (float)StraightLength / 2f)
        {
            float Pr = Mathf.Sqrt(Mathf.Pow(node.position.x, 2) + Mathf.Pow(node.position.y, 2)); //divide y by aspect to fit curve in bounding box
            float Ptheta = Mathf.Atan2(node.position.y, node.position.x); //divide y by aspect twice to get the flow pointed with the tangent.

            float Vr = Gaussian(Pr + StraightLength, fieldThickness, (fieldWidth - 3 * fieldThickness));
            //float Vr = Mathf.Exp(-Mathf.Pow(Pr - (fieldWidth - twoSigma), 2) / twoSigmaSquared);
            float Vtheta = Ptheta + Mathf.PI / 2;

            float Zdecay = Gaussian(node.position.z, fieldThickness, 0);

            return new Vector3(Vr * Mathf.Cos(Vtheta)
                                    * fieldMax * Zdecay,
                               Vr * Mathf.Sin(Vtheta)
                                    * fieldMax * Zdecay,
                               0);
        }
        else return new Vector3(Gaussian(node.position.y,fieldThickness,fieldHeight-(2*fieldThickness)),0,0);
    }

    float Gaussian(float input, float sigma, float distance)
    {
        float twoSigmaSquared = 2 * Mathf.Pow(sigma, 2);
        return Mathf.Exp(-Mathf.Pow(input - distance, 2) / twoSigmaSquared);
    }


}
