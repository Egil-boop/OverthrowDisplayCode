using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject playerCamera;
    [SerializeField] private GameObject playerDeviceManager;
    [SerializeField] private GameObject flagObserver;
    [SerializeField] private GameObject spawnOne;
    [SerializeField] private GameObject spawnTwo;

    [SerializeField] private Transform spawnFlag;

    [SerializeField] private ScriptableObjectTeamInformation soTeamInformation;
    [SerializeField] private SettingsScritable settingContainer;

    [SerializeField] private GameObject spectatorPrefab;

    [SerializeField] private List<GameObject> CanvasObjectsToSetInactiveIfSpectator;

    private AimAssistColliderCheck aimAssist;

    public Material teamOne;
    public Material teamTwo;


    public Material weaponCaseRed;
    public Material weaponCaseBlue;

    public Material bazookaPipeRed;
    public Material bazookaPipeBlue;

    public Material grenadePipeRed;
    public Material grenadePipeBlue;

    [SerializeField] private Sprite teamOneWeaponScope;
    [SerializeField] private Sprite teamTwoWeaponScope;


    private List<SavedPlayerInformation> playerInformations = new List<SavedPlayerInformation>();

    private GameObject playerInstance;

    private void Awake()
    {
        if (!IsHost)
        {
            return;
        }
    }

    /*
     * Method that spawns and sets up the player.
     * Both for the Player and Spectator.
     */

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            return;
        }

        var spawnOffset = 0;


        //Setting PlayerInstanace Information
        foreach (var (key, value) in soTeamInformation.PlayersTeamStateAndOwnerId)
        {
            if (value == TeamState.Spectator)
            {
                playerInstance = Instantiate(spectatorPrefab, spawnOne.transform.position, Quaternion.identity);
                foreach (var go in CanvasObjectsToSetInactiveIfSpectator)
                {
                    go.SetActive(false);
                }
            }
            else
            {
                var playerSpawnPosition = value == TeamState.TeamBlue
                    ? spawnOne.transform.position
                    : spawnTwo.transform.position;

                playerInstance = Instantiate(playerPrefab,
                    new Vector3(playerSpawnPosition.x, playerSpawnPosition.y, playerSpawnPosition.z + spawnOffset),
                    Quaternion.identity);
                playerInstance.GetComponent<Player>().aimAssistUseable = soTeamInformation.isAimAssistEnabled;
            }

            playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(key, false);

            var playerObject = NetworkManager
                .ConnectedClients[playerInstance.GetComponent<NetworkObject>().OwnerClientId].PlayerObject;

            try
            {
                var playerSettings = playerObject.gameObject.GetComponent<PlayerSettings>();
                playerSettings.SetBaseMovementSpeed(settingContainer.MovementSpeed);
                playerSettings.SetMaxJumpForce(settingContainer.JumpForce);
                playerSettings.SetGravity(settingContainer.Gravity);
                playerSettings.SetFlagCarrySpeed(settingContainer.FlagCarrySpeed);

                // Borde finnas i playerSettings?
                var meleeWeaponSettings = playerObject.gameObject.GetComponentInChildren<MeleeWeapon>();
                meleeWeaponSettings.SetDamage(settingContainer.MeleeWeaponDamage);

                //Borde FInnas i playerSettings?
                var knockBackWeapon =
                    playerObject.gameObject.GetComponentInChildren<KnockbackWeapon>();
                knockBackWeapon.SetDamage(settingContainer.KnockBackWeaponDamage);

                //Bazooka och Granat kvar. 
            }
            catch (NullReferenceException)
            {
                Debug.LogWarning(
                    "WARNING CUSTOM: Probably a Spector, if something is not working PlayerSettings is null.");
            }

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { key }
                }
            };

            playerInformations.Add(new SavedPlayerInformation(key, value));

            if (soTeamInformation.PlayersTeamStateAndOwnerId[key] != TeamState.Spectator)
            {
                SpawnCameraClientRpc(key, clientRpcParams);
            }

            SetMyOwnInformationOnClientRpc(soTeamInformation.amountOfPoints, key, value,
                soTeamInformation.isAimAssistEnabled, clientRpcParams);
            spawnOffset += 2;
        }

        CreatingFlagAndSpawningIt();


        foreach (var (playerId, value) in NetworkManager.Singleton.ConnectedClients)
        {
            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new ulong[] { playerId }
                }
            };


            var objectsClient = GameObject.FindGameObjectsWithTag("Player");

            // Foreach object that has the palyerTag
            foreach (var obj in objectsClient)
            {
                // Aslong as the object that is found is not itslef
                if (obj.GetComponent<NetworkObject>().OwnerClientId == playerId)
                {
                    continue;
                }

                // Go thoguth all the playerInformations
                foreach (var playerInformation in playerInformations)
                {
                    SetLocalInformationForInstancesOfPlayersClientRpc(playerInformation,
                        soTeamInformation.isAimAssistEnabled,
                        clientRpcParams);
                }
            }
        }
    }

    private void SetPlayerPrefabInformation()
    {
        throw new System.NotImplementedException();
    }

    private void CreatingFlagAndSpawningIt()
    {
        GameObject flagInstance = Instantiate(flagObserver);
        flagInstance.transform.position = spawnFlag.position;
        flagInstance.GetComponent<NetworkObject>().Spawn();
    }

    /*
     * Ett ClientRPC där varje klient som har spawntas går igenom
     * Sina lokala instancer av de spawnade spelarna och sätter de lokala
     * värderna till det som finns i playerInfmationClient.
     * 
     */
    [ClientRpc]
    private void SetLocalInformationForInstancesOfPlayersClientRpc(SavedPlayerInformation playerInformationClient,
        bool aimAssistEnabled,
        ClientRpcParams clientRpcParams)
    {
        var objects = GameObject.FindGameObjectsWithTag("Player"); // alla Insancer av spelarna hos klienten.

        PlayerWorldInformation pInfo =
            NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerWorldInformation>();

        foreach (var obj in objects)
        {
            if (obj.GetComponent<NetworkObject>().OwnerClientId == playerInformationClient.PLAYERID)
            {
                PlayerWorldInformation objPlayerWorldInformation = obj.GetComponent<PlayerWorldInformation>();

                objPlayerWorldInformation.myTeamState = playerInformationClient.TEAM;
                objPlayerWorldInformation.myOwnMask =
                    playerInformationClient.TEAM == TeamState.TeamBlue ? 1 << 7 : 1 << 8;

                objPlayerWorldInformation.aimAssistObj.SetActive(aimAssistEnabled);
                SetMaterialByTeam(playerInformationClient.TEAM, objPlayerWorldInformation);

                obj.layer = objPlayerWorldInformation.myTeamState == TeamState.TeamBlue
                    ? LayerMask.NameToLayer("TeamOne")
                    : LayerMask.NameToLayer(
                        "TeamTwo");


                if (objPlayerWorldInformation.myTeamState == pInfo.myTeamState)
                {
                    if (!pInfo.myTeamMates.Contains(obj))
                    {
                        pInfo.myTeamMates.Add(obj);
                        objPlayerWorldInformation.SetMinimap(pInfo.myTeamState);
                    }
                }

                if (!pInfo.playersInGame.Contains(obj))
                {
                    pInfo.playersInGame.Add(obj);
                }
            }
        }
    }

    /*Sum
     *Setting up PlayerWorldInformation about my own client.
     *  
     */
    [ClientRpc]
    private void SetMyOwnInformationOnClientRpc(int points, ulong clientId, TeamState state, bool aimAssistEnabled,
        ClientRpcParams clientRpcParams = default)
    {
        var playerNetworkObject = NetworkManager.Singleton.LocalClient.PlayerObject;
        var playerWorldInformation = playerNetworkObject.GetComponent<PlayerWorldInformation>();

        if (state == TeamState.Spectator)
        {
            playerWorldInformation.playersInGame.Add(playerNetworkObject.gameObject);
            return;
        }

        playerNetworkObject.GetComponent<Player>()
            .SetWeaponScope(state == TeamState.TeamBlue ? teamOneWeaponScope : teamTwoWeaponScope);


        playerNetworkObject.GetComponent<PlayerHealth>().SetSpawnPoint((state == TeamState.TeamBlue
            ? spawnOne.transform
            : spawnTwo.transform));

        playerWorldInformation.myTeamMates.Add(playerNetworkObject.gameObject);
        playerWorldInformation.amountOfPoints = points;
        playerWorldInformation.myTeamState = state;
        playerWorldInformation.myOwnMask = state == TeamState.TeamBlue ? 1 << 7 : 1 << 8;
        playerWorldInformation.aimAssistObj.SetActive(aimAssistEnabled);
        playerWorldInformation.myMaterial = SetMaterialByTeam(state, playerWorldInformation);
        playerWorldInformation.playersInGame.Add(playerNetworkObject.gameObject);

        playerWorldInformation.SetMinimap(state);

        playerNetworkObject.gameObject.layer = state == TeamState.TeamBlue
            ? LayerMask.NameToLayer("TeamOne")
            : LayerMask.NameToLayer("TeamTwo");
        FindObjectOfType<InGameMenu>().SetPlayer(playerNetworkObject.gameObject.GetComponent<Player>());
    }

    private Material SetMaterialByTeam(TeamState state, PlayerWorldInformation playerWorldInformation)
    {
        if (state == TeamState.TeamBlue)
        {
            playerWorldInformation.SetMyMaterial(teamOne, teamOne, weaponCaseBlue, bazookaPipeBlue, grenadePipeBlue);

            return teamOne;
        }
        else
        {
            playerWorldInformation.SetMyMaterial(teamTwo, teamTwo, weaponCaseRed, bazookaPipeRed, grenadePipeRed);

            return teamTwo;
        }
    }

    [ClientRpc]
    private void SpawnCameraClientRpc(ulong clientID, ClientRpcParams clientRpcParams = default)
    {
        var cameraInstance = Instantiate(playerCamera);
        var playerCam = cameraInstance.GetComponent<PlayerCamera>();

        playerCam.GetCameraComponents();

        playerCam.FollowAndLookPlayer(NetworkManager.Singleton.LocalClient.PlayerObject.transform);

        Player player = NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<Player>();
        player.SetCamera(playerCam);

        player.GetDeviceManager();
        player.GetComponentInChildren<Crosshair>().SetPlayerAndPlayerCamera(player);
        player.Weapons.GetCrosshairComponent();
    }
}


public struct SavedPlayerInformation : INetworkSerializable
{
    public ulong PLAYERID;
    public TeamState TEAM;


    public SavedPlayerInformation(ulong playerid, TeamState team)
    {
        PLAYERID = playerid;
        TEAM = team;
    }

    public override string ToString()
    {
        return $"PlayerId {PLAYERID},  Team {TEAM}";
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref PLAYERID);
        serializer.SerializeValue(ref TEAM);
    }
}