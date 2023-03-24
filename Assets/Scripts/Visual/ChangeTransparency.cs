using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeTransparency : MonoBehaviour
{
    public Material TargetMaterial;
    public Material original;
    public MeshRenderer Mrenderer;

    private void Start()
    {
        Mrenderer = GetComponent<MeshRenderer>();
        original = Mrenderer.material;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Mrenderer.material = TargetMaterial;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Mrenderer.material = original;
        }
    }
}
