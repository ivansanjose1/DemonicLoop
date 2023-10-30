using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyBoard_Controls : MonoBehaviour
{   
    float Jspeed;

    Player_Move player_move;

    // Start is called before the first frame update
    void Start()
    {
        player_move = GetComponent<Player_Move>();

        Jspeed = player_move.JSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 speedV = player_move.SpeedV;

        // Cuando se pulsa el espacio el juagador salta.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            speedV.y = 0; speedV.y = Jspeed;

            player_move.SpeedV = speedV;
        }
    }
}
