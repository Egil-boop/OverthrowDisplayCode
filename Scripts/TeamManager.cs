using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;

using UnityEngine;
using UnityEngine.UI;



public class TeamManager : NetworkBehaviour
{
    public int teamOneCount;

    public int teamTwoCount;

    public int teamSpectatorCount;

    [Header("TMPro")] [SerializeField] private TMP_Text playerTextInfomrationPrefab;
    [SerializeField] private TMP_Dropdown pointDropdown;
    [SerializeField] private TMP_InputField nameInput;

    [SerializeField] private ScriptableObjectTeamInformation teamInfo;

    [SerializeField] private List<int> pointAmount;

    [Header("Buttons")] [SerializeField] private Button joinTeamOne;
    [SerializeField] private Button joinTeamTwo;
    [SerializeField] private Button joinSpectator;


    [Tooltip("Player object for the blue team that represents the player in the lobby")] [SerializeField]
    private List<GameObject> blueTeam;

    [Tooltip("Player object for the red team that represents the player in the lobby")] [SerializeField]
    private List<GameObject> redTeam;

    private TeamState teamState = TeamState.NotSinged;

    private Action<ulong> removeClientFromTeamWhenLeaving;
    private Action<ulong> sendInfoToJoiningClient;

    private Dictionary<ulong, string> instancePlayerIDTogetherWithName = new Dictionary<ulong, string>();

    private int teamOne = 0;
    private int teamTwo = 0;

    private void Awake()
    {
        nameInput.characterLimit = 12;
        nameInput.lineLimit = 1;
    }

    public ScriptableObjectTeamInformation GetTeamInfo()
    {
        return teamInfo;
    }

    public override void OnNetworkSpawn()
    {
        if (!IsHost)
        {
            pointDropdown.gameObject.SetActive(false);
        }

        AddNameToTeamInfoServerRpc(NetworkManager.Singleton.LocalClient.ClientId, nameInput.text);

        ButtonHandler();
    }

    private void Start()
    {
        if (!IsHost)
        {
            return;
        }

        removeClientFromTeamWhenLeaving += obj => RemoveFromTeamServerRpc(obj);
        NetworkManager.Singleton.OnClientDisconnectCallback += removeClientFromTeamWhenLeaving;

        sendInfoToJoiningClient += InitializeInformationForJoiningClient;
        NetworkManager.Singleton.OnClientConnectedCallback += sendInfoToJoiningClient;

        SetAmountOfPointValue(pointDropdown.value);
        pointDropdown.onValueChanged.AddListener(SetAmountOfPointValue);
    }


    private void SetAmountOfPointValue(int points)
    {
        teamInfo.amountOfPoints = points switch
        {
            0 => pointAmount[points],
            1 => pointAmount[points],
            2 => pointAmount[points],
            3 => pointAmount[points],
            _ => 250
        };
    }

    private void Update()
    {
        if (!IsHost)
        {
            joinSpectator.gameObject.SetActive(false);
        }

        teamOne = 0;
        teamTwo = 0;
        AddToVerticalLayoutGroup();
        
    }

    private void FixedUpdate()
    {
        KeepNameUpdatedForAllWithATeam();
    }

