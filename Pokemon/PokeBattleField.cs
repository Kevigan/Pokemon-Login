using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PokeBattleField : MonoBehaviour
{
    public Pokemon[] pokemon;
    private bool startFight = false;
    private bool isFirstPoke = true;
    public TextMeshProUGUI message;
    public PokeAPIController controller;
    public AchievementManager achievementManager;

    void Start()
    {
        pokemon[0].battleField = this;
        pokemon[1].battleField = this;
    }

    private void Update()
    {
    }

    public void StartFight()
    {
        if (pokemon[0].HP <= 0 || pokemon[1].HP <= 0)
        {
            EndFight();
            return;
        }
        else
        {
            StartCoroutine(FightLoop());
        }
    }

    private IEnumerator FightLoop()
    {
        StartCoroutine(Attack());
        yield return new WaitForSeconds(4);

        if (pokemon[0].HP <= 0)
        {
            EndFight();
            isFirstPoke=true;
            SetMessage(pokemon[0].pokeNameText.text + " died in battle. " + pokemon[1].pokeNameText.text + " is the winner!");
            pokemon[1].PlayWinAnim();
            StartCoroutine(controller.GetShinySprit(pokemon[1].pokemonIndex, pokemon[1]));
        }
        else if (pokemon[1].HP <= 0)
        {
            EndFight();
            isFirstPoke=true;
            SetMessage(pokemon[1].pokeNameText.text + " died in battle. " + pokemon[0].pokeNameText.text + " is the winner!");
            pokemon[0].PlayWinAnim();
            StartCoroutine(controller.GetShinySprit(pokemon[0].pokemonIndex, pokemon[0]));
        }
        else StartCoroutine(FightLoop());
    }

    private IEnumerator Attack()
    {
        if(isFirstPoke) pokemon[0].PlayAttackAnim();
        else pokemon[1].PlayAttackAnim();

        yield return new WaitForSeconds(1);

        if (isFirstPoke)
        {
            pokemon[0].Attack(pokemon[1]);
            pokemon[1].PlayHurtAnim();
        }
        else
        {
            pokemon[1].Attack(pokemon[0]);
            pokemon[0].PlayHurtAnim();
        }

        isFirstPoke = !isFirstPoke;
    }

    private void EndFight()
    {
        startFight = false;
    }

    public void SetMessage(string actionMessage)
    {
        message.text = actionMessage;
    }
}
