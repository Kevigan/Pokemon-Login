using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Pokemon : MonoBehaviour
{
    public int HP, attack, defense, specialAttack, specialDefense;
    public int pokemonIndex;

    public RawImage pokeRawImage;
    public TextMeshProUGUI pokeNameText, pokenumText, hpText, attackText, defenseText, specialAttactText;
    public TextMeshProUGUI[] pokeTypeTextArray;

    public Animator animator;
    public string attackName;

    public PokeBattleField battleField;

    private void Start()
    {
        pokeRawImage.texture = Texture2D.blackTexture;

        pokeNameText.text = "";
        pokenumText.text = "";
        hpText.text = "";
        attackText.text = "";

        foreach (TextMeshProUGUI pokeTypeText in pokeTypeTextArray)
        {
            pokeTypeText.text = "";
        }
    }

    public void ParseStats(string HP_Stat, string Attack_Stat, string Defense_Stat, string SpecialAttack_Stat)
    {
        for (int i = 0; i < 4; i++)
        {
            try
            {
                switch (i)
                {
                    case 0:
                        HP = int.Parse(HP_Stat);
                        break;
                    case 1:
                        attack = int.Parse(Attack_Stat);
                        break;
                    case 2:
                        defense = int.Parse(Defense_Stat);
                        break;
                    case 3:
                        specialAttack = int.Parse(SpecialAttack_Stat);
                        break;
                    default:
                        break;
                }
            }
            catch (FormatException)
            {
                Debug.LogError("String is not a valid integer");
            }
        }
        SetStats();
    }

    public void SetStats()
    {
        hpText.text = "HP: " + HP.ToString();
        attackText.text = "Attack: " + attack.ToString();
        defenseText.text = "Defense: " + defense.ToString();
        specialAttactText.text = "Spc. Attack: " + specialAttack.ToString();
    }

    public void Attack(Pokemon pokeToAttack)
    {
        int damage = (int)MathF.Abs((attack - (pokeToAttack.defense / 2)));
        pokeToAttack.HP -= damage;
        if (pokeToAttack.HP < 0) pokeToAttack.HP = 0;
        pokeToAttack.SetStats();
        battleField.SetMessage(pokeNameText.text + " did " + damage + " damage to " + pokeToAttack.pokeNameText.text);
    }

    public void PlayAttackAnim()
    {
        animator.SetTrigger("attackTrigger");
    }

    public void PlayHurtAnim()
    {
        animator.SetTrigger("hurtTrigger");
    }

    public void PlayWinAnim()
    {
        animator.SetTrigger("winTrigger");
        print("asdasdas");
    }
}
