using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

namespace CosmicShore.Game.UI
{
    public class LobbyUIMediator : MonoBehaviour
    {
        private const string DEFAULT_LOBBY_NAME = "NoName";

        [Header("Create Lobby")]

        [SerializeField]
        TMP_InputField _lobbyName;

        [Header("Join Lobby")]

        [SerializeField]
        TMP_InputField _lobbyCode;

        public void CreateLobby()
        {

        }

        public async void CreateLobbyRequest(string lobbyName, bool isPrivate)
        {
            // before sending request to lobby service, populate an empty lobby name, if necessary
            if (string.IsNullOrEmpty(lobbyName))
            {
                lobbyName = DEFAULT_LOBBY_NAME;
            }

            // Check if authentication is done or not, we must need to be aunthenticated
            /*if (!_authenticationServiceFacade.IsAuthorizedToAuthenticationService())
            {
                UnblockUIAfterLoadingIsComplete();
                return;
            }*/

            /*(bool Success, Lobby Lobby) lobbyCreationAttempt =
                await _lobbyServiceFacade.TryCreateLobbyAsync(lobbyName, _connectionManager.MaxConnectedPlayers, isPrivate);

            if (lobbyCreationAttempt.Success)
            {
                _localLobbyUser.IsHost = true;
                _lobbyServiceFacade.SetRemoteLobby(lobbyCreationAttempt.Lobby);

                Debug.Log($"Created lobby with ID: {_localLobby.LobbyID} and code {_localLobby.LobbyCode}");
                _connectionManager.StartHostLobby(_localLobbyUser.PlayerName);
            }
            else
            {
                UnblockUIAfterLoadingIsComplete();
            }*/
        }

        public void JoinLobby()
        {

        }
    }
}