using UnityEngine;

public class LevelSystem : MonoBehaviour
{
    private Stats characterST;
    [SerializeField] private float requiredXP;
    public float RequiredXP { get { return requiredXP; } set { requiredXP = value; }}

    private FloatingTextCombat floatingText;

    [SerializeField] [Range(1f,300f)] private float adittionMultiplier = 300;
    [SerializeField] [Range(2f,4f)] private float powerMultiplier = 2;
    [SerializeField] [Range(7f,14f)] private float divisionMultiplier = 7;

    private const float NormalStatUpgrade = 20;

    private const float EffectiveStatUpgrade = 70;

    private const float CriticalStatUpgrade = 3;

    private void Start()
    {
        characterST = GetComponent<Stats>();

        if(characterST.CompareTag("Player"))
        {
            requiredXP = CalculateRequireXp();

            floatingText = GameObject.Find("System").GetComponent<FloatingTextCombat>();
        }
    }

    public void GainExperienceFlatRate(float xpGained)
    {
        characterST.CurrentXP += xpGained;

        Debug.Log("Personaje: " + characterST.name + " obtiene experiencia " + xpGained);
        
        if(characterST.CurrentXP > requiredXP)
        {
            floatingText.ShowFloatingText(characterST.gameObject, "Lvl Up!", Color.black);
            LevelUp();
        }
    }

    private void LevelUp()
    {
        Debug.Log("Personaje: " + characterST.name + " subio de nivel");
        characterST.Level++;
        characterST.CurrentXP = Mathf.RoundToInt(characterST.CurrentXP - requiredXP); 

        IncrementStats();

        requiredXP = CalculateRequireXp();
    } 

    public void LevelUpEnemy(int setLevel, Stats chara)
    {
        if(chara.Level < setLevel)
        {
            while(chara.Level < setLevel)
            {
                chara.Level++;
                IncrementStats(chara);

                chara.DropXP += 30;
                chara.MoneyDrop += 30;

                Debug.Log("Enemigo: " + chara.name + " subio de nivel: " + chara.Level);
            }
        }
    }

    // Jugador
    private void IncrementStats()
    {
        switch(characterST.Rol)
        {
            case CharacterRol.Tank:
                characterST.MaxHealth += EffectiveStatUpgrade;
                characterST.MaxMana += NormalStatUpgrade;
                characterST.Strenght += NormalStatUpgrade;
                characterST.PhysicalDefense += EffectiveStatUpgrade;
                characterST.MagicAtk += NormalStatUpgrade;
                characterST.MagicDef += EffectiveStatUpgrade;
                characterST.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Priest:
                characterST.MaxHealth += EffectiveStatUpgrade;
                characterST.MaxMana += EffectiveStatUpgrade;
                characterST.Strenght += NormalStatUpgrade;
                characterST.PhysicalDefense += NormalStatUpgrade;
                characterST.MagicAtk += NormalStatUpgrade;
                characterST.MagicDef += EffectiveStatUpgrade;
                characterST.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Wizard:
                characterST.MaxHealth += NormalStatUpgrade;
                characterST.MaxMana += EffectiveStatUpgrade;
                characterST.Strenght += NormalStatUpgrade;
                characterST.PhysicalDefense += NormalStatUpgrade;
                characterST.MagicAtk += EffectiveStatUpgrade;
                characterST.MagicDef += EffectiveStatUpgrade;
                characterST.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Knight:
                characterST.MaxHealth += EffectiveStatUpgrade;
                characterST.MaxMana += NormalStatUpgrade;
                characterST.Strenght += EffectiveStatUpgrade;
                characterST.PhysicalDefense += EffectiveStatUpgrade;
                characterST.MagicAtk += NormalStatUpgrade;
                characterST.MagicDef += NormalStatUpgrade;
                characterST.CriticalChance += CriticalStatUpgrade;
            break;

        }

        characterST.Health = characterST.MaxHealth;
        characterST.Mana = characterST.MaxMana;
    }

    // Enemigos
    private void IncrementStats(Stats chara)
    {
        switch(chara.Rol)
        {
            case CharacterRol.Tank:
                chara.MaxHealth += EffectiveStatUpgrade;
                chara.MaxMana += NormalStatUpgrade;
                chara.Strenght += NormalStatUpgrade;
                chara.PhysicalDefense += EffectiveStatUpgrade;
                chara.MagicAtk += NormalStatUpgrade;
                chara.MagicDef += EffectiveStatUpgrade;
                chara.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Priest:
                chara.MaxHealth += EffectiveStatUpgrade;
                chara.MaxMana += EffectiveStatUpgrade;
                chara.Strenght += NormalStatUpgrade;
                chara.PhysicalDefense += NormalStatUpgrade;
                chara.MagicAtk += NormalStatUpgrade;
                chara.MagicDef += EffectiveStatUpgrade;
                chara.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Wizard:
                chara.MaxHealth += NormalStatUpgrade;
                chara.MaxMana += EffectiveStatUpgrade;
                chara.Strenght += NormalStatUpgrade;
                chara.PhysicalDefense += NormalStatUpgrade;
                chara.MagicAtk += EffectiveStatUpgrade;
                chara.MagicDef += EffectiveStatUpgrade;
                chara.CriticalChance += CriticalStatUpgrade;
            break;

            case CharacterRol.Knight:
                chara.MaxHealth += EffectiveStatUpgrade;
                chara.MaxMana += NormalStatUpgrade;
                chara.Strenght += EffectiveStatUpgrade;
                chara.PhysicalDefense += EffectiveStatUpgrade;
                chara.MagicAtk += NormalStatUpgrade;
                chara.MagicDef += NormalStatUpgrade;
                chara.CriticalChance += CriticalStatUpgrade;
            break;

        }

        chara.Health = chara.MaxHealth;
        chara.Mana = chara.MaxMana;
    }

    private int CalculateRequireXp()
    {
        var solveForRequireXp = 0;

        for (int levelCycle = 1; levelCycle <= characterST.Level; levelCycle++)
        {
            solveForRequireXp += (int) Mathf.Floor(levelCycle + adittionMultiplier * Mathf.Pow(powerMultiplier, levelCycle / divisionMultiplier));
        }

        return solveForRequireXp / 4;

    }


}
