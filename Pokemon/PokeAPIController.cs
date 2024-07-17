using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using SimpleJSON;
using UnityEngine.UI;
using TMPro;

public class PokeAPIController : MonoBehaviour
{
    public PokeBattleField pokeBattleField;

    private readonly string basePokeURL = "https://pokeapi.co/api/v2/";
    

    void Start()
    {
    }

    public void OnButtonRandomPokemon()
    {
        print(pokeBattleField);
        pokeBattleField.controller = this;
        StartCoroutine(GetPokemonAtIndex());
    }

    IEnumerator GetPokemonAtIndex()
    {
        pokeBattleField.SetMessage("");
        foreach (Pokemon pokemon in pokeBattleField.pokemon)
        {
            int randomPokeIndex = Random.Range(1, 808);
            pokemon.pokenumText.text = "Loading...";
            pokemon.pokenumText.text = "#" + randomPokeIndex;
            pokemon.pokemonIndex = randomPokeIndex;

            foreach (TextMeshProUGUI pokeTypeText in pokemon.pokeTypeTextArray)
            {
                pokeTypeText.text = "";
            }

            string pokemonURL = basePokeURL + "pokemon/" + randomPokeIndex.ToString();

            UnityWebRequest pokeInfoRequest = UnityWebRequest.Get(pokemonURL);

            yield return pokeInfoRequest.SendWebRequest();

            if (pokeInfoRequest.isNetworkError || pokeInfoRequest.isHttpError)
            {
                Debug.LogError(pokeInfoRequest.error);
                yield break;
            }

            JSONNode pokeInfo = JSON.Parse(pokeInfoRequest.downloadHandler.text);

            string pokeName = pokeInfo["name"];
            string pokeSpriteURL = pokeInfo["sprites"]["front_default"];
            
            string pokeHP = pokeInfo["stats"][0]["base_stat"];
            string pokeAttack = pokeInfo["stats"][1]["base_stat"];
            string pokeDefense = pokeInfo["stats"][2]["base_stat"];
            string pokeSpecialAttack = pokeInfo["stats"][3]["base_stat"];


            JSONNode pokeTypes = pokeInfo["types"];
            string[] pokeTypeNames = new string[pokeTypes.Count];

            for (int i = 0, j = pokeTypes.Count - 1; i < pokeTypes.Count; i++, j--)
            {
                pokeTypeNames[j] = pokeTypes[i]["type"]["name"];
            }

            //Get Pokemon Sprite

            UnityWebRequest pokeSpriteRequest = UnityWebRequestTexture.GetTexture(pokeSpriteURL);
            

            yield return pokeSpriteRequest.SendWebRequest();

            if (pokeSpriteRequest.isNetworkError || pokeSpriteRequest.isHttpError)
            {
                Debug.LogError(pokeSpriteRequest.error);
                yield break;
            }
            
            // Set UI Objects

            pokemon.pokeRawImage.texture = DownloadHandlerTexture.GetContent(pokeSpriteRequest);
            pokemon.pokeRawImage.texture.filterMode = FilterMode.Point;

            pokemon.pokeNameText.text = CapitalizeFirstLetter(pokeName);
            pokemon.ParseStats(pokeHP, pokeAttack, pokeDefense, pokeSpecialAttack);

            for (int i = 0; i < pokeTypeNames.Length; i++)
            {
                pokemon.pokeTypeTextArray[i].text = CapitalizeFirstLetter(pokeTypeNames[i]);
            }
        }
    }

    private string CapitalizeFirstLetter(string str)
    {
        return char.ToUpper(str[0]) + str.Substring(1);
    }

    public IEnumerator GetShinySprit(int index, Pokemon poke)
    {
       // poke.pokeShinyRawImage.enabled = true;
        //poke.pokeRawImage.enabled = false;
        string pokemonURL = basePokeURL + "pokemon/" + index.ToString();

        UnityWebRequest pokeInfoRequest = UnityWebRequest.Get(pokemonURL);

        yield return pokeInfoRequest.SendWebRequest();

        if (pokeInfoRequest.isNetworkError || pokeInfoRequest.isHttpError)
        {
            Debug.LogError(pokeInfoRequest.error);
            yield break;
        }

        JSONNode pokeInfo = JSON.Parse(pokeInfoRequest.downloadHandler.text);
        ///////
        //string pokeShinySpriteURL = pokeInfo["sprites"]["front_shiny"];
        string pokeShinySpriteURL = pokeInfo["sprites"]["other"]["home"]["front_shiny"];

        UnityWebRequest pokeShinySpriteRequest = UnityWebRequestTexture.GetTexture(pokeShinySpriteURL);

        yield return pokeShinySpriteRequest.SendWebRequest();

        if (pokeShinySpriteRequest.isNetworkError || pokeShinySpriteRequest.isHttpError)
        {
            Debug.LogError(pokeShinySpriteRequest.error);
            yield break;
        }

        poke.pokeRawImage.texture = DownloadHandlerTexture.GetContent(pokeShinySpriteRequest);
        poke.pokeRawImage.texture.filterMode = FilterMode.Point;
    }
}
