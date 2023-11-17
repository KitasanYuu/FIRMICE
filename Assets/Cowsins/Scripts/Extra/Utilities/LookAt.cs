/// <summary>
/// This script belongs to cowsins™ as a part of the cowsins´ FPS Engine. All rights reserved. 
/// </summary>
using UnityEngine;
namespace cowsins {
public class LookAt : MonoBehaviour
{
    private Transform Player;

    private void Start() => Player = GameObject.FindGameObjectWithTag("Player").transform; 

    private void Update() => transform.LookAt(new Vector3(Player.position.x,transform.position.y, Player.position.z));   
}
}