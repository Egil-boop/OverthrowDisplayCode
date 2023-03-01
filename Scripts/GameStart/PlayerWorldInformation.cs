using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PlayerWorldInformation : NetworkBehaviour
{
    [Header("Settings")] [Range(0.1f, 3)] public float dissolveTime = 3f;

    [SerializeField] private ScriptableObjectTeamInformation teamInformation;

    [Header("Other")] public Material myMaterial;

    [SerializeField] private TMP_Text playerName;

    [SerializeField] private Renderer rendererBodyLow;
    [SerializeField] private Renderer renderBlades;

    [SerializeField] private MeshRenderer charBody;
    [SerializeField] private MeshRenderer charBodySecond;

    [SerializeField] private MeshRenderer weaponCaseBazooka;
    [SerializeField] private MeshRenderer weaponPipeBazooka;
    [SerializeField] private MeshRenderer weaponPipeTwoBazooka;

    [SerializeField] private MeshRenderer weaponCaseGrenadeLauncher;
    [SerializeField] private MeshRenderer weaponPipeGrenadeLauncher;
    [SerializeField] private MeshRenderer weaponPipeTwoGrenadeLauncher;

    [SerializeField] private GameObject playerCanvas;
    [SerializeField] private GameObject heatEffect;

    [SerializeField] private GameObject miniMapIcon;

    [SerializeField] private UIVolumePlayer uiVolumePlayer;

    private static readonly int DissolveAmount = Shader.PropertyToID("_DissolveAmount");

    public int amountOfPoints;

    public List<GameObject> playersInGame;
    public List<GameObject> myTeamMates = new List<GameObject>();

    public LayerMask myOwnMask;
    public GameObject aimAssistObj;

    public TeamState myTeamState;

    public string myPlayerName;

    public GameObject GetPlayerCanvas()
    {
        return playerCanvas;
    }


    private void Start()
    {
        if (!IsOwner)
        {
            return;
        }

        NetworkManager.Singleton.OnClientDisconnectCallback += RemoveDissconnectingPlayers;

    }

    private void RemoveDissconnectingPlayers(ulong player)
    {
        foreach (var disconnectingPlayer in playersInGame.Where(disconnectingPlayer =>
                     disconnectingPlayer.GetComponent<NetworkObject>().OwnerClientId == player))
        {
            playersInGame.Remove(disconnectingPlayer);

            if (myTeamMates.Contains(disconnectingPlayer))
            {
                myTeamMates.Remove(disconnectingPlayer);
            }

            break;
        }
    }

    public override void OnDestroy()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback -= RemoveDissconnectingPlayers;
        base.OnDestroy();
    }

    public void SetMyMaterial(Material one, Material two, Material weaponCase, Material weaponPipeBazooka,
        Material weaponPipeGrenadeLauncher)
    {
        charBody.material = one;
        charBodySecond.material = two;

        this.weaponCaseBazooka.material = weaponCase;
        this.weaponPipeBazooka.material = weaponPipeBazooka;
        this.weaponPipeTwoBazooka.material = weaponPipeBazooka;

        this.weaponCaseGrenadeLauncher.material = weaponCase;
        this.weaponPipeGrenadeLauncher.material = weaponPipeGrenadeLauncher;
        this.weaponPipeTwoGrenadeLauncher.material = weaponPipeGrenadeLauncher;


        StartCoroutine(ReverseDissolvePlayer());
    }

    private float DissolveAmountfloat = 0;

    public bool isDissolved;

    public void SetMinimap(TeamState state)
    {
        if (miniMapIcon == null)
        {
            return;
        }

        miniMapIcon.SetActive(true);
        uiVolumePlayer.Recieve(teamInformation.colorBlindSettings);
        miniMapIcon.GetComponentInChildren<Image>().color = state == TeamState.TeamBlue
            ? uiVolumePlayer.currentColorBlue
            : uiVolumePlayer.currentColorRed;
    }

    public void SetminiMapUi(TeamState state)
    {
        if (miniMapIcon == null)
        {
            return;
        }

        uiVolumePlayer.Recieve(teamInformation.colorBlindSettings);
        miniMapIcon.GetComponentInChildren<Image>().color = state == TeamState.TeamBlue
            ? uiVolumePlayer.currentColorBlue
            : uiVolumePlayer.currentColorRed;
    }


    public void ResetPlayerInLevelList()
    {
        StartCoroutine("testWithThis");
    }

    private IEnumerator testWithThis()
    {
        yield return new WaitForSeconds(1);
        for (var i = playersInGame.Count - 1; i > -1; i--)
        {
            if (playersInGame[i] == null)
                playersInGame.RemoveAt(i);
        }

        for (var i = myTeamMates.Count - 1; i > -1; i--)
        {
            if (myTeamMates[i] == null)
                myTeamMates.RemoveAt(i);
        }
    }

    public IEnumerator DissolvePlayer()
    {
        float timeElapsed = 0;

        try
        {
            playerName.enabled = false;
        }
        catch (NullReferenceException)
        {
        }

        heatEffect.SetActive(false);
        while (timeElapsed < dissolveTime)
        {
            DissolveAmountfloat = Mathf.Lerp(DissolveAmountfloat, 1.0f, timeElapsed / dissolveTime);
            rendererBodyLow.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            renderBlades.material.SetFloat(DissolveAmount, DissolveAmountfloat);

            this.weaponCaseBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            this.weaponPipeBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            this.weaponPipeTwoBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);

            this.weaponCaseGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            this.weaponPipeGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            this.weaponPipeTwoGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);


            timeElapsed += Time.deltaTime;
            yield return null;
        }


        DissolveAmountfloat = 1;
    }

    public IEnumerator ReverseDissolvePlayer()
    {
        float timeElapsed = 0;
        try
        {
            playerName.enabled = true;
        }
        catch (NullReferenceException)
        {
        }

        heatEffect.SetActive(true);
        while (timeElapsed < dissolveTime)
        {
            DissolveAmountfloat = Mathf.Lerp(DissolveAmountfloat, 0f, timeElapsed / dissolveTime);
            rendererBodyLow.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            renderBlades.material.SetFloat(DissolveAmount, DissolveAmountfloat);

            weaponCaseBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            weaponPipeBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            weaponPipeTwoBazooka.material.SetFloat(DissolveAmount, DissolveAmountfloat);

            weaponCaseGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            weaponPipeGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);
            weaponPipeTwoGrenadeLauncher.material.SetFloat(DissolveAmount, DissolveAmountfloat);


            timeElapsed += Time.deltaTime;
            yield return null;
        }


        DissolveAmountfloat = 0;
    }

    public GameObject GetPlayerHeatEffect()
    {
        return heatEffect;
    }
}