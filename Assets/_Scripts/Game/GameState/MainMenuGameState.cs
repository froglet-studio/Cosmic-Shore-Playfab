using CosmicShore.Game.UI;
using CosmicShore.Utilities;
using CosmicShore.Utilities.Network;
using PlayFab;
using System;
using Unity.Services.Core;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace CosmicShore.Gameplay.GameState
{
    /// <summary>
    /// Game logic that runs when sitting at the MainMenu. This is likely to be "nothing", as no game has been started.
    /// But it is nonetheless important to have a game state, as the GameStateBehaviour system requires that all scenes have states.
    /// </summary>
    /// <remarks>
    /// OnNetworkSpawn() won't ever run, because there is no network connection at the main menu screen.
    /// Fortunately we know you are a client, because all players are client when sitting at the main menu screen.
    /// </remarks>
    public class MainMenuGameState : GameStateBehaviour
    {
        [SerializeField]
        private LobbyUIMediator _lobbyUIMediator;

        [SerializeField]
        private GameObject _signInSpinner;

        [Inject]
        private SceneNameListSO _sceneNameList;

        private LocalLobbyUser _localLobbyUser;
        private LocalLobby _localLobby;

        public override GameState ActiveState => GameState.MainMenu;

        private string _profileName;
        
        
        protected override void Awake()
        {
            base.Awake();
            _signInSpinner.SetActive(false);
        }

        protected override async void Start()
        {
            base.Start();

            string savedName = PlayFabClientAPI.IsClientLoggedIn() ? "Yash" : "No-name";

            _localLobbyUser.PlayerName = savedName;

            // The local lobby user object will be hooked into UI before the LocalLobby is populated during lobby join, so the LocalLobby must know about it already
            // when that happens.
            _localLobby.AddUser(_localLobbyUser);
        }

        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            builder.RegisterComponent(_lobbyUIMediator);
        }

        [Inject]
        private void AddDependencies(
            LocalLobbyUser localLobbyUser,
            LocalLobby localLobby)
        {
            _localLobbyUser = localLobbyUser;
            _localLobby = localLobby;
        }
    }
}
