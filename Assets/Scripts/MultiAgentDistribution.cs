using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiAgentDistribution : MonoBehaviour
{
    public GameObject prefab; // El objeto que se va a instanciar
    public int width = 12; // El número de objetos en el eje X
    public int height = 12; // El número de objetos en el eje Z
    public float spacing = 12.0f; // La distancia entre los objetos

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                // Calcula la posición del nuevo objeto
                Vector3 position = new Vector3(x * spacing, 0, z * spacing);

                // Crea una nueva instancia del prefab en la posición calculada
                Instantiate(prefab, position, Quaternion.identity);
            }
        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}
