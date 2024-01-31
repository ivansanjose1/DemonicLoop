using UnityEngine;

public class KeyBoardControls : MonoBehaviour
{
    float Jspeed;

    PlayerMove player_move;
    PlayerInventory player_inventory;
    EnterBattle enterBattle;
    [SerializeField] ShoppingSystem shopping;

    public ShoppingSystem Shopping { set { shopping = value; } }

    // Start is called before the first frame update
    void Start()
    {
        player_move = GetComponent<PlayerMove>();
        player_inventory = GameObject.Find("System").GetComponent<PlayerInventory>();
        enterBattle = GameObject.Find("System").GetComponent<EnterBattle>();

        Jspeed = player_move.JSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 speedV = player_move.SpeedV;

        // Cuando se pulsa el espacio el jugador salta.
        if (Input.GetKey(KeyCode.Space) && player_move.OnFloor)
        {
            speedV.y = 0; speedV.y = Jspeed;
            player_move.SpeedV = speedV;
        }

        // Cambio de velocidades al moverse para simular caminar y correr.
        player_move.Speed = Input.GetKey(KeyCode.LeftShift);

        // Abrir y cerrar el inventario solo cuando el jugador no se encuentre en batalla.
        if (Input.GetKeyDown(KeyCode.Escape) && !enterBattle.OneTime && !player_inventory.DontOpenInventory)
        {
            player_inventory.OpenCloseInventory();
            Debug.Log("Inventario");
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && player_inventory.DontOpenInventory)
            shopping.OpenCloseShop();

        if (Input.GetKeyDown(KeyCode.Mouse0) && !player_inventory.InventoryState)
            transform.GetComponentInChildren<PlayerInteract>().Click = true; 
    }
}
