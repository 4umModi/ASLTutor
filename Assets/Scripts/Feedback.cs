using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script will give feedback based upon the featureIDs expected and the ones present in the users recorded signing
public class Feedback : MonoBehaviour
{ 

    //prints out featureID to unity console
    public void printFeatureID(List<int> featureIDs)
    {
        string featureIDString = string.Join(",", featureIDs);
        Debug.Log(featureIDString);
    }

    //gets the feature IDs and other information from classifier script
    public void getFeatureIDs(List<int> staticFeatureIDs, List<int> dynamicFeatureIDs, string name, bool isDynamic, List<int> expectedFeatureIDs)
    {
        List<int> featureIDs = staticFeatureIDs;
        featureIDs.AddRange(dynamicFeatureIDs);
        Debug.Log("In Feedback:");
        printFeatureID(featureIDs);
        Debug.Log("Expected");
        printFeatureID(expectedFeatureIDs);


    }
}
