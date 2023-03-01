using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(NetworkLoadScene), (typeof(NetworkObject)))]
public class NetowrkUIManager : MonoBehaviour
{
    [SerializeField] private Button startHost;
    [SerializeField] private Button startClient;
    [SerializeField] private Button ExitButton;
    [SerializeField] private Button practise;

    [SerializeField] private TMP_InputField inputField;

    [SerializeField] private SceneTransition sceneTransition;

    [SerializeField] private ScriptableObjectTeamInformation teamInformation;

    [SerializeField] private List<GameObject> deactivatedObjectsInUi;
    [SerializeField] private int maxConnections = 3;

    [SerializeField] private MainMenu mainMenu;

    private List<GameObject> activatedUiObjects;

    private void Awake()
    {
        Cursor.visible = true;
        activatedUiObjects = deactivatedObjectsInUi;
    }

    public List<GameObject> GetThingsThatShouldBeTurnedOnAgain()
    {
        return activatedUiObjects;
    }

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Sigend In " + AuthenticationService.Instance.PlayerId);
        };
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        ExitButton.onClick.AddListener(() => { Application.Quit(); });

        practise.onClick.AddListener(() =>
        {
            try
            {
                CreateRelayPractise();
            }
            catch (NotServerException)
            {
                Debug.LogError("Something went wrong with the UnityRelay");
                return;
            }

            ThingsTurningOf();
        });


        startHost.onClick.AddListener((() =>
        {
            try
            {
                CreateRelay();
            }
            catch (NotServerException)
            {
                Debug.LogError("Something went wrong with the UnityRelay");
                return;
            }

            ThingsTurningOf();
        }));

        startClient.onClick.AddListener((() =>
        {
            try
            {
                JoinRelay(inputField.text);
            }
            catch (NotServerException e)
            {
                Console.WriteLine(e);
            }
        }));
    }

    private async void CreateRelay()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Need to SignIn");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(joinCode + " JoinCode");
            teamInformation.Joing = joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

        sceneTransition.WaitForNM();
        NetworkLoadScene.Instance.HostGame();
    }

    private async void CreateRelayPractise()
    {
        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
            var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            teamInformation.Joing = joinCode;

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData);

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }

        sceneTransition.WaitForNM();
        NetworkLoadScene.Instance.PractiseGame();
    }


    private async void JoinRelay(string joinCode)
    {
        AuthenticationService.Instance.SignOut(true);
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Need To signIn JoinRealy");
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        teamInformation.Joing = joinCode;
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(teamInformation.Joing);
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
            );
            NetworkManager.Singleton.StartClient();
            sceneTransition.WaitForNM();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError(e + " JOIN CODE " + joinCode + " TeamInformation joincode " + teamInformation.Joing);
            return;
        }

        ThingsTurningOf();
    }

    private void ThingsTurningOf()
    {
        foreach (var uiElements in deactivatedObjectsInUi)
        {
            uiElements.SetActive(false);
        }
    }
}