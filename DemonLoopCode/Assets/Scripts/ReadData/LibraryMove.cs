using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LibraryMove : MonoBehaviour
{
    // Tipos de daño por tipo posibles
    private const float SuperEffective = 1.5F;

    private const float NotVeryEffective = 0.5f;

    private const float NormalEffective = 1f;

    // Daño al tener similitud de tipo respecto ataque y atacante
    private const float SameTypeDamage = 2.5f;

    private const float CriticalDamageMultiplier = 2f;

    private GameObject character;
    private GameObject target;

    private Dictionary<string, AttackData> attackCache = new();

    private FloatingTextCombat floatingText;

    private float spCountBar;

    private void Start()
    {
        LoadAttacks();

        floatingText = GetComponent<FloatingTextCombat>();
    }


    // Función que realiza un movimiento a un solo objetivo.
    public void Library(GameObject character, GameObject target, string movement, LibraryStates statesLibrary, LibraryBattleModifiers battleModifiersLibrary,float spCountBar)
    {
        this.character = character;
        this.target = target;

        Stats target_ST = target.GetComponent<Stats>();
        Stats character_ST = character.GetComponent<Stats>();

        this.spCountBar = spCountBar;

        var attack = CheckAttack(movement);

        var healOrAttack = CheckAttackOrHeal(movement);

        var atkSp=CheckSpeacialAttack(movement);


        if (!healOrAttack)
        {
            if (target_ST.Type == attack.Type && target_ST.AbsorbsDamageOfSameType)
            {
                target_ST.Health += attack.BaseDamage / 2;

                floatingText.ShowFloatingTextNumbers(target, attack.BaseDamage, Color.green);
                floatingText.ShowFloatingText(target, "ABSORBED", Color.yellow);
            }
            else
            {
                float damage = DamageFull(target_ST, character_ST, attack);
                if (atkSp)
                {
                    target_ST.Health -= damage*spCountBar;
                }
                else
                {
                    target_ST.Health -= damage;
                }
            }

        }
        else
        {
            if (attack.BaseDamage != 0)
            {
                if (atkSp)
                {
                    target_ST.Health += attack.BaseDamage*spCountBar;
                }
                else
                {
                    target_ST.Health += attack.BaseDamage;
                }

                    floatingText.ShowFloatingTextNumbers(target, attack.BaseDamage, Color.green);
            }

        }

        if (attack.BattleModifierAsociated != null) battleModifiersLibrary.ActiveBattleModifier(target, attack.BattleModifierAsociated);

        if (attack.GenerateAState != ActionStates.None) StartCoroutine(StateActions(attack, target, statesLibrary));

        if (attack.ManaCost != 0) ManaManager(attack, character_ST);

        character = null; target = null;




    }//Fin de Library


    // Carga inicial de ataques a la "cache"
    private void LoadAttacks()
    {
        AttackData[] attackDatas = Resources.LoadAll<AttackData>("Data/Attacks");

        foreach (AttackData attackData in attackDatas)
        {
            string atkName = attackData.name.Substring(4, attackData.name.Length - 4).Replace("^", " ").ToUpper();

            attackCache.Add(atkName, attackData);

            //Debug.Log("Ataque " + atkName + " | danno base " + (@object as AttackData).BaseDamage + " | LOADED TO CACHE");
        }
    }//Fin de LoadAttacks


    // Se llama para recibir la clase base de ataques. Se obtiene su informacion esencial.
    private AttackData CheckAttack(string movement)
    {
        AttackData attackData;

        if (attackCache.ContainsKey(movement.ToUpper()))
        {
            attackData = attackCache[movement.ToUpper()];

            //Debug.Log("Ataque " + movement.ToUpper() + " | danno base " + attackData.BaseDamage.ToString() + " | CACHE");
        }
        else
        {
            Debug.Log("ATAQUE NO ENCONTRADO, RECURRIENDO A PUNCH");

            attackData = attackCache["PUNCH"];
        }
        return attackData;
    }//Fin de CheckAttack

    // Comprobamos si el ataque es para hacer daño o para curar.
    public bool CheckAttackOrHeal(string movementName)
    {
        bool healing = false;

        var attackInfo = CheckAttack(movementName);

        if (attackInfo.PhyAttack == 0 && attackInfo.MagicAttack == 0)
        {
            healing = true;
        }

        return healing;
    }//Fin de CheckAttackOrHeal

    // Comprobamos si el ataque es AOE o single-target
    public bool CheckAoeAttack(string movementName)
    {
        bool isAOE = false;

        var attackInfo = CheckAttack(movementName);

        if (attackInfo.IsAoeAttack)
        {
            isAOE = true;
        }

        return isAOE;
    }//Fin de CheckAoeAttack


    //Comprueba si es un ATK Special
    public bool CheckSpeacialAttack(string movementName)
    {
        bool isSpecial = false;

        var attackInfo = CheckAttack(movementName);

        if (attackInfo.IsSpecial)
        {
            isSpecial = true;
        }

        return isSpecial;
    }//Fin de CheckSpeacialAttack

    public bool CheckIfAtkSpecialPoints(GameObject characterST, string movementName)
    {
        bool pointSp = false;

        var attackInfo = CheckAttack(movementName);

        if (characterST.GetComponent<Stats>().SP >= attackInfo.PointSpecial)
        {
            pointSp = true;
        }

        return pointSp;
    }

    public bool CheckIfManaIsEnough(GameObject characterST, string movementName)
    {
        bool manaEnough = false;

        var attackInfo = CheckAttack(movementName);

        if (characterST.GetComponent<Stats>().Mana >= attackInfo.ManaCost)
        {
            manaEnough = true;
        }

        return manaEnough;
    }

    // Funcion para ejecutar el ataque que sirve para pasar de turno.
    public void PassTurn(GameObject characterST, string movementName)
    {
        var attackData = CheckAttack(movementName);

        ManaManager(attackData, characterST.GetComponent<Stats>());
    }

    private float DamageFull(Stats target_ST, Stats character_ST, AttackData attack)
    {
        float damage;
        float damagePhyAttack = (character_ST.Strenght * attack.PhyAttack);
        float damageMagic = (character_ST.MagicAtk * attack.MagicAttack);
        float defenseMagic = (target_ST.MagicDef * attack.MagicAttack);
        float defensePhy = (target_ST.PhysicalDefense * attack.PhyAttack);

        float damageType = DamageType(target_ST, attack);

        float damageTypeEnhancer = TypeEnhancer(character_ST, attack);

        float criticalDamage = CriticalDamage(character_ST.CriticalChance);

        //Lo hemos hecho asi para que se vea mejor
        damage = (attack.BaseDamage + damagePhyAttack + damageMagic - defenseMagic - defensePhy);
        damage = (damageType * damage) + (damageTypeEnhancer * damage);
        damage *= criticalDamage;

        if (damage <= 0) damage = 1;

        if (criticalDamage == CriticalDamageMultiplier)
        {
            floatingText.ShowFloatingTextNumbers(target, damage, Color.red);

        }
        else floatingText.ShowFloatingTextNumbers(target, damage, Color.white);


        Debug.Log(character_ST.name + " ataca a " + target_ST.name + " con ataque: " + attack.name + " con un daño de: " + damage);

        if (attack.LifeTheft)
        {
            character_ST.Health += damage;
            floatingText.ShowFloatingTextNumbers(character, damage, Color.green);
        }

        if (attack.ManaTheft)
        {
            character_ST.Mana += damage;
            target_ST.Mana -= damage;
            floatingText.ShowFloatingTextNumbers(character, damage, Color.blue);
            floatingText.ShowFloatingTextNumbers(target, -damage, Color.blue);
        }

        //En el caso de que sea attack.Special (El Special nombre provisional) 
        //Es que el ataque es tan fuerte que te quita hasta vida
        if (attack.Berserker)
        {
            character_ST.Health -= attack.BaseDamage;
            floatingText.ShowFloatingTextNumbers(character, attack.BaseDamage, Color.red);
        }



        return damage;
    }//Fin de DamageFull

    private float DamageType(Stats target_ST, AttackData attack)
    {
        float damageType = NormalEffective;

        switch (attack.Type)
        {
            case Types.Fire:
                if (target_ST.Type == Types.Plant)
                {
                    damageType = SuperEffective;
                }
                if (target_ST.Type == Types.Water)
                {
                    damageType = NotVeryEffective;
                }
                break;

            case Types.Plant:
                if (target_ST.Type == Types.Water)
                {
                    damageType = SuperEffective;
                }
                if (target_ST.Type == Types.Fire)
                {
                    damageType = NotVeryEffective;
                }
                break;

            case Types.Water:
                if (target_ST.Type == Types.Fire)
                {
                    damageType = SuperEffective;
                }
                if (target_ST.Type == Types.Plant)
                {
                    damageType = NotVeryEffective;
                }
                break;

            case Types.Light:
                if (target_ST.Type == Types.Darkness)
                {
                    damageType = SuperEffective;
                }
                break;

            case Types.Darkness:
                if (target_ST.Type == Types.Light)
                {
                    damageType = SuperEffective;
                }
                break;
        }

        return damageType;
    }//Fin de DamageType

    private float TypeEnhancer(Stats character_ST, AttackData attack)
    {
        //Cambiar los datos de los potenciadores de los tipos
        float damage = 0.0f;

        if (character_ST.Type == attack.Type) damage = SameTypeDamage;

        return damage;
    }//Fin de TypeEnhancer

    // Se devuelve un x2 en el daño, si no, se mantiene en 1 y el daño no es modificado.
    private float CriticalDamage(float CriticalStat)
    {
        float damage = NormalEffective;

        if (Random.Range(0, 100f) < CriticalStat) damage = CriticalDamageMultiplier;

        return damage;
    }

    // Funcion para ejecutar las diferentes acciones respecto a los estados.
    private IEnumerator StateActions(AttackData attack, GameObject targetToApplyState, LibraryStates statesLibrary)
    {
        switch (attack.GenerateAState)
        {
            case ActionStates.Inflict:
                if (Random.Range(0, 100) < attack.ProbabilityOfState)
                {
                    if (statesLibrary.CheckStatus(targetToApplyState, attack.StateGenerated))
                    {
                        //Debug.Log("El personaje ya tiene ese mismo estado");
                        statesLibrary.ResetTurnsOfCharacterState(targetToApplyState, attack.StateGenerated);

                    }
                    else StartCoroutine(statesLibrary.StateEffectIndividual(targetToApplyState, attack.StateGenerated));
                }

                break;

            case ActionStates.Heal:
                Debug.Log("targetToApplyState " + targetToApplyState);
                Debug.Log("attack " + attack.StateGenerated);
                statesLibrary.RemoveCharacterWithState(targetToApplyState, attack.StateGenerated);

                break;
        }

        yield return new WaitForSeconds(0.001f);

    }

    private void ManaManager(AttackData attack, Stats characterStats)
    {
        characterStats.Mana -= attack.ManaCost;

        // Para hacerlo más facil de ver visualmente, los ataques fisicos cuentan con un -, asi esos se vuelven positivos y los "positivos reales" se muestran negativos.
        floatingText.ShowFloatingTextNumbers(characterStats.gameObject, -attack.ManaCost, Color.blue);
    }


}