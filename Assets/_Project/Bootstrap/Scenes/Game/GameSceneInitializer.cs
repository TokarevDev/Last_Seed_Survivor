using System;
using UnityEngine;
using Zenject;

using Game.Core.Randomization;
using Game.Gameplay.Player;
using Game.Gameplay.Pooling;
using Game.Gameplay.World;

namespace Game.Bootstrap.Scenes.Game
{
    public sealed class GameSceneInitializer : IInitializable
    {
        private readonly Camera _worldCamera;
        private readonly IRandomSource _randomSource;
        private readonly ScreenBoundsService _screenBoundsService;
        private readonly PlayerMovementController _playerMovementController;
        private readonly PoolRegistry _projectilePoolRegistry;
        private readonly AcaciaThornRuntimeInitializer _acaciaThornRuntimeInitializer;
        private readonly PlayerWeaponController _playerWeaponController;

        public GameSceneInitializer(
            Camera worldCamera,
            IRandomSource randomSource,
            ScreenBoundsService screenBoundsService,
            PlayerMovementController playerMovementController,
            PoolRegistry projectilePoolRegistry,
            AcaciaThornRuntimeInitializer acaciaThornRuntimeInitializer,
            PlayerWeaponController playerWeaponController)
        {
            _worldCamera = worldCamera;
            _randomSource = randomSource;
            _screenBoundsService = screenBoundsService;
            _playerMovementController = playerMovementController;
            _projectilePoolRegistry = projectilePoolRegistry;
            _acaciaThornRuntimeInitializer = acaciaThornRuntimeInitializer;
            _playerWeaponController = playerWeaponController;
        }

        public void Initialize()
        {
            ValidateDependencies();

            _screenBoundsService.Recalculate(_worldCamera);
            _playerMovementController.Initialize(_screenBoundsService);
            _projectilePoolRegistry.Init(_screenBoundsService, _randomSource);
            _acaciaThornRuntimeInitializer.Initialize(_screenBoundsService);
            _playerWeaponController.Initialize();
        }

        private void ValidateDependencies()
        {
            if (_worldCamera == null)
                throw new InvalidOperationException("Game world camera is not configured.");

            if (_randomSource == null)
                throw new InvalidOperationException("Random source is not configured.");

            if (_playerMovementController == null)
                throw new InvalidOperationException("Player movement controller is not configured.");

            if (_projectilePoolRegistry == null)
                throw new InvalidOperationException("Projectile pool registry is not configured.");

            if (_acaciaThornRuntimeInitializer == null)
                throw new InvalidOperationException("Acacia Thorn runtime initializer is not configured.");

            if (_playerWeaponController == null)
                throw new InvalidOperationException("Player weapon controller is not configured.");
        }
    }
}