    private void AddToVerticalLayoutGroup()
    {
        foreach (var (key, value) in teamInfo.PlayersTeamStateAndOwnerId)
        {
            switch (value)
            {
                case TeamState.TeamBlue:
                    teamOne++;
                    teamOneCount = teamOne;
                    break;
                case TeamState.TeamRed:
                    teamTwo++;
                    teamTwoCount = teamTwo;
                    break;
                case TeamState.NotSinged:
                    break;
                case TeamState.Spectator:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (instancePlayerIDTogetherWithName.ContainsKey(key)) continue;

            var playerText = playerTextInfomrationPrefab;

            instancePlayerIDTogetherWithName.Add(key, playerText.text);
        }
    }

    private void KeepNameUpdatedForAllWithATeam()
    {
        foreach (var (key, value) in teamInfo.playerNameWIthOwnerId.Where(variable =>
                     instancePlayerIDTogetherWithName.ContainsKey(variable.Key)))
        {
            instancePlayerIDTogetherWithName[key] = value;

            if (!teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(key))
            {
                continue;
            }

            switch (teamInfo.PlayersTeamStateAndOwnerId[key])
            {
                case TeamState.TeamBlue:
                    foreach (var obj in blueTeam.Where(obj =>
                                 obj.GetComponent<DroneLobbyContainer>().myOwnId == key))
                    {
                        obj.GetComponent<DroneLobbyContainer>().textMeshPro.enabled = true;
                        obj.GetComponent<DroneLobbyContainer>().textMeshPro.text = value;
                    }

                    break;
                case TeamState.TeamRed:
                    foreach (var obj in redTeam.Where(obj =>
                                 obj.GetComponent<DroneLobbyContainer>().myOwnId == key))
                    {
                        obj.GetComponent<DroneLobbyContainer>().textMeshPro.enabled = true;
                        obj.GetComponent<DroneLobbyContainer>().textMeshPro.text = value;
                    }

                    break;
                case TeamState.Spectator:
                    break;
                case TeamState.NotSinged:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }

    /// <summary>
    /// This method adds listeners to the buttons in the lobby
    /// It takes care of sending information about when to remove and when to add the player to the team.
    /// It also sets buttons to inactive so that the player cannot choose the same team again.
    /// </summary>
    private void ButtonHandler()
    {
        nameInput.onValueChanged.AddListener(((obj) =>
        {
            AddNameToTeamInfoServerRpc(NetworkManager.Singleton.LocalClient.ClientId,
                !string.IsNullOrWhiteSpace(obj) ? obj : "PlayerNoName");
        }));

        joinSpectator.onClick.AddListener((() =>
        {
            if (teamState is TeamState.TeamBlue or TeamState.TeamRed)
            {
                RemoveFromTeamServerRpc();
            }

            teamSpectatorCount = 1;
            AddPlayerToTeam(TeamState.Spectator,
                instancePlayerIDTogetherWithName[NetworkManager.Singleton.LocalClientId]);
            joinSpectator.enabled = false;
            joinSpectator.interactable = false;

            joinTeamOne.enabled = true;
            joinTeamOne.interactable = true;

            joinTeamTwo.enabled = true;
            joinTeamTwo.interactable = true;
        }));


        joinTeamOne.onClick.AddListener((() =>
        {
            if (teamState is TeamState.TeamRed or TeamState.Spectator)
            {
                RemoveFromTeamServerRpc();
            }

            if (IsHost)
            {
                teamSpectatorCount = 0;
            }

            AddPlayerToTeam(TeamState.TeamBlue,
                instancePlayerIDTogetherWithName[NetworkManager.Singleton.LocalClientId]);
            AddNameToTeamInfoServerRpc(NetworkManager.Singleton.LocalClientId, GetInstancePlayerIdTogetherWithName());

            joinTeamOne.enabled = false;
            joinTeamOne.interactable = false;

            joinSpectator.enabled = true;
            joinSpectator.interactable = true;

            if (joinTeamTwo.enabled == false)
            {
                joinTeamTwo.enabled = true;
                joinTeamTwo.interactable = true;
            }
        }));

        joinTeamTwo.onClick.AddListener((() =>
        {
            if (teamState is TeamState.TeamBlue or TeamState.Spectator)
            {
                RemoveFromTeamServerRpc();
            }

            if (IsHost)
            {
                teamSpectatorCount = 0;
            }

            AddPlayerToTeam(TeamState.TeamRed,
                instancePlayerIDTogetherWithName[NetworkManager.Singleton.LocalClientId]);
            AddNameToTeamInfoServerRpc(NetworkManager.Singleton.LocalClientId, GetInstancePlayerIdTogetherWithName());

            joinSpectator.enabled = true;
            joinSpectator.interactable = true;
            joinTeamTwo.enabled = false;
            joinTeamTwo.interactable = false;

            if (joinTeamOne.enabled == false)
            {
                joinTeamOne.enabled = true;
                joinTeamOne.interactable = true;
            }
        }));
    }

    private string GetInstancePlayerIdTogetherWithName()
    {
        if (instancePlayerIDTogetherWithName.ContainsKey(NetworkManager.Singleton.LocalClientId))
        {
            return instancePlayerIDTogetherWithName[NetworkManager.Singleton.LocalClientId];
        }
        else
        {
            return $"Player {NetworkManager.Singleton.LocalClientId}";
        }
    }


    private void InitializeInformationForJoiningClient(ulong client)
    {
       

        foreach (var playerID in teamInfo.PlayersTeamStateAndOwnerId)
        {
            string text;
            if (instancePlayerIDTogetherWithName.ContainsKey(playerID.Key))
            {
                text = instancePlayerIDTogetherWithName[playerID.Key];
                var s = text.Split(" ");
                text = s[0];
            }
            else
            {
                text = "";
            }

            SetJoiningClientRpc(playerID.Key, playerID.Value, text);

            var clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] {client}
                }
            };

           

            UpdateTeamsClientRpc(playerID.Key, playerID.Value,
                clientRpcParams);
        }
    }

