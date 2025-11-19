using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChangeMachineButton : MonoBehaviour
{

    int index = 0;
    int prevIndex;
    public GameObject[] machines;

    void Start()
    {
        machines[0].SetActive(true);
    }

    public void UpMachine()
    {
        prevIndex = index;

        index++;
        if (index == machines.Count())
        {
            index = 0;
        }

        Active();

    }

    public void DownMachine()
    {
        prevIndex = index;

        index--;
        if (index < 0)
        {
            index = machines.Count() - 1;
        }

        Active();

    }

    void Active()
    {
        machines[index].SetActive(true);
        machines[prevIndex].SetActive(false);
    } 

}
