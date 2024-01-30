using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public enum ObjectTypes { Health, Mana, HealState, Throwable, Revive }

[CreateAssetMenu]
public class ObjectData : ScriptableObject
{
    private FloatingTextCombat floatingText;
    [SerializeField] private Sprite icon;

    [TextArea]
    [SerializeField] private string description;
    [SerializeField] private float cost;
    [SerializeField] private ObjectTypes objectType;
    [SerializeField] private float baseNum;
    [SerializeField] private StateData stateAsociated;
    [SerializeField] bool alliesTargets = false;
    [SerializeField] Types type;
    [SerializeField] GameObject buttonPrefab;

    float baseDamage = 1.5f;

    EnterBattle enterBattle;
    PlayerInventory inventory;

    public Sprite Icon { get { return icon; } }
    public string Description { get { return description; } }
    public float Cost { get { return cost; } }
    public ObjectTypes ObjectType { get { return objectType; } }
    public Types Type { get { return type; } }
    public float BaseNum { get { return baseNum; } }

    public float BaseDamage { get { return baseDamage; } }

    // Cuando se hace click en este objeto.
    public void Click(PlayerInventory inventory)
    {
        floatingText = GameObject.Find("System").GetComponent<FloatingTextCombat>();

        this.inventory = inventory;
        enterBattle = GameObject.Find("System").GetComponent<EnterBattle>();
        if (enterBattle.OneTime)
        {
            if(objectType == ObjectTypes.Revive)
            {
                GameObject.Find("System").GetComponent<CombatFlow>().GeneratePlayersDefeatedButtons(this);
            } else GameObject.Find("System").GetComponent<CombatFlow>().GenerateTargetsButtons(alliesTargets, this);
        }
        else
        {
            GameObject.Find("Inventory").transform.GetChild(1).gameObject.SetActive(true);

            GameObject[] buttons = GameObject.FindGameObjectsWithTag("Buttons");

            if (buttons.Length > 0)
            {
                foreach (GameObject bt in buttons)
                {
                    Destroy(bt);
                }
            }

            CreateButtons(GameObject.Find("PartyButtons"));
        }
    }

    void CreateButtons(GameObject spawnMoveBT)
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player").ToArray();

        foreach (GameObject pl in players)
        {
            Debug.Log(spawnMoveBT);

            GameObject bt = Instantiate(buttonPrefab, spawnMoveBT.transform.position, Quaternion.identity);
            bt.transform.SetParent(spawnMoveBT.transform);
            bt.name = "Ally " + pl.name;//Nombre de los botones que se van a generar
            bt.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = pl.name.Substring(1, pl.name.Length - 1);
            bt.GetComponent<Button>().onClick.AddListener(delegate { UserObject(pl); });
        }
    }

    public void UserObject(GameObject @character)
    {
        if (enterBattle.OneTime)
            GameObject.Find("System").GetComponent<CombatFlow>().InventoryTurn();
        else
            GameObject.Find("Inventory").transform.GetChild(1).gameObject.SetActive(false);
        
        Stats target = @character.GetComponent<Stats>();

        switch (ObjectType)
        {
            case ObjectTypes.Health:
                //Debug.Log("Pocion de Cura");
                floatingText.ShowFloatingTextNumbers(@character, BaseNum, Color.green);
                target.Health += BaseNum;
                break;

            case ObjectTypes.Mana:
                //Debug.Log("Pocion de Mana");
                floatingText.ShowFloatingTextNumbers(@character, BaseNum, Color.blue);
                target.Mana += BaseNum;
                break;

            case ObjectTypes.HealState:
                GameObject.Find("System").GetComponent<LibraryStates>().RemoveCharacterWithState(@character, ObtainStateName());
                break;
            
            case ObjectTypes.Revive:
                target.Revive(baseNum);
                floatingText.ShowFloatingText(character, "Revived", Color.yellow);
                GameObject.Find("System").GetComponent<CombatFlow>().CheckIfAnAllyHasRevived();

                break;

            case ObjectTypes.Throwable:
                //En este caso le tiramos algo al enemigo
                switch (Type)
                {
                    case Types.Fire:
                        if (target.Type == Types.Plant)
                        {
                            target.Health -= BaseNum * baseDamage;
                        }
                        break;

                    case Types.Plant:
                        if (target.Type == Types.Water)
                        {
                            target.Health -= BaseNum * baseDamage;
                        }
                        break;

                    case Types.Water:
                        if (target.Type == Types.Fire)
                        {
                            target.Health -= BaseNum * baseDamage;
                        }
                        break;

                    case Types.Light:
                        if (target.Type == Types.Darkness)
                        {
                            target.Health -= BaseNum * baseDamage;
                        }
                        break;

                    case Types.Darkness:
                        if (target.Type == Types.Light)
                        {
                            target.Health -= BaseNum * baseDamage;
                        }
                        break;
                }

                if(target.Health <= 0)
                {
                    if(target.gameObject.CompareTag("Enemy")) GameObject.Find("System").GetComponent<CombatFlow>().DeleteEnemyFromList(target.gameObject);
                    else GameObject.Find("System").GetComponent<CombatFlow>().DeleteAllieFromArray(target.gameObject);
                }
                
                break;

        }
        inventory.RemoveObjectFromInventory(name.Substring(4, name.Length - 4));
    }//Fin de UserObject

    private string ObtainStateName()
    {
        return stateAsociated.name.Substring(4, stateAsociated.name.Length - 4).ToUpper();
    }
}