    [ClientRpc]
    private void SetJoiningClientRpc(ulong client, TeamState state, string text)
    {
        if (!teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(client))
        {
            teamInfo.PlayersTeamStateAndOwnerId.Add(client, state);
        }

        if (!teamInfo.playerNameWIthOwnerId.ContainsKey(client))
        {
            teamInfo.playerNameWIthOwnerId.Add(client, text);
        }
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= removeClientFromTeamWhenLeaving;
        }

        if (IsHost)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= sendInfoToJoiningClient;
        }

        base.OnDestroy();
    }


    private void AddPlayerToTeam(TeamState state, string name)
    {
        teamState = state;
        UpdateTeamsServerRpc(state, name);
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateTeamsServerRpc(TeamState state, string name, ServerRpcParams serverRpcReceiveParams = default)
    {
        teamInfo.PlayersTeamStateAndOwnerId.Add(serverRpcReceiveParams.Receive.SenderClientId, state);
        UpdateTeamsClientRpc(serverRpcReceiveParams.Receive.SenderClientId, state);
    }

    [ClientRpc]
    private void UpdateTeamsClientRpc(ulong ownerId,  TeamState state,
        ClientRpcParams clientRpcParams = default)
    {
        if (state == TeamState.NotSinged)
        {
            return;
        }

        if (!teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(ownerId))
        {
            teamInfo.PlayersTeamStateAndOwnerId.Add(ownerId, state);
        }

        SetReverseDissolve(ownerId, state);
    }


    private void SetReverseDissolve(ulong ownerId, TeamState state)
    {
        GameObject obj = null;
        switch (state)
        {
            case TeamState.TeamBlue:
            {
                foreach (var t in blueTeam.Where(t => t.GetComponent<DroneLobbyContainer>().myOwnId > 10))
                {
                    t.GetComponent<DroneLobbyContainer>().myOwnId = ownerId;
                    obj = t;
                    break;
                }

                if (obj != null)
                {
                    StartCoroutine(obj.GetComponent<PlayerWorldInformation>().ReverseDissolvePlayer());
                }

                break;
            }
            case TeamState.TeamRed:
            {
                foreach (var t in redTeam.Where(t => t.GetComponent<DroneLobbyContainer>().myOwnId > 10))
                {
                    t.GetComponent<DroneLobbyContainer>().myOwnId = ownerId;
                    obj = t;
                    break;
                }

                if (obj != null)
                {
                    StartCoroutine(obj.GetComponent<PlayerWorldInformation>().ReverseDissolvePlayer());
                }

                break;
            }
            case TeamState.NotSinged:
                break;
            case TeamState.Spectator:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddNameToTeamInfoServerRpc(ulong id, string name)
    {
        AddNameToTeamInfoClientRpc(id, name);
    }

    [ClientRpc]
    private void AddNameToTeamInfoClientRpc(ulong id, string name)
    {
        if (teamInfo.playerNameWIthOwnerId.ContainsKey(id))
        {
            teamInfo.playerNameWIthOwnerId[id] = name;

            instancePlayerIDTogetherWithName[id] = name;
        }
        else
        {
            teamInfo.playerNameWIthOwnerId.Add(id, name);

            if (instancePlayerIDTogetherWithName.ContainsKey(id)) return;


            instancePlayerIDTogetherWithName.Add(id, name);
        }
    }

    // Method for when a client disconnect
    [ServerRpc(RequireOwnership = false)]
    private void RemoveFromTeamServerRpc(ServerRpcParams serverRpcReceiveParams = default)
    {
        var state = TeamState.NotSinged;
        if (teamInfo.playerNameWIthOwnerId.ContainsKey(serverRpcReceiveParams.Receive.SenderClientId))
        {
            teamInfo.playerNameWIthOwnerId.Remove(serverRpcReceiveParams.Receive.SenderClientId);
        }

        if (teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(serverRpcReceiveParams.Receive.SenderClientId))
        {
            state = teamInfo.PlayersTeamStateAndOwnerId[serverRpcReceiveParams.Receive.SenderClientId];
            teamInfo.PlayersTeamStateAndOwnerId.Remove(serverRpcReceiveParams.Receive.SenderClientId);
        }


        RemoveFromTeamClientRpc(serverRpcReceiveParams.Receive.SenderClientId, state);
    }

    //Method for when a client switch team
    [ServerRpc(RequireOwnership = false)]
    private void RemoveFromTeamServerRpc(ulong clinet,
        ServerRpcParams serverRpcReceiveParams = default)
    {
        var state = TeamState.NotSinged;
        if (teamInfo.playerNameWIthOwnerId.ContainsKey(clinet))
        {
            teamInfo.playerNameWIthOwnerId.Remove(clinet);
        }

        if (teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(clinet))
        {
            state = teamInfo.PlayersTeamStateAndOwnerId[clinet];
            teamInfo.PlayersTeamStateAndOwnerId.Remove(clinet);
        }

        RemoveFromTeamClientRpc(clinet, state);
    }

    [ClientRpc]
    private void RemoveFromTeamClientRpc(ulong player, TeamState state)
    {
        if (teamInfo.PlayersTeamStateAndOwnerId.ContainsKey(player))
        {
            teamInfo.PlayersTeamStateAndOwnerId.Remove(player);
        }

        if (teamInfo.playerNameWIthOwnerId.ContainsKey(player))
        {
            teamInfo.playerNameWIthOwnerId.Remove(player);
        }

        SetDissolvePlayer(player, state);
    }

    private void SetDissolvePlayer(ulong player, TeamState state)
    {
        GameObject obj = null;
        switch (state)
        {
            case TeamState.TeamBlue:
            {
                foreach (var t in blueTeam.Where(t => t.GetComponent<DroneLobbyContainer>().myOwnId == player))
                {
                    t.GetComponent<DroneLobbyContainer>().myOwnId = 90;
                    obj = t;
                    obj.GetComponent<DroneLobbyContainer>().textMeshPro.enabled = false;
                    break;
                }

                if (obj != null)
                {
                    StartCoroutine(obj.GetComponent<PlayerWorldInformation>().DissolvePlayer());
                }

                break;
            }
            case TeamState.TeamRed:
            {
                foreach (var t in redTeam.Where(t => t.GetComponent<DroneLobbyContainer>().myOwnId == player))
                {
                    t.GetComponent<DroneLobbyContainer>().myOwnId = 90;
                    obj = t;
                    obj.GetComponent<DroneLobbyContainer>().textMeshPro.enabled = false;
                    break;
                }

                if (obj != null)
                {
                    StartCoroutine(obj.GetComponent<PlayerWorldInformation>().DissolvePlayer());
                }

                break;
            }
            case TeamState.NotSinged:
                break;
            case TeamState.Spectator:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(state), state, null);
        }
    }
}

public enum TeamState
{
    NotSinged,
    Spectator,
    TeamBlue,
    TeamRed
}