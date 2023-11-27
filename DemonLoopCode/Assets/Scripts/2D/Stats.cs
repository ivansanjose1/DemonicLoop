using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class Stats : MonoBehaviour
{
    private Image barHP;

    private Image barMana;

    private GameObject charFloatingTextSpaceNumbers;

    [SerializeField] float health;
    [SerializeField] float maxHealth = 100f;
    [SerializeField] float mana;
    [SerializeField] float maxMana = 100f;
    [SerializeField] float strength = 15f;
    [SerializeField] float physicalDef = 12f;
    [SerializeField] float magicAtk = 0f;
    [SerializeField] float magicDef = 0f;
    [SerializeField] float criticalChance;
    [SerializeField] float moneyDrop = 1.1f;
    [SerializeField] List<AttackData> listAtk = new();

    [SerializeField] Types type;

    public float Health { get { return health; } set { health = value; OnAttackReceived(); } }
    public float Mana { get { return mana; } set { mana = value; OnManaChanged(); } }
    public float Strenght { get { return strength; } }
    public float PhysicalDefense { get { return physicalDef; } }
    public float MagicAtk { get { return magicAtk; } }
    public float MagicDef { get { return magicDef; } }
    public float CriticalChance { get { return criticalChance; } }
    public float MoneyDrop { get { return moneyDrop; } }
    public List<string> ListAtk { get { return ObtainNameAttacks(); }}
    public GameObject CharFloatingTextSpaceNumbers { get { return charFloatingTextSpaceNumbers; } }

    public Types Type { get { return type; } }

    void Start()
    {
        health = maxHealth;
        barHP = transform.GetChild(0).Find("BarHPFill").GetComponent<Image>();

        mana = maxMana;
        barMana = transform.GetChild(1).Find("BarManaFill").GetComponent<Image>();

        charFloatingTextSpaceNumbers = transform.GetChild(2).gameObject;
    }

    // Si en el caso de de que el jugador tenga mas ataques no podra usarlo
    // Solo puede usar 4 ataques que son los espacios acordados
    private void CheckListAtkMax()
    {
        if (listAtk.Count > 4)
        {
            listAtk.Remove(listAtk[^1]); // == listAtk.Count - 1
        }
    }//Fin de CheckListAtk

    private void OnAttackReceived()
    {
        if (health >= maxHealth)
        {
            health = maxHealth;
        }

        if (health <= 0)
        {
            health = 0;
        }

        barHP.fillAmount = health / maxHealth;

        if (health == 0)
        {
            gameObject.SetActive(false);
        }
    }//Fin de OnAttackReceived

    private void OnManaChanged()
    {
        if(mana >= maxMana)
        {
            mana = maxMana;
        }

        if(mana <= 0)
        {
            mana = 0;
        }

        barMana.fillAmount = mana / maxMana;
    }

    private List<string> ObtainNameAttacks()
    {
        List<string> nameList = new();

        listAtk.ForEach(x => {
            if(x != null) 
                nameList.Add(x.name.Substring(4, x.name.Length - 4).Replace("^", " ")); 
            });

        return nameList;
    }

    // TODO: Funcion para agregar un ataque a la lista pasandole un nombre, hace falta revisarlo.
    public void SetAttack(string attack)
    {   
        string path = AssetDatabase.GUIDToAssetPath(attack);

        ScriptableObject @object = AssetDatabase.LoadAssetAtPath<AttackData>(path);

        CheckListAtkMax(); // Aqui se realizará la comprobacion del max de ataques por personaje.

        listAtk.Add(@object as AttackData);
    }
}
