using NaughtyAttributes;
using System;
using UnityEngine;

public class TreeManager : MonoBehaviour, IDeformable
{
    public Material stackMaterial;
    [Tag]
    public string treeTag;
    
    public IRenderer CurrentRenderer()
    {
        return new Temp();
    }

    public void Deform()
    {

    }

    public Material GetStackMaterial()
    {
        return stackMaterial; 
    }

    public void IncreaseDeformArea(float value)
    {
    
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(treeTag))
        {
            other.GetComponent<Tree>().Fall(this);
        }
    }

}
