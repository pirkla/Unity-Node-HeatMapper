using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HeatMapperDemo
{
    public class SimpleMovement : MonoBehaviour
    {
        public float speed = 10f;
        // Update is called once per frame
        void Update()
        {
            var x = Input.GetAxis("Horizontal") * Time.deltaTime * 150.0f;
            var z = Input.GetAxis("Vertical") * Time.deltaTime * speed;

            transform.Rotate(0, x, 0);
            transform.Translate(0, 0, z);
        }
    }
}