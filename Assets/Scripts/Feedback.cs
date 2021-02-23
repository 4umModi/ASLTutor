using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Feedback : MonoBehaviour
{ 
    // Start is called before the first frame update
    void Start()
    {
        int y = 0;
    }

    // Update is called once per frame
    void Update()
    {
        int x = 1;
    }

    //prints out featureID to unity console
    public void printFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(",", featureIDs);
        Debug.Log(featureIDString);
    }

    public void getFeatureIDs(List<int> staticFeatureIDs, List<int> dynamicFeatureIDs)
    {
        List<int> featureIDs = staticFeatureIDs;
        featureIDs.AddRange(dynamicFeatureIDs);
        Debug.Log("In Feedback:");
        printFeatureID(featureIDs);

    }
}
